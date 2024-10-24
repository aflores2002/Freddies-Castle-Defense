using UnityEngine;
using UnityEngine.Events;

public class ZombieHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private Animator animator;
    private bool isDead = false;
    private float hurtCooldown = 0.5f;
    private float lastHurtTime;

    public UnityEvent OnZombieDeath = new UnityEvent();

    void Start()
    {
        animator = GetComponent<Animator>();
        lastHurtTime = -hurtCooldown;
        currentHealth = maxHealth; // Will use the default if SetMaxHealth wasn't called
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Zombie took {damage} damage. Current health: {currentHealth}/{maxHealth}");

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
        animator.SetTrigger("Hurt");
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
        Debug.Log("Death state triggered");
        animator.SetTrigger("Death");

        // Invoke the death event
        OnZombieDeath.Invoke();

        // Disable the zombie's collider
        Collider2D zombieCollider = GetComponent<Collider2D>();
        if (zombieCollider != null)
        {
            zombieCollider.enabled = false;
        }

        // Disable the ZombieMovement script
        ZombieMovement movement = GetComponent<ZombieMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        // Get the length of the death animation
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float deathAnimationLength = stateInfo.length;

        // Destroy the zombie after the animation finishes
        Destroy(gameObject, deathAnimationLength);
    }
}