using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Trigger hurt animation
            animator.SetTrigger("Hurt");
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        // Trigger death animation
        animator.SetTrigger("Death");

        // Disable the zombie's collider
        Collider2D zombieCollider = GetComponent<Collider2D>();
        if (zombieCollider != null)
        {
            zombieCollider.enabled = false;
        }

        // Disable all MonoBehaviour scripts attached to the zombie
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this) // Don't disable this script
            {
                script.enabled = false;
            }
        }

        // You might want to destroy the zombie after a delay or handle it differently
        Destroy(gameObject, 2f);  // Destroy after 2 seconds
    }
}