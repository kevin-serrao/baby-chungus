using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;


public class PlayerController : MonoBehaviour {

    public int health = 3;
    private bool isInvincible = false;
    public float invincibleTime = 1.0f;

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        health -= damage;
        Debug.Log($"[Player] Took {damage} damage. Health: {health}");
        // Visual feedback
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
            Invoke(nameof(ResetColor), 0.2f);
        }
        isInvincible = true;
        Invoke(nameof(ResetInvincibility), invincibleTime);
        if (health <= 0)
        {
            Die();
        }
    }

    private void ResetInvincibility()
    {
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("[Player] Died!");
        // TODO: Add death logic (respawn, game over, etc.)
    }

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
    private bool canAttack = true;
    public float attackCooldown = 0.5f;
    public float attackRange = 1.5f;
    public float attackRadius = 1f;
    public Transform attackPointTransform;
    public Color attackGizmoColor = Color.red;
    public Color attackMarkerColor = new Color(1f, 0f, 0f, 0.35f);
    public float attackMarkerDuration = 0.15f;

    public bool useRuntimeStickFigure = true;
    public int stickFigureSpriteWidth = 64;
    public int stickFigureSpriteHeight = 96;
    public Color stickFigureColor = Color.white;
    public Color stickFigureOutlineColor = Color.black;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D attackPointCollider;
    private GameObject attackMarker;
    private SpriteRenderer attackMarkerRenderer;
    private Vector2 lastAttackPosition;

    private int facingDirection = 1; // 1 = right, -1 = left

    private void Awake()
    {
        controls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (useRuntimeStickFigure && spriteRenderer != null)
        {
            spriteRenderer.sprite = CreateStickFigureSprite(stickFigureSpriteWidth, stickFigureSpriteHeight, stickFigureColor, stickFigureOutlineColor);
            spriteRenderer.color = Color.white;
        }

        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Gameplay.Jump.performed += ctx => TryJump();
        controls.Gameplay.Strike.performed += ctx => TryAttack();
    }

    private void Start()
    {
        if (attackPointTransform == null)
        {
            Transform found = transform.Find("AttackPoint");
            if (found != null)
                attackPointTransform = found;
        }

        if (attackPointTransform != null)
        {
            attackPointCollider = attackPointTransform.GetComponent<Collider2D>();
            if (attackPointCollider != null)
                attackPointCollider.enabled = false;
        }

        CreateAttackMarker();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();


    void FixedUpdate()
    {
        HandleMovement();
        HandleBetterJump();
        UpdateFacing();
    }


    void HandleMovement()
    {
        float targetSpeed = moveInput.x * maxSpeed;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

        // Update facing direction if moving
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            facingDirection = moveInput.x > 0 ? 1 : -1;
        }
    }

    void UpdateFacing()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (facingDirection == -1);
        }
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

    void TryAttack()
    {
        if (!canAttack) return;

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        else
        {
            // Fallback: simple programmatic swing animation
            StartCoroutine(AttackSwing());
        }

        // Visual feedback - flash red
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            Invoke(nameof(ResetColor), 0.1f);
        }

        // Simple melee attack - check for enemies in front

        Vector2 attackDirection = facingDirection == 1 ? Vector2.right : Vector2.left;
        lastAttackPosition = (Vector2)transform.position + attackDirection * attackRange;

        if (attackPointTransform != null)
        {
            attackPointTransform.position = lastAttackPosition;
            if (attackPointCollider != null)
                attackPointCollider.enabled = false;
        }

        ShowAttackMarker(lastAttackPosition, attackRadius);
        Debug.DrawLine(transform.position, lastAttackPosition, Color.yellow, attackMarkerDuration);


        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(lastAttackPosition, attackRadius);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // Damage the enemy
                BotScript bot = enemy.GetComponent<BotScript>();
                if (bot != null)
                {
                    bot.TakeDamage(1);
                    // Bat the bot away using BatAway method
                    float direction = Mathf.Sign(enemy.transform.position.x - transform.position.x);
                    Vector2 batForce = new Vector2(direction * 16f, 8f); // strong impulse, tune as needed
                    bot.BatAway(batForce);
                }
            }
        }

        // Attack cooldown
        canAttack = false;
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    IEnumerator AttackSwing()
    {
        // Simple swing animation - rotate player
        float originalRotation = transform.eulerAngles.z;
        float swingAngle = transform.localScale.x > 0 ? -45f : 45f; // Swing opposite to facing direction
        
        // Swing forward
        float elapsed = 0f;
        float duration = 0.1f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentAngle = Mathf.Lerp(originalRotation, originalRotation + swingAngle, t);
            transform.eulerAngles = new Vector3(0, 0, currentAngle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Swing back
        elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentAngle = Mathf.Lerp(originalRotation + swingAngle, originalRotation, t);
            transform.eulerAngles = new Vector3(0, 0, currentAngle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Reset to original
        transform.eulerAngles = new Vector3(0, 0, originalRotation);
    }

    private void CreateAttackMarker()
    {
        attackMarker = new GameObject("AttackMarker");
        attackMarker.transform.SetParent(transform);
        attackMarker.transform.localPosition = Vector3.zero;
        attackMarker.transform.localRotation = Quaternion.identity;
        attackMarker.transform.localScale = Vector3.one;

        attackMarkerRenderer = attackMarker.AddComponent<SpriteRenderer>();
        attackMarkerRenderer.sprite = CreateCircleSprite(16, Color.white);
        attackMarkerRenderer.color = attackMarkerColor;
        attackMarkerRenderer.sortingOrder = 100;
        attackMarker.SetActive(false);
    }

    private Sprite CreateCircleSprite(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        Color transparent = new Color(color.r, color.g, color.b, 0f);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, dist <= radius ? color : transparent);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite CreateStickFigureSprite(int width, int height, Color fillColor, Color outlineColor)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        Vector2 headCenter = new Vector2(width * 0.5f, height - height * 0.18f);
        float headRadius = width * 0.12f;
        DrawCircle(texture, headCenter, headRadius, outlineColor, fillColor);

        Vector2 torsoTop = new Vector2(width * 0.5f, height - height * 0.28f);
        Vector2 torsoBottom = new Vector2(width * 0.5f, height * 0.45f);
        DrawLine(texture, torsoTop, torsoBottom, outlineColor);

        Vector2 leftArm = new Vector2(width * 0.18f, height * 0.55f);
        Vector2 rightArm = new Vector2(width * 0.82f, height * 0.55f);
        DrawLine(texture, torsoTop, leftArm, outlineColor);
        DrawLine(texture, torsoTop, rightArm, outlineColor);

        Vector2 leftLeg = new Vector2(width * 0.28f, height * 0.14f);
        Vector2 rightLeg = new Vector2(width * 0.72f, height * 0.14f);
        DrawLine(texture, torsoBottom, leftLeg, outlineColor);
        DrawLine(texture, torsoBottom, rightLeg, outlineColor);

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private void DrawPixel(Texture2D texture, int x, int y, Color color)
    {
        if (x < 0 || x >= texture.width || y < 0 || y >= texture.height) return;
        texture.SetPixel(x, y, color);
    }

    private void DrawLine(Texture2D texture, Vector2 a, Vector2 b, Color color)
    {
        int steps = Mathf.CeilToInt(Vector2.Distance(a, b));
        for (int i = 0; i <= steps; i++)
        {
            float t = steps == 0 ? 0f : (float)i / steps;
            Vector2 point = Vector2.Lerp(a, b, t);
            DrawPixel(texture, Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), color);
            DrawPixel(texture, Mathf.RoundToInt(point.x) + 1, Mathf.RoundToInt(point.y), color);
            DrawPixel(texture, Mathf.RoundToInt(point.x) - 1, Mathf.RoundToInt(point.y), color);
            DrawPixel(texture, Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y) + 1, color);
            DrawPixel(texture, Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y) - 1, color);
        }
    }

    private void DrawCircle(Texture2D texture, Vector2 center, float radius, Color outlineColor, Color fillColor)
    {
        int intRadius = Mathf.CeilToInt(radius);
        for (int y = -intRadius; y <= intRadius; y++)
        {
            for (int x = -intRadius; x <= intRadius; x++)
            {
                float dist = Mathf.Sqrt(x * x + y * y);
                if (dist <= radius)
                {
                    Color color = dist > radius - 1.5f ? outlineColor : fillColor;
                    DrawPixel(texture, Mathf.RoundToInt(center.x + x), Mathf.RoundToInt(center.y + y), color);
                }
            }
        }
    }

    private void ShowAttackMarker(Vector2 position, float radius)
    {
        if (attackMarker == null) return;

        attackMarker.transform.position = position;
        float scale = radius * 2f / (16f / 100f);
        attackMarker.transform.localScale = new Vector3(scale, scale, 1f);
        attackMarker.SetActive(true);
        CancelInvoke(nameof(HideAttackMarker));
        Invoke(nameof(HideAttackMarker), attackMarkerDuration);
    }

    private void HideAttackMarker()
    {
        if (attackMarker != null)
            attackMarker.SetActive(false);
    }

    void ResetAttack()
    {
        canAttack = true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        isGrounded = true;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Horizontal knockback and freeze rotation on enemy contact
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (rb != null)
            {
                float direction = Mathf.Sign(transform.position.x - collision.transform.position.x);
                rb.linearVelocity = new Vector2(direction * 10f, rb.linearVelocity.y);
                rb.angularVelocity = 0f;
                rb.rotation = 0f;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    private void OnDrawGizmos()
    {
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * attackRange;

        Gizmos.color = attackGizmoColor;
        Gizmos.DrawLine(transform.position, attackPosition);
        Gizmos.DrawWireSphere(attackPosition, attackRadius);

        if (Application.isPlaying)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawSphere(lastAttackPosition, attackRadius);
        }
    }
}