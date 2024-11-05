using UnityEngine;
using System.Collections;
using UnityEngine.Events;

// ZombieHealth manages the health system, damage handling, and death behavior for zombies
public class ZombieHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100; // Max health
    private int currentHealth;                    // Current health value
    private Animator animator;                    // Reference to zombie's animator
    private bool isDead = false;                  // Track if zombie is dead
    private float hurtCooldown = 0.5f;            // Time required between hurt states
    private float lastHurtTime;                   // Time of last hurt state

    // Properties for boss status and death event
    public bool IsBoss { get; set; } = false;           // Whether this is a boss zombie
    public UnityEvent OnZombieDeath = new UnityEvent(); // Event fired when zombie dies

    void Start()
    {
        // Initialize health and components
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        lastHurtTime = -hurtCooldown; // Set negative cooldown to allow immediate first hit
    }

    // Set max and current health
    public void SetMaxHealth(int health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
    }

    // Handle damage taken by the zombie
    public void TakeDamage(int damage)
    {
        // Prevent damage to dead zombies
        if (isDead) return;

        // Apply damage
        currentHealth -= damage;

        // Log different messages for boss and regular zombies
        if (IsBoss)
        {
            Debug.Log($"BOSS took {damage} damage. Current health: {currentHealth}/{maxHealth}");
        }
        else
        {
            Debug.Log($"Zombie took {damage} damage. Current health: {currentHealth}/{maxHealth}");
        }

        // Check for death or hurt state
        if (currentHealth <= 0)
        {
            Die();
        }
        else if (Time.time >= lastHurtTime + hurtCooldown)
        {
            Hurt();
        }
    }

    // Handle hurt state
    void Hurt()
    {
        // Update last hurt time
        lastHurtTime = Time.time;

        // Trigger hurt animation if animator exists
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
        Debug.Log("Hurt state triggered");

        // Disable movement during hurt state
        ZombieMovement movement = GetComponent<ZombieMovement>();
        if (movement != null)
        {
            movement.enabled = false;
            animator.SetTrigger("Idle");
        }
    }

    // Handle death state
    void Die()
    {
        // Prevent multiple death calls
        if (isDead) return;

        isDead = true;

        // Play appropriate death sound based on zombie type
        if (IsBoss)
        {
            AudioManager.Instance.PlaySoundEffect("BossDeath");
        }
        else
        {
            AudioManager.Instance.PlaySoundEffect("ZombieDeath");
        }

        // Trigger death animation if animator exists
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Disable all colliders on death
        Collider2D[] zombieColliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in zombieColliders)
        {
            collider.enabled = false;
        }

        // Disable movement on death
        ZombieMovement movement = GetComponent<ZombieMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        // Notify listeners that zombie has died
        OnZombieDeath.Invoke();

        // Schedule zombie destruction after death animation
        if (animator != null)
        {
            // Get death animation length for timed destruction
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float deathAnimationLength = stateInfo.length;
            Destroy(gameObject, deathAnimationLength);
        }
        else
        {
            // Use default destruction delay if no animator
            Destroy(gameObject, 1f);
        }
    }
}