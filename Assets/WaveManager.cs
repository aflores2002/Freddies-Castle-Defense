using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int baseZombiesPerWave = 10;
    [SerializeField] private int zombieIncreasePerWave = 5;

    [Header("Castle Healing")]
    [SerializeField] private int healAmount = 10;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveCounterText;
    [SerializeField] private TextMeshProUGUI killCounterText;
    [SerializeField] private GameObject waveCompletePanel;
    [SerializeField] private Button upgradeDamageButton;
    [SerializeField] private Button healCastleButton;
    [SerializeField] private Button nextWaveButton;

    private int currentWave = 1;
    private int currentKills = 0;
    private int requiredKills;
    private bool isWaveComplete = false;
    private ZombieSpawner zombieSpawner;
    private CastleHealth castleHealth;

    private void Start()
    {
        zombieSpawner = FindObjectOfType<ZombieSpawner>();
        castleHealth = FindObjectOfType<CastleHealth>();
        requiredKills = baseZombiesPerWave;

        if (castleHealth == null)
        {
            Debug.LogError("WaveManager: CastleHealth component not found in scene!");
        }

        // Setup UI
        UpdateWaveCounter();
        UpdateKillCounter();
        UpdateHealButtonInteractivity();
        waveCompletePanel.SetActive(false);

        // Add button listeners
        upgradeDamageButton.onClick.AddListener(OnUpgradeDamageClicked);
        healCastleButton.onClick.AddListener(OnHealCastleClicked);
        nextWaveButton.onClick.AddListener(OnNextWaveClicked);
    }

    public void OnZombieKilled()
    {
        currentKills++;
        UpdateKillCounter();

        if (currentKills >= requiredKills && !isWaveComplete)
        {
            CompleteWave();
        }
    }

    private void CompleteWave()
    {
        isWaveComplete = true;

        // Stop zombie spawning
        if (zombieSpawner != null)
        {
            zombieSpawner.StopSpawning();
        }

        // Update heal button interactivity
        UpdateHealButtonInteractivity();

        // Show wave complete panel
        waveCompletePanel.SetActive(true);
    }

    private void UpdateHealButtonInteractivity()
    {
        if (healCastleButton != null && castleHealth != null)
        {
            // Disable heal button if castle is at max health
            healCastleButton.interactable = castleHealth.CurrentHealth < castleHealth.MaximumHealth;

            // Update the button text to show potential healing
            TextMeshProUGUI buttonText = healCastleButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Heal Castle (+{healAmount} HP)";
            }
        }
    }

    private void UpdateKillCounter()
    {
        killCounterText.text = $"Kills: {currentKills}/{requiredKills}";
    }

    private void UpdateWaveCounter()
    {
        waveCounterText.text = $"Wave {currentWave}";
    }

    private void StartNextWave()
    {
        // Hide panel
        waveCompletePanel.SetActive(false);

        // Increment wave
        currentWave++;
        UpdateWaveCounter();

        // Calculate new required kills
        requiredKills = baseZombiesPerWave + (zombieIncreasePerWave * (currentWave - 1));

        // Reset for new wave
        currentKills = 0;
        isWaveComplete = false;
        UpdateKillCounter();

        // Resume spawning
        if (zombieSpawner != null)
        {
            zombieSpawner.StartSpawning();
        }
    }

    // Button click handlers
    private void OnUpgradeDamageClicked()
    {
        Debug.Log("Upgrade Damage clicked - Feature to be implemented");
    }

    private void OnHealCastleClicked()
    {
        if (castleHealth != null && castleHealth.CurrentHealth < castleHealth.MaximumHealth)
        {
            castleHealth.Heal(healAmount);
            Debug.Log($"Castle healed for {healAmount} HP. Current health: {castleHealth.CurrentHealth}");

            // Update button state
            UpdateHealButtonInteractivity();

            // Start next wave
            StartNextWave();
        }
    }

    private void OnNextWaveClicked()
    {
        StartNextWave();
    }
}