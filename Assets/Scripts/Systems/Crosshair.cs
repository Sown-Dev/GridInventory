using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public static Crosshair instance;

    // Raw mouse world position, unaffected by recoil — for UI, click/drag targeting, etc.
    public Vector3 CursorWorldPosition { get; private set; }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Hides the default OS mouse cursor so the reticle sprite replaces it — same as the original script.
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 mouseWorld = Camera.main != null
            ? Camera.main.ScreenToWorldPoint(Input.mousePosition)
            : Vector3.zero;
        mouseWorld.z = 0f;
        CursorWorldPosition = mouseWorld;

        if (Player.instance == null || Camera.main == null)
        {
            transform.position = mouseWorld;
            return;
        }

        Vector2 origin = Player.instance.AimOrigin;
        Vector2 toMouse = (Vector2)mouseWorld - origin;
        float baseDistance = toMouse.magnitude;

        Vector2 recoiledDir = Player.instance.RecoiledAimDirection;
        float recoiledDistance = baseDistance + Player.instance.RecoilKickback;

        Vector3 crosshairPos = origin + recoiledDir * recoiledDistance;
        crosshairPos.z = 0f;

        transform.position = crosshairPos;
    }
}