using UnityEngine;
using System.Collections.Generic;

public class PlayerAttackHitbox : MonoBehaviour
{
    public int damage = 2;
    private PlayerController player;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
        if (player == null)
            player = FindObjectOfType<PlayerController>();
        
        if (player == null)
            Debug.LogError("PlayerAttackHitbox: No PlayerController found!");

        SetupCollisionLayers();
    }

    private void SetupCollisionLayers()
    {

        Collider2D hitboxCollider = GetComponent<Collider2D>();
        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        hitEnemies.Clear();
        Debug.Log("Player attack hitbox enabled - Ready to hit enemies");
    }

    private void OnDisable()
    {
        Debug.Log("Player attack hitbox disabled");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Player hitbox detected: {other.name} with tag: {other.tag}");

        if (!other.CompareTag("Enemy"))
        {
            Debug.Log($"Ignoring collision with non-enemy: {other.name}");
            return;
        }

        if (hitEnemies.Contains(other.gameObject))
        {
            Debug.Log($"Enemy {other.name} already hit in this attack");
            return;
        }

        if (player == null || !player.IsAttacking())
        {
            Debug.Log($"Player not attacking or null. Player: {player != null}, Attacking: {player?.IsAttacking()}");
            return;
        }

        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null && !enemy.IsDead())
        {
            enemy.TakeDamage(damage);
            hitEnemies.Add(other.gameObject);
            Debug.Log($"SUCCESS: Player hit enemy {other.name} for {damage} damage!");
        }
        else
        {
            Debug.LogError($"Enemy {other.name} doesn't have EnemyController or is already dead!");
        }
    }
}