using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

// CastleHealth manages the castle's health system, visual appearance, and damage/healing effects
public class CastleHealth : MonoBehaviour
{
    // Base health settings
    [SerializeField] private int maxHealth = 100;        // Max possible health
    private int currentHealth;                           // Current health

    [Header("Castle Sprites")]
    [SerializeField] private SpriteRenderer castleRenderer; // Main sprite renderer component
    [SerializeField] private Sprite castle_100;             // Full health sprite
    [SerializeField] private Sprite castle_50;              // Half health sprite
    [SerializeField] private Sprite castle_0;               // Destroyed sprite

    [Header("Health Bar")]
    [SerializeField] private Image healthBarFill;                 // UI health bar fill image
    [SerializeField] private Color healthyColor = Color.green;    // Color when health > 50%
    [SerializeField] private Color damagedColor = Color.yellow;   // Color when health 25-50%
    [SerializeField] private Color criticalColor = Color.red;     // Color when health < 25%

    [Header("Visual Effects")]
    [SerializeField] private float flashDuration = 0.2f;         // Duration of damage/heal flash
    [SerializeField] private Color damageFlashColor = Color.red; // Color flash on damage
    [SerializeField] private Color healFlashColor = Color.green; // Color flash on heal
    private Color originalColor;                                 // Store original sprite color
    private Coroutine flashCoroutine;                            // Reference to active flash effect

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel; // Panel shown when castle is destroyed
    [SerializeField] private Button startOverButton;   // Button to restart the game
    private WaveManager waveManager;                   // Reference to wave manager
    private Vector3 originalPosition;                  // Store the castle's starting position

    // Public properties for external access
    public int MaximumHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float CurrentHealthPercentage => (float)currentHealth / maxHealth * 100f;

    // Animation settings
    public bool HasAnimationWhenHealthChanges = true;
    public float AnimationDuration = 0.1f;

    // Event to notify listeners of health changes
    public UnityEvent<HealthChangeData> OnHealthChanged = new UnityEvent<HealthChangeData>();

    void Start()
    {
        // Store original position
        originalPosition = transform.position;

        // Initialize health and colors
        currentHealth = maxHealth;
        if (castleRenderer != null)
        {
            originalColor = castleRenderer.color;
        }

        // Get WaveManager reference
        waveManager = FindObjectOfType<WaveManager>();

        // Setup start over button
        if (startOverButton != null)
        {
            startOverButton.onClick.AddListener(OnStartOverClicked);
        }

        UpdateCastleAppearance();
        UpdateHealthBar();

        // Setup castle collision detection
        BoxCollider2D castleCollider = GetComponent<BoxCollider2D>();
        if (castleCollider != null)
        {
            castleCollider.isTrigger = true;
            castleCollider.size = new Vector2(2f, 15f);        // Set collision box size
            castleCollider.offset = new Vector2(-5.5f, -1.4f); // Adjust collision box position
        }

        // Setup physics properties
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.isKinematic = true;     // Disable physics simulation
        rb.gravityScale = 0;       // Disable gravity

        gameObject.tag = "Castle"; // Set tag for collision detection

        // Ensure game over panel starts hidden
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // Handle taking damage
    public void TakeDamage(int damage)
    {
        Debug.Log($"Castle taking {damage} damage");
        AudioManager.Instance.PlaySoundEffect("CastleDamage");

        // Calculate and apply damage
        int previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Prevent negative health

        // Update visuals
        UpdateCastleAppearance();
        UpdateHealthBar();
        StartFlashEffect(damageFlashColor);

        // Notify listeners of health change
        OnHealthChanged.Invoke(new HealthChangeData
        {
            CurrentHealth = currentHealth,
            PreviousHealth = previousHealth,
            HealthPercentage = CurrentHealthPercentage,
            IsHeal = false
        });

        // Check if castle is destroyed
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    // Handle healing
    public void Heal(int amount)
    {
        if (amount <= 0) return;

        Debug.Log($"Castle healing for {amount} health");
        AudioManager.Instance?.PlaySoundEffect("CastleHeal");

        // Calculate and apply healing
        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);  // Cap at max health

        // Update visuals
        UpdateCastleAppearance();
        UpdateHealthBar();
        StartFlashEffect(healFlashColor);

        // Notify listeners of health change
        OnHealthChanged.Invoke(new HealthChangeData
        {
            CurrentHealth = currentHealth,
            PreviousHealth = previousHealth,
            HealthPercentage = CurrentHealthPercentage,
            IsHeal = true
        });
    }

    // Initiate color flash effect
    private void StartFlashEffect(Color flashColor)
    {
        if (castleRenderer == null) return;

        // Stop any existing flash effect
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashEffectCoroutine(flashColor));
    }

    // Coroutine to handle the flash effect animation
    private IEnumerator FlashEffectCoroutine(Color flashColor)
    {
        // Change to flash color
        castleRenderer.color = flashColor;

        // Wait for flash duration
        yield return new WaitForSeconds(flashDuration);

        // Revert to original color
        castleRenderer.color = originalColor;

        flashCoroutine = null;
    }

    // Updates the health bar UI
    private void UpdateHealthBar()
    {
        if (healthBarFill == null)
        {
            Debug.LogWarning("Health bar fill image is not assigned!");
            return;
        }

        // Update fill amount based on health percentage
        float fillAmount = (float)currentHealth / maxHealth;
        healthBarFill.fillAmount = fillAmount;

        // Update health bar color based on health level
        if (fillAmount > 0.5f)
            healthBarFill.color = healthyColor;
        else if (fillAmount > 0.25f)
            healthBarFill.color = damagedColor;
        else
            healthBarFill.color = criticalColor;
    }

    // Updates the castle's visual appearance based on health
    private void UpdateCastleAppearance()
    {
        if (castleRenderer == null) return;

        // Change sprite based on health level
        if (currentHealth > 50)
        {
            castleRenderer.sprite = castle_100;
            transform.position = originalPosition;
        }
        else if (currentHealth > 0)
        {
            castleRenderer.sprite = castle_50;
            transform.position = originalPosition;
        }
        else
        {
            castleRenderer.sprite = castle_0;
            // Adjust position down for destroyed state
            transform.position = new Vector3(originalPosition.x, originalPosition.y - 1.55f, originalPosition.z);
        }
    }

    // Handles game over state
    private void GameOver()
    {
        Debug.Log("Game Over! Castle has been destroyed.");

        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Stop and clear zombies through wave manager
        if (waveManager != null)
        {
            waveManager.HandleGameOver();
        }
        else
        {
            Debug.LogError("WaveManager reference not found in CastleHealth!");
        }
    }

    // Handle start over button click
    private void OnStartOverClicked()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Reset castle health and position
        currentHealth = maxHealth;
        transform.position = originalPosition; // Reset position
        UpdateCastleAppearance();
        UpdateHealthBar();

        // Restart game through wave manager
        if (waveManager != null)
        {
            waveManager.StartNewGame();
        }
    }
}

// Data structure for health change events
public struct HealthChangeData
{
    public int CurrentHealth;      // Current health after change
    public int PreviousHealth;     // Health before change
    public float HealthPercentage; // Current health as percentage
    public bool IsHeal;            // Whether castle healing or damaged
}