using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int baseZombiesPerWave = 5;
    [SerializeField] private int zombieIncreasePerWave = 5;

    [Header("Castle Healing")]
    [SerializeField] private int healAmount = 10;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveCounterText;
    [SerializeField] private TextMeshProUGUI killCounterText;
    [SerializeField] private GameObject waveCompletePanel;
    [SerializeField] private HeroKnight playerCharacter;
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
        // Play ZombieGrowl sound
        AudioManager.Instance.PlaySoundEffect("ZombieGrowl");

        zombieSpawner = FindObjectOfType<ZombieSpawner>();
        castleHealth = FindObjectOfType<CastleHealth>();
        heroKnight = FindObjectOfType<HeroKnight>();

        // Assign playerCharacter if it's not already assigned in inspector
        if (playerCharacter == null)
        {
            playerCharacter = heroKnight;
            if (playerCharacter == null)
            {
                Debug.LogError("WaveManager: playerCharacter not found!");
            }
        }

        requiredKills = baseZombiesPerWave;

        if (castleHealth == null)
        {
            Debug.LogError("WaveManager: CastleHealth component not found in scene!");
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
            zombieSpawner.DestroyAllZombies();
        }

        UpdateButtonsText();
        waveCompletePanel.SetActive(true);

        if (playerCharacter != null)
        {
            playerCharacter.DisableAttacking();
            Debug.Log("Disabled player attacking");
        }
        else
        {
            Debug.LogError("WaveManager: playerCharacter is null in CompleteWave!");
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
        // Play ZombieGrowl sound
        AudioManager.Instance.PlaySoundEffect("ZombieGrowl");

        waveCompletePanel.SetActive(false);

        if (playerCharacter != null)
        {
            playerCharacter.EnableAttacking();
            Debug.Log("Enabled player attacking");
        }
        else
        {
            Debug.LogError("WaveManager: playerCharacter is null in StartNextWave!");
        }
        currentWave++;
        UpdateWaveCounter();

        requiredKills = baseZombiesPerWave + (zombieIncreasePerWave * (currentWave - 1));

        currentKills = 0;
        isWaveComplete = false;
        UpdateKillCounter();

        if (zombieSpawner != null)
        {
            zombieSpawner.IncreaseDifficulty(currentWave); // Pass the wave number
            zombieSpawner.StartSpawning();
        }
    }

    private void OnUpgradeDamageClicked()
    {
        // Play UpgradeDamage
        AudioManager.Instance.PlaySoundEffect("UpgradeDamage");

        if (heroKnight != null)
        {
            heroKnight.UpgradeDamage();
            UpdateUpgradeButtonText();
            StartNextWave();
        }
    }

    private void OnHealCastleClicked()
    {
        // Play HealCastle
        AudioManager.Instance.PlaySoundEffect("HealCastle");

        if (castleHealth != null && castleHealth.CurrentHealth < castleHealth.MaximumHealth)
        {
            castleHealth.Heal(healAmount);
            UpdateHealButtonText();
            StartNextWave();
        }
    }

    private void OnNextWaveClicked()
    {
        // Play AdvanceWave
        AudioManager.Instance.PlaySoundEffect("AdvanceWave");

        StartNextWave();
    }
}