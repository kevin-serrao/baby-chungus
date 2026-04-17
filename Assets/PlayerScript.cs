using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerControls controls;
    private Vector2 moveInput;

    private Rigidbody2D rb;


    public float maxSpeed = 11f;
    public float acceleration = 60f;
    public float deceleration = 80f;
    public float jumpForce = 14f;

    
    public float fallGravityMultiplier = 1f;
    public float riseGravityMultiplier = 0.33f;

    private bool isGrounded;

    private void Awake()
    {
        controls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();

        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Gameplay.Jump.performed += ctx => TryJump();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void FixedUpdate()
    {
        HandleMovement();
        HandleBetterJump();
    }

    void HandleMovement()
    {
        float targetSpeed = moveInput.x * maxSpeed;

        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    void HandleBetterJump()
    {
        if (rb.linearVelocity.y > 0)
        {
            rb.gravityScale = riseGravityMultiplier;
        }
        else
        {
            rb.gravityScale = fallGravityMultiplier;
        }
    }

    void TryJump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}