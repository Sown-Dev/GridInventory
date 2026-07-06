using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Drone : MonoBehaviour
{
    [Header("Drone Setup")]
    [Tooltip("Position of thruster A relative to drone center")]
    public Vector2 thrusterAOffset = new Vector2(-0.5f, 0f);
    
    [Tooltip("Position of thruster B relative to drone center")]
    public Vector2 thrusterBOffset = new Vector2(0.5f, 0f);
    
    [Header("Thruster Configuration")]
    [Tooltip("Throttle range: negative = downward force")]
    public float maxThrottle = 1.5f;
    public float minThrottle = -0.5f;
    
    private float baseForcePerThruster;
    
    [Header("Target")]
    [Tooltip("If assigned, the drone will follow this transform continuously.")]
    public Transform targetTransform;
    [Tooltip("Fallback target position used if targetTransform is null.")]
    public Vector2 targetPosition = new Vector2(0f, 5f);

    [Header("MISC")] 
    public ParticleSystem thruster1;
    public ParticleSystem thruster2;
    public Rigidbody2D rb;
    
    [Header("PID Tuning - Altitude")]
    public float altitudeKp = 0.8f;
    public float altitudeKi = 0.02f;
    public float altitudeKd = 1.2f;
    public float altitudeIntegralMax = 5f;
    
    [Header("PID Tuning - Rotation (Attitude)")]
    public float rotationKp = 5f;
    public float rotationKi = 0.0f;
    public float rotationKd = 4f;
    public float rotationIntegralMax = 2f;
    
    [Header("PID Tuning - Horizontal Position")]
    public float horizontalKp = 1.5f;
    public float horizontalKi = 0.0f;
    public float horizontalKd = 2.0f;
    public float horizontalIntegralMax = 3f;
    
    [Header("Control Limits")]
    [Tooltip("Maximum tilt angle for horizontal movement (degrees)")]
    public float maxTiltAngle = 30f;
    
    [Tooltip("Maximum rotation correction throttle difference")]
    public float maxRotationThrottle = 0.25f;
    
    [Tooltip("Maximum altitude correction throttle adjustment")]
    public float maxAltitudeThrottle = 0.4f;
    
    [Tooltip("Deadzone for position - stops trying to reach target within this distance")]
    public float positionDeadzone = 0.1f;
    
    [Tooltip("Low-pass filter coefficient for derivative smoothing (0-1)")]
    public float derivativeAlpha = 0.3f;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool logThrottle = false;
    public bool enableAltitudeControl = true;
    public bool enableRotationControl = true;
    public bool enableHorizontalControl = false;
    
    // PID State - Altitude
    private float altitudeIntegral = 0f;
    private float altitudePreviousError = 0f;
    private float altitudePreviousDerivative = 0f;
    
    // PID State - Rotation
    private float rotationIntegral = 0f;
    private float rotationPreviousError = 0f;
    private float rotationPreviousDerivative = 0f;
    
    // PID State - Horizontal
    private float horizontalIntegral = 0f;
    private float horizontalPreviousError = 0f;
    private float horizontalPreviousDerivative = 0f;
    
    // Output State
    private float thrusterAThrottle = 0f;
    private float thrusterBThrottle = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        baseForcePerThruster = (rb.mass * Mathf.Abs(Physics2D.gravity.y)) / 2f;
        
        Debug.Log($"Drone initialized. Mass: {rb.mass}kg, Base force per thruster: {baseForcePerThruster}N");
    }
    
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        if (dt < 0.0001f) dt = 0.0001f;
        
        // === Determine Active Target ===
        Vector2 activeTargetPos = targetTransform != null ? (Vector2)targetTransform.position : targetPosition;

        // === Calculate errors ===
        float altitudeError = activeTargetPos.y - rb.position.y;
        float horizontalError = activeTargetPos.x - rb.position.x;
        
        if (Mathf.Abs(horizontalError) < positionDeadzone) horizontalError = 0f;
        if (Mathf.Abs(altitudeError) < positionDeadzone) altitudeError = 0f;
        
        // === Calculate desired tilt for horizontal movement ===
        float desiredTilt = 0f;
        if (enableHorizontalControl)
        {
            // Horizontal PID outputs desired tilt in DEGREES
            // Positive error (target is to the RIGHT) should produce NEGATIVE tilt (lean right)
            // This is because in Unity 2D: negative rotation = clockwise = lean right
            float horizontalPIDOutput = UpdatePID(
                horizontalError, dt,
                horizontalKp, horizontalKi, horizontalKd,
                ref horizontalIntegral, horizontalIntegralMax,
                ref horizontalPreviousError, ref horizontalPreviousDerivative
            );
            
            // INVERTED: Positive error (target right) needs negative tilt (lean right)
            desiredTilt = -horizontalPIDOutput;
            desiredTilt = Mathf.Clamp(desiredTilt, -maxTiltAngle, maxTiltAngle);
        }
        
        float currentRotation = rb.rotation;
        float rotationError = Mathf.DeltaAngle(currentRotation, desiredTilt);
        
        // === Run PID controllers ===
        float altitudeAdjustment = 0f;
        if (enableAltitudeControl)
        {
            altitudeAdjustment = UpdatePID(
                altitudeError, dt,
                altitudeKp, altitudeKi, altitudeKd,
                ref altitudeIntegral, altitudeIntegralMax,
                ref altitudePreviousError, ref altitudePreviousDerivative
            );
            altitudeAdjustment = Mathf.Clamp(altitudeAdjustment, -maxAltitudeThrottle, maxAltitudeThrottle);
        }
        
        float rotationAdjustment = 0f;
        if (enableRotationControl)
        {
            rotationAdjustment = UpdatePID(
                rotationError, dt,
                rotationKp, rotationKi, rotationKd,
                ref rotationIntegral, rotationIntegralMax,
                ref rotationPreviousError, ref rotationPreviousDerivative
            );
            rotationAdjustment = Mathf.Clamp(rotationAdjustment, -maxRotationThrottle, maxRotationThrottle);
        }
        
        // === Calculate thruster throttles ===
        float tiltRadians = currentRotation * Mathf.Deg2Rad;
        float tiltCompensation = 1f / Mathf.Max(Mathf.Cos(tiltRadians), 0.7f) - 1f;
        float baseThrottle = 1.0f + tiltCompensation * 0.3f;
        float totalThrottle = baseThrottle + altitudeAdjustment;
        
        thrusterAThrottle = Mathf.Clamp(totalThrottle + rotationAdjustment, minThrottle, maxThrottle);
        thrusterBThrottle = Mathf.Clamp(totalThrottle - rotationAdjustment, minThrottle, maxThrottle);
        
        // === Apply forces ===
        ApplyThrusterForce(thrusterAOffset, thrusterAThrottle);
        ApplyThrusterForce(thrusterBOffset, thrusterBThrottle);
        
        // particle effects
        var em = thruster2.emission;
        em.rateOverTime = Mathf.Pow(thrusterBThrottle, 2) * 10f;
        
        var em1 = thruster1.emission;
        em1.rateOverTime = Mathf.Pow(thrusterAThrottle, 2) * 10f;
        
        if (logThrottle)
        {
            Debug.Log($"Alt Err: {altitudeError:F2}m, Rot Err: {rotationError:F2}°, Horiz Err: {horizontalError:F2}m, " +
                      $"Desired Tilt: {desiredTilt:F1}°, Current Rot: {currentRotation:F1}°, " +
                      $"Throttle A: {thrusterAThrottle:F2}, B: {thrusterBThrottle:F2}");
        }
    }
    
    private float UpdatePID(float error, float deltaTime, 
        float kp, float ki, float kd,
        ref float integral, float integralMax,
        ref float previousError, ref float previousDerivative)
    {
        float p = kp * error;
        
        integral += error * deltaTime;
        integral = Mathf.Clamp(integral, -integralMax, integralMax);
        float i = ki * integral;
        
        float rawDerivative = (error - previousError) / deltaTime;
        float smoothedDerivative = derivativeAlpha * rawDerivative + (1f - derivativeAlpha) * previousDerivative;
        float d = kd * smoothedDerivative;
        
        previousError = error;
        previousDerivative = smoothedDerivative;
        
        return p + i + d;
    }
    
    private void ApplyThrusterForce(Vector2 localOffset, float throttle)
    {
        Vector2 thrusterWorldPos = rb.position + (Vector2)(transform.rotation * localOffset);
        float forceMagnitude = baseForcePerThruster * throttle;
        Vector2 force = (Vector2)transform.up * forceMagnitude;
        rb.AddForceAtPosition(force, thrusterWorldPos);
    }
    
    public void SetTargetPosition(Vector2 newTarget)
    {
        targetTransform = null; // Clear the transform so the Vector2 takes over
        targetPosition = newTarget;
    }

    public void SetTargetTransform(Transform newTargetTransform)
    {
        targetTransform = newTargetTransform;
    }
    
    public void ResetControllers()
    {
        altitudeIntegral = 0f;
        altitudePreviousError = 0f;
        altitudePreviousDerivative = 0f;
        
        rotationIntegral = 0f;
        rotationPreviousError = 0f;
        rotationPreviousDerivative = 0f;
        
        horizontalIntegral = 0f;
        horizontalPreviousError = 0f;
        horizontalPreviousDerivative = 0f;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Vector2 activeTargetPos = targetTransform != null ? (Vector2)targetTransform.position : targetPosition;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(activeTargetPos, 0.3f);
        Gizmos.DrawLine(activeTargetPos + Vector2.left * 0.5f, activeTargetPos + Vector2.right * 0.5f);
        Gizmos.DrawLine(activeTargetPos + Vector2.up * 0.5f, activeTargetPos + Vector2.down * 0.5f);
        
        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(rb.position, activeTargetPos);
            
            Vector2 thrusterAPos = rb.position + (Vector2)(transform.rotation * thrusterAOffset);
            Vector2 thrusterBPos = rb.position + (Vector2)(transform.rotation * thrusterBOffset);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(thrusterAPos, 0.1f);
            Gizmos.DrawLine(thrusterAPos, thrusterAPos + (Vector2)transform.up * (thrusterAThrottle * 0.5f));
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(thrusterBPos, 0.1f);
            Gizmos.DrawLine(thrusterBPos, thrusterBPos + (Vector2)transform.up * (thrusterBThrottle * 0.5f));
        }
    }
}