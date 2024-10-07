using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FollowCameraRotation))]
public class HealthBar : MonoBehaviour
{
    [SerializeField] bool isBillboarded = true;
    [SerializeField] bool shouldShowHealthNumbers = true;

    float finalValue;
    float animationSpeed = 0.1f;
    float leftoverAmount = 0f;

    // Caches
    CastleHealth castleHealth;
    Image fillImage;
    Text healthText;
    FollowCameraRotation followCameraRotation;

    private void Start()
    {
        InitializeComponents();
    }

    void InitializeComponents()
    {
        castleHealth = GetComponentInParent<CastleHealth>();
        if (castleHealth == null)
        {
            Debug.LogError("CastleHealth not found in parent. Please ensure it's attached to the parent object.");
            enabled = false;
            return;
        }

        fillImage = GetComponentInChildren<Image>();
        if (fillImage == null)
        {
            Debug.LogError("Image component not found in children. Please ensure there's an Image component for the health bar fill.");
            enabled = false;
            return;
        }

        healthText = GetComponentInChildren<Text>();
        if (healthText == null)
        {
            Debug.LogWarning("Text component not found in children. Health numbers will not be displayed.");
            shouldShowHealthNumbers = false;
        }

        followCameraRotation = GetComponent<FollowCameraRotation>();
        if (followCameraRotation == null)
        {
            Debug.LogWarning("FollowCameraRotation component not found. The health bar may not face the camera correctly.");
        }

        castleHealth.OnHealthChanged.AddListener(ChangeHealthFill);
    }

    void Update()
    {
        if (castleHealth == null || fillImage == null) return;

        animationSpeed = castleHealth.AnimationDuration;

        if (!castleHealth.HasAnimationWhenHealthChanges)
        {
            fillImage.fillAmount = castleHealth.CurrentHealthPercentage / 100;
        }

        if (shouldShowHealthNumbers && healthText != null)
        {
            healthText.text = $"{castleHealth.CurrentHealth}/{castleHealth.MaximumHealth}";
            healthText.enabled = shouldShowHealthNumbers;
        }

        if (followCameraRotation != null)
        {
            followCameraRotation.enabled = isBillboarded;
        }
    }

    private void ChangeHealthFill(HealthChangeData healthData)
    {
        if (!castleHealth.HasAnimationWhenHealthChanges) return;

        StopAllCoroutines();
        StartCoroutine(ChangeFillAmount(healthData));
    }

    private IEnumerator ChangeFillAmount(HealthChangeData healthData)
    {
        finalValue = healthData.HealthPercentage / 100;

        float cacheLeftoverAmount = this.leftoverAmount;

        float timeElapsed = 0;

        while (timeElapsed < animationSpeed)
        {
            float leftoverAmount = Mathf.Lerp((float)healthData.PreviousHealth / castleHealth.MaximumHealth + cacheLeftoverAmount, finalValue, timeElapsed / animationSpeed);
            this.leftoverAmount = leftoverAmount - finalValue;
            fillImage.fillAmount = leftoverAmount;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        this.leftoverAmount = 0;
        fillImage.fillAmount = finalValue;
    }
}