using UnityEngine;

public class LookAt : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform to look at, e.g. the Crosshair.")]
    public Transform target;

    [Header("Angle Limits")]
    [Tooltip("Limits are relative to local rest orientation (0 = facing this object's local +X, i.e. straight ahead when unmirrored).")]
    public float minAngle = -60f;
    public float maxAngle = 60f;

    [Tooltip("Look away from target instead of toward it (adds 180 degrees before clamping).")]
    public bool invert = false;

    [Header("Smoothing")]
    [Tooltip("Degrees/sec the look rotation can turn. Set to 0 for instant snapping.")]
    public float rotationSpeed = 720f;

    private float currentLocalAngle;

    void Update()
    {
        if (target == null)
        {
            return;
        }

        // Compute the target's position in the PARENT's local space, rather than working in
        // world space. InverseTransformPoint divides out the parent's full matrix, including
        // scale sign — so if something up the hierarchy is flipped via a negative localScale.x
        // (a mirror), this automatically un-mirrors the direction for us. Working in world
        // space and assigning transform.rotation directly (the old approach) skips this
        // correction entirely, which is why it came out reflected/looking away.
        Vector3 targetLocal = transform.parent != null
            ? transform.parent.InverseTransformPoint(target.position)
            : transform.InverseTransformPoint(target.position) + transform.localPosition;

        Vector2 toTargetLocal = (Vector2)targetLocal - (Vector2)transform.localPosition;
        float localAngle = Mathf.Atan2(toTargetLocal.y, toTargetLocal.x) * Mathf.Rad2Deg;

        if (invert)
        {
            localAngle += 180f;
        }

        // Clamp is now a straightforward local-space clamp — no need to track the parent's
        // base angle separately, since InverseTransformPoint already factored parent rotation
        // (and the mirror) out for us.
        float clampedLocalAngle = Mathf.Clamp(NormalizeAngle(localAngle), minAngle, maxAngle);

        if (rotationSpeed <= 0f)
        {
            currentLocalAngle = clampedLocalAngle;
        }
        else
        {
            currentLocalAngle = Mathf.MoveTowardsAngle(currentLocalAngle, clampedLocalAngle, rotationSpeed * Time.deltaTime);
        }

        transform.localRotation = Quaternion.Euler(0f, 0f, currentLocalAngle);
    }

    // Wraps to -180..180 so Clamp behaves correctly across the wraparound point.
    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }
}