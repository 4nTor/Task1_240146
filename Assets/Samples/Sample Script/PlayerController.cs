using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 8f;
    private bool isGrounded;

    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public GameObject Health;

    [Header("Combat")]
    public GameObject attackHitbox;
    private bool isAttacking = false;
    public float attackDuration = 0.5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isDead = false;
    private bool canTakeDamage = true;
    public GameObject gameOverPanel;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        UpdateHearts();

        if (Health != null)
            Health.SetActive(true);

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        Debug.Log($"Player initialized with {currentHealth} health");
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleJump();
        HandleAttack();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (animator != null)
                animator.SetBool("isRunning", false);
        }
        else
        {
            rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
            if (animator != null)
                animator.SetBool("isRunning", moveX != 0);

            if (moveX != 0)
            {
                Vector3 newScale = transform.localScale;
                newScale.x = Mathf.Abs(newScale.x) * (moveX < 0 ? 1 : -1);
                transform.localScale = newScale;
            }
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (animator != null)
                animator.SetBool("isJump", true);
            isGrounded = false;
        }

        if (rb.linearVelocity.y < 0 && !isGrounded)
        {
            if (animator != null)
                animator.SetBool("isJump", false);
        }
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            isAttacking = true;
            if (animator != null)
            {
                animator.ResetTrigger("isAttack");
                animator.SetTrigger("isAttack");
            }
            Debug.Log("Player started attack");

            Invoke(nameof(EndAttack), attackDuration);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
            if (animator != null)
                animator.SetBool("isJump", false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || !canTakeDamage)
        {
            Debug.Log($"Player damage blocked (Dead: {isDead}, CanTakeDamage: {canTakeDamage})");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();

        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        StartCoroutine(DamageInvincibility());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator DamageInvincibility()
    {
        canTakeDamage = false;

        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        yield return new WaitForSeconds(0.4f);
        canTakeDamage = true;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player Died - Game Over!");

        rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.ResetTrigger("isDead");
            animator.SetTrigger("isDead");
        }

        if (GameManager.instance != null)
            GameManager.instance.GameOver();
        else
            Debug.LogError("GameManager.instance is null!");
        
        Time.timeScale = 0f;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        this.enabled = false;
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
            }
        }
    }

    public void EnableHitbox()
    {
        if (attackHitbox != null && !isDead)
        {
            attackHitbox.SetActive(true);
            Debug.Log("Player hitbox enabled via animation event");
        }
    }

    public void DisableHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
            Debug.Log("Player hitbox disabled via animation event");
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        DisableHitbox();
        Debug.Log("Player ended attack");

        CancelInvoke(nameof(EndAttack));
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

    [ContextMenu("Test Die")]
    public void TestDie()
    {
        currentHealth = 0;
        Die();
    }
}