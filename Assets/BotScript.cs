using UnityEngine;

public class BotScript : MonoBehaviour {
    // Each bot gets a unique horizontal offset for hovering
    private float hoverHorizontalOffset = 0f;

    private enum BotPhase { Hover, Buzz, Attack, Stunned, Recovering }
    private BotPhase phase = BotPhase.Hover;
        private float stunnedDuration = 1.0f; // seconds
        private float stunnedTimer = 0f;
    private float phaseTimer = 0f;
    private GameObject playerTarget;

    public void EnterHoverPhase(GameObject player)
    {
        // Assign a random horizontal offset for this bot (between -60 and 60 units)
        hoverHorizontalOffset = UnityEngine.Random.Range(-60f, 60f);
        Debug.Log($"[Bot] EnterHoverPhase called. Player: {(player != null ? player.name : "null")}");
        phase = BotPhase.Hover;
        phaseTimer = 0f;
        playerTarget = player;
        if (myRigidBody != null)
        {
            myRigidBody.gravityScale = 0.2f; // much lighter
            myRigidBody.linearVelocity = Vector2.zero;
            myRigidBody.constraints = RigidbodyConstraints2D.None;
            myRigidBody.bodyType = RigidbodyType2D.Dynamic;
            myRigidBody.mass = 0.2f; // much lighter
            myRigidBody.linearDamping = 0.01f; // almost no drag
            myRigidBody.angularDamping = 0.01f;
            Debug.Log($"[Bot] Rigidbody2D state: bodyType={myRigidBody.bodyType}, constraints={myRigidBody.constraints}, gravityScale={myRigidBody.gravityScale}, mass={myRigidBody.mass}, drag={myRigidBody.linearDamping}, angularDrag={myRigidBody.angularDamping}");
        }
    }
    
