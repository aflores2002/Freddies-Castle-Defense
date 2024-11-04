using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// HealthBar manages the visual representation of the castle's health
// Requires FollowCameraRotation component to be attached
[RequireComponent(typeof(FollowCameraRotation))]
public class HealthBar : MonoBehaviour
{
    // Configuration options
    [SerializeField] bool isBillboarded = true;             // Whether health bar should face camera
    [SerializeField] bool shouldShowHealthNumbers = true;   // Whether to display numerical health values

    // Animation variables
    float finalValue;            // Target fill amount for health bar
    float animationSpeed = 0.1f; // Speed of health bar fill animation
    float leftoverAmount = 0f;   // Used for health bar transitions

    // Component references
    CastleHealth castleHealth;                 // Reference to the castle's health component
    Image fillImage;                           // Reference to health bar fill image
    Text healthText;                           // Reference to health number display
    FollowCameraRotation followCameraRotation; // Reference to camera following component

    private void Start()
    {
        InitializeComponents();
    }

    // Initialize and validate all required components
    void InitializeComponents()
    {
        // Get CastleHealth from parent object
        castleHealth = GetComponentInParent<CastleHealth>();
        if (castleHealth == null)
        {
            Debug.LogError("CastleHealth not found in parent. Please ensure it's attached to the parent object.");
            enabled = false;
            return;
        }

        // Get fill image component from children
        fillImage = GetComponentInChildren<Image>();
        if (fillImage == null)
        {
            Debug.LogError("Image component not found in children. Please ensure there's an Image component for the health bar fill.");
            enabled = false;
            return;
        }

        // Get text component for health numbers
        healthText = GetComponentInChildren<Text>();
        if (healthText == null)
        {
            Debug.LogWarning("Text component not found in children. Health numbers will not be displayed.");
            shouldShowHealthNumbers = false;
        }

        // Get camera rotation following component
        followCameraRotation = GetComponent<FollowCameraRotation>();
        if (followCameraRotation == null)
        {
            Debug.LogWarning("FollowCameraRotation component not found. The health bar may not face the camera correctly.");
        }

        // Subscribe to health change events
        castleHealth.OnHealthChanged.AddListener(ChangeHealthFill);
    }

    void Update()
    {
        // Validate required components
        if (castleHealth == null || fillImage == null) return;

        // Update animation speed from castle health settings
        animationSpeed = castleHealth.AnimationDuration;

        // Update health bar fill instantly if animations are disabled
        if (!castleHealth.HasAnimationWhenHealthChanges)
        {
            fillImage.fillAmount = castleHealth.CurrentHealthPercentage / 100;
        }

        // Update numerical health display if enabled
        if (shouldShowHealthNumbers && healthText != null)
        {
            healthText.text = $"{castleHealth.CurrentHealth}/{castleHealth.MaximumHealth}";
            healthText.enabled = shouldShowHealthNumbers;
        }

        // Update billboarding (facing camera) if enabled
        if (followCameraRotation != null)
        {
            followCameraRotation.enabled = isBillboarded;
        }
    }

    // Handle health change events
    private void ChangeHealthFill(HealthChangeData healthData)
    {
        // Skip if animations are disabled
        if (!castleHealth.HasAnimationWhenHealthChanges) return;

        // Stop any running animations and start new one
        StopAllCoroutines();
        StartCoroutine(ChangeFillAmount(healthData));
    }

    // Coroutine to smoothly animate health bar fill
    private IEnumerator ChangeFillAmount(HealthChangeData healthData)
    {
        // Calculate target fill amount
        finalValue = healthData.HealthPercentage / 100;

        // Cache current leftover amount for smooth transition
        float cacheLeftoverAmount = this.leftoverAmount;

        float timeElapsed = 0;

        // Animate fill amount over time
        while (timeElapsed < animationSpeed)
        {
            // Calculate smooth transition using Lerp
            float leftoverAmount = Mathf.Lerp(
                (float)healthData.PreviousHealth / castleHealth.MaximumHealth + cacheLeftoverAmount,
                finalValue,
                timeElapsed / animationSpeed
            );

            // Update leftover amount for next frame
            this.leftoverAmount = leftoverAmount - finalValue;

            // Update fill amount
            fillImage.fillAmount = leftoverAmount;

            // Update timer
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final values are set correctly
        this.leftoverAmount = 0;
        fillImage.fillAmount = finalValue;
    }
}