using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI; // Add this for UI components

public class CastleHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Castle Sprites")]
    [SerializeField] private SpriteRenderer castleRenderer;
    [SerializeField] private Sprite castle_100;
    [SerializeField] private Sprite castle_50;
    [SerializeField] private Sprite castle_0;

    [Header("Health Bar")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;

    public int MaximumHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float CurrentHealthPercentage => (float)currentHealth / maxHealth * 100f;

    public bool HasAnimationWhenHealthChanges = true;
    public float AnimationDuration = 0.1f;

    public UnityEvent<HealthChangeData> OnHealthChanged = new UnityEvent<HealthChangeData>();

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateCastleAppearance();
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        AudioManager.Instance.PlaySoundEffect("CastleDamage");

        int previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateCastleAppearance();
        UpdateHealthBar();

        OnHealthChanged.Invoke(new HealthChangeData
        {
            CurrentHealth = currentHealth,
            PreviousHealth = previousHealth,
            HealthPercentage = CurrentHealthPercentage,
            IsHeal = false
        });

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        UpdateCastleAppearance();
        UpdateHealthBar();

        OnHealthChanged.Invoke(new HealthChangeData
        {
            CurrentHealth = currentHealth,
            PreviousHealth = previousHealth,
            HealthPercentage = CurrentHealthPercentage,
            IsHeal = true
        });
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill == null)
        {
            Debug.LogWarning("Health bar fill image is not assigned!");
            return;
        }

        float fillAmount = (float)currentHealth / maxHealth;
        healthBarFill.fillAmount = fillAmount;

        // Update health bar color based on health percentage
        if (fillAmount > 0.5f)
            healthBarFill.color = healthyColor;
        else if (fillAmount > 0.25f)
            healthBarFill.color = damagedColor;
        else
            healthBarFill.color = criticalColor;
    }

    private void UpdateCastleAppearance()
    {
        if (castleRenderer == null) return;

        if (currentHealth > 50)
        {
            castleRenderer.sprite = castle_100;
        }
        else if (currentHealth > 0)
        {
            castleRenderer.sprite = castle_50;
        }
        else
        {
            castleRenderer.sprite = castle_0;
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! Castle has been destroyed.");
    }
}

public struct HealthChangeData
{
    public int CurrentHealth;
    public int PreviousHealth;
    public float HealthPercentage;
    public bool IsHeal;
}