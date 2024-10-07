using UnityEngine;
using UnityEngine.Events;

public class CastleHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public int MaximumHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float CurrentHealthPercentage => (float)currentHealth / maxHealth * 100f;

    public bool HasAnimationWhenHealthChanges = true;
    public float AnimationDuration = 0.1f;

    public UnityEvent<HealthChangeData> OnHealthChanged = new UnityEvent<HealthChangeData>();

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        int previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Ensure health doesn't go below 0

        OnHealthChanged.Invoke(new HealthChangeData
        {
            CurrentHealth = currentHealth,
            PreviousHealth = previousHealth,
            HealthPercentage = CurrentHealthPercentage
        });

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! Castle has been destroyed.");
        // Add your game over logic here (e.g., show game over screen, restart level, etc.)
    }
}

public struct HealthChangeData
{
    public int CurrentHealth;
    public int PreviousHealth;
    public float HealthPercentage;
}