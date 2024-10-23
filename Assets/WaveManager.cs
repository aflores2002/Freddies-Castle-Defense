using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int baseZombiesPerWave = 10;
    [SerializeField] private int zombieIncreasePerWave = 5;

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

    private void Start()
    {
        zombieSpawner = FindObjectOfType<ZombieSpawner>();
        requiredKills = baseZombiesPerWave;

        // Setup UI
        UpdateWaveCounter();
        UpdateKillCounter();
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

        // Show wave complete panel
        waveCompletePanel.SetActive(true);
    }

    private void UpdateKillCounter()
    {
        killCounterText.text = $"Kills: {currentKills}/{requiredKills}";
    }

    private void UpdateWaveCounter()
    {
        waveCounterText.text = $"Wave {currentWave}";
    }

    // Button click handlers
    private void OnUpgradeDamageClicked()
    {
        Debug.Log("Upgrade Damage clicked - Feature to be implemented");
    }

    private void OnHealCastleClicked()
    {
        Debug.Log("Heal Castle clicked - Feature to be implemented");
    }

    private void OnNextWaveClicked()
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
}