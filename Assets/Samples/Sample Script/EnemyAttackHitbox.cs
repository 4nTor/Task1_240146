using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    public EnemyController enemy;
    public int damage = 2;
    private bool hasHit = false;

    private void Start()
    {
        if (enemy == null)
            enemy = GetComponentInParent<EnemyController>();
        
        if (enemy == null)
            Debug.LogError($"EnemyAttackHitbox on {gameObject.name}: No EnemyController found!");
        
        SetupCollisionLayers();
    }

    private void SetupCollisionLayers()
    {
        Collider2D hitboxCollider = GetComponent<Collider2D>();
        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("EnemyAttack"); // Ensure layer exists
        }
        else
        {
            Debug.LogError($"EnemyAttackHitbox on {gameObject.name}: No Collider2D found!");
        }
    }

    private void OnEnable()
    {
        hasHit = false;
        Debug.Log($"Enemy attack hitbox enabled for {enemy?.gameObject.name}");
    }

    private void OnDisable()
    {
        Debug.Log($"Enemy attack hitbox disabled for {enemy?.gameObject.name}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Enemy hitbox detected: {other.name} with tag: {other.tag}");
        
        if (!other.CompareTag("Player"))
        {
            Debug.Log($"Ignoring collision with non-player: {other.name}");
            return;
        }
        
        if (hasHit)
        {
            Debug.Log("Enemy attack already hit this cycle");
            return;
        }

        if (enemy == null || !enemy.IsAttacking())
        {
            Debug.Log($"Enemy not attacking or null. Enemy: {enemy != null}, Attacking: {enemy?.IsAttacking()}");
            return;
        }

        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            int damageAmount = enemy.Damage > 0 ? enemy.Damage : damage;
            playerController.TakeDamage(damageAmount);
            hasHit = true;
            Debug.Log($"SUCCESS: Enemy hit player for {damageAmount} damage!");
        }
        else
        {
            Debug.LogError($"Player object {other.name} doesn't have PlayerController component!");
        }
    }

    public void ResetHit()
    {
        hasHit = false;
    }
}