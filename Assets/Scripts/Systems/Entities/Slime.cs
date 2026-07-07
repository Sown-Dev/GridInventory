using Systems.Entities;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SlimeEnemy : ContactDamageUnit
{
    [Header("Slime Movement")]
    [SerializeField] private float jumpForceForward = 4f;
    [SerializeField] private float jumpForceUpward = 6f;
    [SerializeField] private float jumpInterval = 2f;
    
    [Header("Counter-Jump Settings")]
    [SerializeField] private float counterJumpDetectionRange = 5f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Transform target;
    private float jumpTimer;
    private bool isGrounded;

    public override void Start()
    {
        // Inherit base health setup and flash materials from Damageable
        base.Start(); 
        
        rb = GetComponent<Rigidbody2D>();

        // Automatically target the player singleton transform on awake/start
        if (Player.instance != null)
        {
            target = Player.instance.transform;
        }
    }

    public override void Update()
    {
        // Keep the base flash decay running
        base.Update(); 

        if (target == null) return;

        HandleFacingDirection();
        CheckGroundStatus();
        HandleJumpTimers();
    }

    private void HandleFacingDirection()
    {
        // Only change facing direction when firmly on the ground
        if (!isGrounded) return;

        // Flip the sprite scale to look toward the target
        if (target.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (target.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    private void CheckGroundStatus()
    {
        if (groundCheckPoint != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        }
    }

    private void HandleJumpTimers()
    {
        if (!isGrounded) return;

        jumpTimer += Time.deltaTime;

        // Check if the specialized counter-jump condition is met
        if (CanCounterJump())
        {
            ExecuteJump();
            return;
        }

        // Standard rhythmic jump interval
        if (jumpTimer >= jumpInterval)
        {
            ExecuteJump();
        }
    }

    private bool CanCounterJump()
    {
        // Must be at least halfway through the normal jump cooldown
        if (jumpTimer < (jumpInterval * 0.5f)) return false;

        Vector2 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;

        if (distance > counterJumpDetectionRange) return false;

        // Verify the target is vertically above the slime
        bool isAbove = target.position.y > transform.position.y;
        if (!isAbove) return false;

        // Verify the target is horizontally in front of where the slime is facing
        float facingDirection = transform.localScale.x; 
        bool isInFront = (facingDirection > 0 && toTarget.x > 0) || (facingDirection < 0 && toTarget.x < 0);

        return isInFront;
    }

    private void ExecuteJump()
    {
        jumpTimer = 0f;

        // Determine horizontal jump direction based on current facing scale
        float directionX = transform.localScale.x > 0 ? 1f : -1f;

        // Reset current vertical velocity to ensure clean, consistent jump forces
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        
        rb.AddForce(new Vector2(directionX * jumpForceForward, jumpForceUpward), ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        // Visual aid inside the Unity editor for positioning your ground check circle
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}