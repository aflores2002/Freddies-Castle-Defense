using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class CastleHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Castle Sprites")]
    [SerializeField] private SpriteRenderer castleRenderer;
    [SerializeField] private Color originalColor;
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

    void Start()
    {
        currentHealth = maxHealth;
        if (castleRenderer != null)
        {
            originalColor = castleRenderer.color;
        }

        // Setup castle collider
        SetupCastleCollider();
    }

    private void SetupCastleCollider()
    {
        BoxCollider2D castleCollider = GetComponent<BoxCollider2D>();
        if (castleCollider == null)
        {
            castleCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // Make sure it's NOT a trigger
        castleCollider.isTrigger = false;

        // Adjust the collider size to match your castle sprite
        // You might need to adjust these values based on your sprite size
        castleCollider.size = new Vector2(2f, 15f);
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

        // Flash red when hit
        if (castleRenderer != null)
        {
            StartCoroutine(FlashRed());
        }

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    private IEnumerator FlashRed()
    {
        castleRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        castleRenderer.color = originalColor;
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

        Vector3 currentPosition = transform.position;

        if (currentHealth > 50)
        {
            castleRenderer.sprite = castle_100;
            transform.position = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z);
        }
        else if (currentHealth > 0)
        {
            castleRenderer.sprite = castle_50;
            transform.position = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z);
        }
        else
        {
            castleRenderer.sprite = castle_0;
            // Adjust Y position for Castle_0
            transform.position = new Vector3(currentPosition.x, currentPosition.y - 1.55f, currentPosition.z);
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