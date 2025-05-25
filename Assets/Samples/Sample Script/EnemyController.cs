using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public Transform[] patrolPoints;
    public float moveSpeed = 2f;
    private int currentPointIndex = 0;

    [Header("Combat")]
    public float aggroRange = 5f;
    public float attackRange = 1.5f;
    public int Damage = 2;
    public float attackCooldown = 2f;
    private float lastAttackTime = -999f;

    [Header("Health & Death")]
    public int maxHealth = 5;
    private int currentHealth;
    public float deathDelay = 2f;

    [Header("References")]
    public GameObject attackHitbox;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;
    private Transform player;

    private bool isAttacking = false;
    private bool isAggressive = false;
    private bool isDead = false;
    private bool canTakeDamage = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"Enemy {gameObject.name} found player: {player.name}");
        }
        else
        {
            Debug.LogError($"Enemy {gameObject.name}: No Player found with 'Player' tag! Disabling enemy.");
            enabled = false;
            return;
        }

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
            Debug.Log($"Enemy {gameObject.name}: Attack hitbox setup complete");
        }
        else
        {
            Debug.LogError($"Enemy {gameObject.name}: No attack hitbox assigned!");
        }

        Debug.Log($"Enemy {gameObject.name} initialized with {currentHealth} health");
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < aggroRange && !isAggressive)
        {
            isAggressive = true;
            Debug.Log($"Enemy {gameObject.name} became aggressive - player in range!");
        }

        if (isAttacking) return;

        if (isAggressive)
        {
            HandleAggressiveBehavior(distanceToPlayer);
        }
        else
        {
            HandlePatrolBehavior();
        }
    }

    private void HandleAggressiveBehavior(float distanceToPlayer)
    {
        if (distanceToPlayer > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, 0);

            if (direction.x != 0)
            {
                Vector3 newScale = transform.localScale;
                newScale.x = Mathf.Abs(newScale.x) * (direction.x < 0 ? 1 : -1);
                transform.localScale = newScale;
                UpdateAttackHitboxSide();
            }

            if (animator != null)
                animator.SetBool("isRunning", true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            if (animator != null)
                animator.SetBool("isRunning", false);

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                StartAttack();
            }
        }
    }

    private void HandlePatrolBehavior()
    {
        if (patrolPoints.Length == 0 || patrolPoints[currentPointIndex] == null)
        {
            Debug.LogWarning($"Enemy {gameObject.name}: Invalid patrol points!");
            rb.linearVelocity = Vector2.zero;
            if (animator != null)
                animator.SetBool("isRunning", false);
            return;
        }

        Transform target = patrolPoints[currentPointIndex];
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, 0);

        if (direction.x != 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = Mathf.Abs(newScale.x) * (direction.x < 0 ? 1 : -1);
            transform.localScale = newScale;
            UpdateAttackHitboxSide();
        }

        if (Vector2.Distance(transform.position, target.position) < 0.2f)
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;

        if (animator != null)
            animator.SetBool("isRunning", true);
    }

    public void StartAttack()
    {
        if (isDead || isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger("isAttack");
        }

        Debug.Log($"Enemy {gameObject.name} started attack animation");
    }

    public void EndAttack()
    {
        isAttacking = false;
        DisableHitbox();
        if (attackHitbox != null)
        {
            EnemyAttackHitbox hitboxScript = attackHitbox.GetComponent<EnemyAttackHitbox>();
            if (hitboxScript != null)
                hitboxScript.ResetHit();
        }
        Debug.Log($"Enemy {gameObject.name} ended attack");
    }

    public void EnableHitbox()
    {
        if (attackHitbox != null && !isDead && isAttacking)
        {
            attackHitbox.SetActive(true);
            Debug.Log($"Enemy {gameObject.name} ENABLED attack hitbox");
        }
        else
        {
            Debug.LogWarning($"Enemy {gameObject.name} failed to enable hitbox - Hitbox: {attackHitbox != null}, Dead: {isDead}, Attacking: {isAttacking}");
        }
    }

    public void DisableHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
            Debug.Log($"Enemy {gameObject.name} DISABLED attack hitbox");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || !canTakeDamage)
        {
            Debug.Log($"Enemy {gameObject.name} - Damage blocked (Dead: {isDead}, CanTakeDamage: {canTakeDamage})");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"Enemy {gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (animator != null)
        {
            animator.SetTrigger("isDamaged");
        }

        StartCoroutine(DamageInvincibility());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator DamageInvincibility()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(0.1f);
        canTakeDamage = true;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"Enemy {gameObject.name} DIED!");

        rb.linearVelocity = Vector2.zero;
        isAttacking = false;

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        if (animator != null)
        {
            animator.SetTrigger("isDead");
            Debug.Log($"Enemy {gameObject.name} - Death animation triggered");
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.OnEnemyDeath();
            Debug.Log($"GameManager notified of {gameObject.name} death");
        }

        this.enabled = false;
        Destroy(gameObject, deathDelay);
        Debug.Log($"Enemy {gameObject.name} will be destroyed in {deathDelay} seconds");
    }

    private void UpdateAttackHitboxSide()
    {
        if (attackHitbox != null)
        {
            Vector3 localPos = attackHitbox.transform.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (transform.localScale.x < 0 ? -1 : 1);
            attackHitbox.transform.localPosition = localPos;
        }
    }

    public bool IsAttacking()
    {
        return isAttacking && !isDead;
    }

    public bool IsDead()
    {
        return isDead;
    }

    [ContextMenu("Test Take Damage")]
    public void TestTakeDamage()
    {
        TakeDamage(1);
    }

    [ContextMenu("Force Attack")]
    public void ForceAttack()
    {
        StartAttack();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (patrolPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.3f);
            }
        }
    }
}