    public float acceleration = (float)0.8;
    public Rigidbody2D myRigidBody;
    public float moveFrequency;
    public float secondsSinceLastMove = 0;
    public LogicManager logic;
    public int health = 9999; // effectively invincible
    public bool invincible = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManager>();
    }

    void OnEnable()
    {
        BeatConductor.OnBeatGlobal += OnBeat;
    }

    void OnDisable()
    {
        BeatConductor.OnBeatGlobal -= OnBeat;
    }

    void OnBeat(int beatIndex, float songTime)
    {
        // React to the beat
        wiggle();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"[Bot] Phase: {phase}, Position: {transform.position}, Velocity: {(myRigidBody != null ? myRigidBody.linearVelocity.ToString() : "null")}");
        if (myRigidBody != null)
        {
            Debug.Log($"[Bot] Phase: {phase}, Position: {transform.position}, Velocity: {myRigidBody.linearVelocity}, bodyType: {myRigidBody.bodyType}, simulated: {myRigidBody.simulated}, constraints: {myRigidBody.constraints}");
        }
        else
        {
            Debug.Log($"[Bot] Phase: {phase}, Position: {transform.position}, Rigidbody2D is null");
        }
        // Bot phase logic
        switch (phase)
        {
            case BotPhase.Hover:
                HoverBehavior();
                break;
            case BotPhase.Buzz:
                BuzzBehavior();
                break;
            case BotPhase.Attack:
                AttackBehavior();
                break;
            case BotPhase.Stunned:
                StunnedBehavior();
                break;
        }

        // (Legacy movement, not used in new phases)
        //if (secondsSinceLastMove < moveFrequency)
        //{ 
        //    secondsSinceLastMove += Time.deltaTime;
        //} else { 
        //    Vector2 randomDirection = Random.insideUnitCircle.normalized;
        //    myRigidBody.linearVelocity += randomDirection * acceleration;
        //    secondsSinceLastMove = 0;
        //}

    }

    void HoverBehavior()
    {
        Debug.Log("[Bot] HoverBehavior running");
        phaseTimer += Time.deltaTime;
        // Hover up and down farther above player
        if (playerTarget != null && myRigidBody != null)
        {
            // Add horizontal offset and some per-bot randomness to the hover position
            float separation = hoverHorizontalOffset;
            float extraWobble = Mathf.Sin(Time.time * 8f + separation) * 4.5f;
            Vector3 hoverPos = playerTarget.transform.position + new Vector3(separation, 18f, 0);
            float hoverOffset = Mathf.Sin(Time.time * 10f + separation) * 3.5f + extraWobble;
            Vector3 targetPos = hoverPos + new Vector3(0, hoverOffset, 0);
            Vector2 toTarget = (targetPos - transform.position);
            // Use AddForce for smooth acceleration
            float accel = 3.5f; // much lower for smoothness
            // Clamp target position to screen bounds
            Vector3 clampedTarget = ClampToScreen(targetPos);
            Vector2 clampedToTarget = (clampedTarget - transform.position);
            myRigidBody.AddForce(clampedToTarget.normalized * accel, ForceMode2D.Force);
            Debug.Log($"[Bot] HoverBehavior applied force: {clampedToTarget.normalized * accel}");
        }
        if (phaseTimer > 0.7f)
        {
            phase = BotPhase.Buzz;
            phaseTimer = 0f;
        }
    }

    void BuzzBehavior()
    {
        Debug.Log("[Bot] BuzzBehavior running");
        phaseTimer += Time.deltaTime;
        // Shake left/right quickly to "buzz" much farther away
        if (playerTarget != null && myRigidBody != null)
        {
            float separation = hoverHorizontalOffset;
            float buzzWobble = Mathf.Sin(Time.time * 60f + separation) * 8.5f;
            Vector3 hoverPos = playerTarget.transform.position + new Vector3(separation, 18f, 0);
            float buzzOffset = buzzWobble;
            Vector3 targetPos = hoverPos + new Vector3(buzzOffset, 0, 0);
            Vector2 toTarget = (targetPos - transform.position);
            // Use AddForce for smooth acceleration
            float accel = 6f; // much lower for smoothness
            // Clamp target position to screen bounds
            Vector3 clampedTarget = ClampToScreen(targetPos);
            Vector2 clampedToTarget = (clampedTarget - transform.position);
            myRigidBody.AddForce(clampedToTarget.normalized * accel, ForceMode2D.Force);
            Debug.Log($"[Bot] BuzzBehavior applied force: {clampedToTarget.normalized * accel}");
        }
        if (phaseTimer > 0.3f)
        {
            phase = BotPhase.Attack;
            phaseTimer = 0f;
        }
    }

    // Clamp a world position to the screen bounds (orthographic camera)
    private Vector3 ClampToScreen(Vector3 worldPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return worldPos;
        Vector3 viewport = cam.WorldToViewportPoint(worldPos);
        viewport.x = Mathf.Clamp01(viewport.x);
        viewport.y = Mathf.Clamp01(viewport.y);
        Vector3 clamped = cam.ViewportToWorldPoint(viewport);
        clamped.z = 0;
        return clamped;
    }


    void AttackBehavior()
    {
        Debug.Log("[Bot] AttackBehavior running");
        // Dive toward player
        if (playerTarget != null && myRigidBody != null)
        {
            Vector2 dir = (playerTarget.transform.position - transform.position).normalized;
            myRigidBody.gravityScale = 0.5f; // lighter even when attacking
            myRigidBody.linearVelocity = dir * 32f; // much faster attack
            Debug.Log($"[Bot] AttackBehavior set velocity: {myRigidBody.linearVelocity}");

            // Check for collision with player and deal damage
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.transform.position);
            if (distanceToPlayer < 2.5f) // slightly larger hitbox for speed
            {
                var playerScript = playerTarget.GetComponent<PlayerController>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(1);
                    // Bot smoothly flies to a new safe position and enters Recovering phase
                    StartCoroutine(FlySmoothlyToSafePosition());
                }
            }
        }
    }

    // Bot smoothly flies to a new safe position and enters Recovering phase
    private System.Collections.IEnumerator FlySmoothlyToSafePosition()
    {
        phase = BotPhase.Recovering;
        float duration = 0.52f; // 30% longer
        float t = 0f;
        Vector2 start = transform.position;
        hoverHorizontalOffset = UnityEngine.Random.Range(-42f, 42f);
        Vector2 safePos = playerTarget.transform.position + new Vector3(hoverHorizontalOffset, 12.6f, 0);
        if (myRigidBody != null) myRigidBody.gravityScale = 0.2f;
        while (t < duration)
        {
            t += Time.deltaTime;
            Vector2 newPos = Vector2.Lerp(start, safePos, t / duration);
            if (myRigidBody != null)
                myRigidBody.MovePosition(newPos);
            yield return null;
        }
        if (myRigidBody != null)
        {
            myRigidBody.linearVelocity = Vector2.zero;
        }
        EnterStunnedPhase();
    }
      

    public void wiggle()
    {
        // Spinning disabled for now
        //Debug.Log("wiggling");
        //myRigidBody.AddTorque(myRigidBody.angularVelocity > 0 ? -80f : 80f, ForceMode2D.Impulse);
        //MoveRotation(myRigidBody.rotation > 0 ? -30 : 30);
    }

    public void TakeDamage(int damage)
    {
        if (invincible)
        {
            Debug.Log("Bot is invincible!");
            return;
        }
        health -= damage;
        Debug.Log("Bot took " + damage + " damage. Health: " + health);
        if (health <= 0)
        {
            Die();
        }
    }

    // Allow player to bat the bot away with a force
    public void BatAway(Vector2 force)
    {
        if (myRigidBody != null)
        {
            myRigidBody.AddForce(force, ForceMode2D.Impulse);
            Debug.Log($"[Bot] Batted away with force: {force}");
            EnterStunnedPhase();
        }
    }
    private void EnterStunnedPhase()
    {
        phase = BotPhase.Stunned;
        stunnedTimer = 0f;
        if (myRigidBody != null)
        {
            myRigidBody.gravityScale = 0.5f; // still light when stunned
        }
        Debug.Log("[Bot] Entered Stunned phase");
    }

    private void StunnedBehavior()
    {
        stunnedTimer += Time.deltaTime;
        // Optionally, add some visual effect or spinning here
        if (stunnedTimer > stunnedDuration)
        {
            EnterHoverPhase(playerTarget);
            Debug.Log("[Bot] Stunned phase ended, returning to Hover");
        }
    }
    

    void Die()
    {
        Debug.Log("Bot died!");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only die from explicit attack events or specific trigger collisions
        if (collision.CompareTag("PlayerAttack"))
        {
            logic.AddScore(1);
            Destroy(gameObject);
        }
    }
    }

