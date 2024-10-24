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
    private HeroKnight heroKnight;

    private void Start()
    {
        zombieSpawner = FindObjectOfType<ZombieSpawner>();
        castleHealth = FindObjectOfType<CastleHealth>();
        heroKnight = FindObjectOfType<HeroKnight>();
        requiredKills = baseZombiesPerWave;

        if (castleHealth == null)
        {
            Debug.LogError("WaveManager: CastleHealth component not found in scene!");
        }

        if (heroKnight == null)
        {
            Debug.LogError("WaveManager: HeroKnight component not found in scene!");
        }

        // Setup UI
        UpdateWaveCounter();
        UpdateKillCounter();
        UpdateButtonsText();
        waveCompletePanel.SetActive(false);

        // Add button listeners
        upgradeDamageButton.onClick.AddListener(OnUpgradeDamageClicked);
        healCastleButton.onClick.AddListener(OnHealCastleClicked);
        nextWaveButton.onClick.AddListener(OnNextWaveClicked);
    }

    private void UpdateButtonsText()
    {
        UpdateHealButtonText();
        UpdateUpgradeButtonText();
    }

    private void UpdateHealButtonText()
    {
        if (healCastleButton != null)
        {
            TextMeshProUGUI buttonText = healCastleButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Heal Castle (+{healAmount} HP)";
            }

            healCastleButton.interactable = castleHealth != null &&
                                          castleHealth.CurrentHealth < castleHealth.MaximumHealth;
        }
    }

    private void UpdateUpgradeButtonText()
    {
        if (upgradeDamageButton != null && heroKnight != null)
        {
            TextMeshProUGUI buttonText = upgradeDamageButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = heroKnight.GetDamageUpgradeText();
            }
        }
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

        if (zombieSpawner != null)
        {
            zombieSpawner.StopSpawning();
        }

        UpdateButtonsText();
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

    private void StartNextWave()
    {
        waveCompletePanel.SetActive(false);
        currentWave++;
        UpdateWaveCounter();

        requiredKills = baseZombiesPerWave + (zombieIncreasePerWave * (currentWave - 1));

        currentKills = 0;
        isWaveComplete = false;
        UpdateKillCounter();

        if (zombieSpawner != null)
        {
            zombieSpawner.StartSpawning();
        }
    }

    private void OnUpgradeDamageClicked()
    {
        if (heroKnight != null)
        {
            heroKnight.UpgradeDamage();
            UpdateUpgradeButtonText();
            StartNextWave();
        }
    }

    private void OnHealCastleClicked()
    {
        if (castleHealth != null && castleHealth.CurrentHealth < castleHealth.MaximumHealth)
        {
            castleHealth.Heal(healAmount);
            UpdateHealButtonText();
            StartNextWave();
        }
    }

    private void OnNextWaveClicked()
    {
        StartNextWave();
    }
}