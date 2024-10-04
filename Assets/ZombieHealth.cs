using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private Animator animator;
    private bool isDead = false;
    private float hurtCooldown = 0.5f;  // Adjust this value as needed
    private float lastHurtTime;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        lastHurtTime = -hurtCooldown;  // Allow immediate hurt on first hit
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Zombie took {damage} damage. Current health: {currentHealth}");

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
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Death state triggered");
        animator.SetTrigger("Death");

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