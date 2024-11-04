using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ZombieHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private Animator animator;
    private bool isDead = false;
    private float hurtCooldown = 0.5f;
    private float lastHurtTime;

    public bool IsBoss { get; set; } = false;
    public UnityEvent OnZombieDeath = new UnityEvent();

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        lastHurtTime = -hurtCooldown;  // Allow immediate hurt on first hit
    }

    public void SetMaxHealth(int health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Different debug messages for boss and normal zombies
        if (IsBoss)
        {
            // AudioManager.Instance?.PlaySound("BossHit");

            Debug.Log($"BOSS took {damage} damage. Current health: {currentHealth}/{maxHealth}");
        }
        else
        {
            // AudioManager.Instance?.PlaySound("ZombieHit");

            Debug.Log($"Zombie took {damage} damage. Current health: {currentHealth}/{maxHealth}");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (Time.time >= lastHurtTime + hurtCooldown)
        {
            Hurt();
        }
    }

    void Hurt()
    {
        lastHurtTime = Time.time;
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
        Debug.Log("Hurt state triggered");

        // Disable the ZombieMovement script
        ZombieMovement movement = GetComponent<ZombieMovement>();
        if (movement != null)
        {
            movement.enabled = false;
            animator.SetTrigger("Idle");
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (IsBoss)
        {
            // Play boss death sound
            AudioManager.Instance.PlaySoundEffect("BossDeath");
        }
        else
        {
            // Play zombie death sound
            AudioManager.Instance.PlaySoundEffect("ZombieDeath");
        }

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Disable the zombie's colliders
        Collider2D[] zombieColliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in zombieColliders)
        {
            collider.enabled = false;
        }

        // Disable the ZombieMovement script
        ZombieMovement movement = GetComponent<ZombieMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        // Signal that this zombie has died
        OnZombieDeath.Invoke();

        // Get the length of the death animation
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float deathAnimationLength = stateInfo.length;
            Destroy(gameObject, deathAnimationLength);
        }
        else
        {
            // If no animator, destroy after a default delay
            Destroy(gameObject, 1f);
        }
    }
}