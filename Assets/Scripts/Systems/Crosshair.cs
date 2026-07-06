using UnityEngine;
using UnityEngine.U2D;

public class Crosshair : MonoBehaviour
{
    public static Crosshair instance;

    [Header("Spread Sizing")]
    [Tooltip("Must be a SpriteRenderer with Draw Mode set to Sliced, so resizing doesn't stretch the art.")]
    public SpriteRenderer crosshairSpriteRenderer;
    public float minCrosshairSize = 0.15f;
    public float crosshairSizeMultiplier = 0.5f;

    [Header("Pixel Perfect")]
    [Tooltip("Assign the scene's Pixel Perfect Camera. If left empty, Awake() will try to find one on Camera.main.")]
    public PixelPerfectCamera pixelPerfectCamera;

    public Vector3 CursorWorldPosition { get; private set; }

    void Awake()
    {
        instance = this;

        if (pixelPerfectCamera == null && Camera.main != null)
        {
            pixelPerfectCamera = Camera.main.GetComponent<PixelPerfectCamera>();
        }
    }

    // Moved from Update to LateUpdate on purpose: this reads Player.RecoiledAimDirection,
    // which Player computes in its own Update(). Unity doesn't guarantee Update() ordering
    // between different scripts, so on frames where this ran first we'd get a stale direction
    // paired with a fresh distance — a one-frame mismatch that shows up as jitter whenever the
    // player moves. LateUpdate() is guaranteed to run after every script's Update(), so this
    // always sees this frame's direction.
    void LateUpdate()
    {
        Vector3 mouseWorld = Camera.main != null
            ? Camera.main.ScreenToWorldPoint(Input.mousePosition)
            : Vector3.zero;
        mouseWorld.z = 0f;
        CursorWorldPosition = mouseWorld;

        if (Player.instance == null || Camera.main == null)
        {
            transform.position = SnapToPixelGrid(mouseWorld);
            return;
        }

        Vector2 origin = Player.instance.AimOrigin;
        Vector2 toMouse = (Vector2)mouseWorld - origin;
        float baseDistance = toMouse.magnitude;

        Vector2 recoiledDir = Player.instance.RecoiledAimDirection;
        float recoiledDistance = baseDistance + Player.instance.RecoilKickback;

        Vector3 crosshairPos = origin + recoiledDir * recoiledDistance;
        crosshairPos.z = 0f;
        transform.position = SnapToPixelGrid(crosshairPos);

        UpdateCrosshairSize(recoiledDistance);
    }

    // Rounds a world position to the nearest pixel using the Pixel Perfect Camera's
    // assets-per-unit setting, so the crosshair always lands on the same grid the
    // rest of the pixel-perfect rendering snaps to.
    private Vector3 SnapToPixelGrid(Vector3 worldPos)
    {
        float ppu = pixelPerfectCamera != null ? pixelPerfectCamera.assetsPPU : 0f;
        if (ppu <= 0f) return worldPos;

        return new Vector3(
            Mathf.Round(worldPos.x * ppu) / ppu,
            Mathf.Round(worldPos.y * ppu) / ppu,
            worldPos.z
        );
    }

    private void UpdateCrosshairSize(float distance)
    {
        if (crosshairSpriteRenderer == null)
        {
            return;
        }

        float halfAngleRadians = Player.instance.CurrentSpreadDegrees * Mathf.Deg2Rad;
        float coneDiameter = 2f * distance * Mathf.Tan(halfAngleRadians);

        float size = Mathf.Max(minCrosshairSize, coneDiameter * crosshairSizeMultiplier);
        crosshairSpriteRenderer.size = new Vector2(size, size);
    }
}