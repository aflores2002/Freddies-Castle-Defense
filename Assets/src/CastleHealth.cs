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
    [SerializeField] private Sprite castle_100;
    [SerializeField] private Sprite castle_50;
    [SerializeField] private Sprite castle_0;

    [Header("Health Bar")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;

    [Header("Visual Effects")]
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private Color healFlashColor = Color.green;
    private Color originalColor;
    private Coroutine flashCoroutine;

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
        UpdateCastleAppearance();
        UpdateHealthBar();

        // Configure castle collider
        BoxCollider2D castleCollider = GetComponent<BoxCollider2D>();
        if (castleCollider != null)
        {
            castleCollider.isTrigger = true;
            castleCollider.size = new Vector2(2f, 15f);
            castleCollider.offset = new Vector2(-5.5f, -1.4f);
        }

        // Add Rigidbody2D if it doesn't exist
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.isKinematic = true;
        rb.gravityScale = 0;

        gameObject.tag = "Castle";
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"Castle taking {damage} damage");
        AudioManager.Instance.PlaySoundEffect("CastleDamage");

        int previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateCastleAppearance();
        UpdateHealthBar();
        StartFlashEffect(damageFlashColor);

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

        Debug.Log($"Castle healing for {amount} health");
        AudioManager.Instance?.PlaySoundEffect("CastleHeal");

        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        UpdateCastleAppearance();
        UpdateHealthBar();
        StartFlashEffect(healFlashColor);

        OnHealthChanged.Invoke(new HealthChangeData
        {
            CurrentHealth = currentHealth,
            PreviousHealth = previousHealth,
            HealthPercentage = CurrentHealthPercentage,
            IsHeal = true
        });
    }

    private void StartFlashEffect(Color flashColor)
    {
        if (castleRenderer == null) return;

        // If there's an existing flash coroutine, stop it
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashEffectCoroutine(flashColor));
    }

    private IEnumerator FlashEffectCoroutine(Color flashColor)
    {
        // Set the sprite color to the flash color
        castleRenderer.color = flashColor;

        // Wait for the specified duration
        yield return new WaitForSeconds(flashDuration);

        // Return to the original color
        castleRenderer.color = originalColor;

        flashCoroutine = null;
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