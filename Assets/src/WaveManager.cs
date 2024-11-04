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
    [SerializeField] private GameObject startGamePanel;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button scaredButton;  // New button reference
    [SerializeField] private HeroKnight playerCharacter;
    [SerializeField] private Button upgradeDamageButton;
    [SerializeField] private Button healCastleButton;
    [SerializeField] private Button nextWaveButton;

    private int currentWave = 1;
    private int currentKills = 0;
    private int requiredKills;
    private bool isWaveComplete = false;
    private bool hasGameStarted = false;  // New flag
    private ZombieSpawner zombieSpawner;
    private CastleHealth castleHealth;
    private HeroKnight heroKnight;

    private const string SCARED_TEXT = "I'm Scared";
    private const string TOO_BAD_TEXT = "Too Bad Start The Game";

    private void Start()
    {
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

        // Setup initial UI state
        SetupInitialGameState();

        // Add button listeners
        upgradeDamageButton.onClick.AddListener(OnUpgradeDamageClicked);
        healCastleButton.onClick.AddListener(OnHealCastleClicked);
        nextWaveButton.onClick.AddListener(OnNextWaveClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);
        scaredButton.onClick.AddListener(OnScaredButtonClicked); // Add scared button listener
    }

    private void OnScaredButtonClicked()
{
    if (scaredButton == null)
    {
        Debug.LogError("WaveManager: scaredButton is null!");
        return;
    }

    // Find the TextMeshProUGUI component directly in children
    TextMeshProUGUI[] allTextComponents = scaredButton.GetComponentsInChildren<TextMeshProUGUI>();
    TextMeshProUGUI buttonText = null;

    // Find the first TextMeshProUGUI component
    foreach (TextMeshProUGUI textComponent in allTextComponents)
    {
        if (textComponent != null)
        {
            buttonText = textComponent;
            break;
        }
    }

    if (buttonText != null)
    {
        Debug.Log($"Current button text: {buttonText.text}"); // Debug log
        buttonText.text = TOO_BAD_TEXT;
        Debug.Log("Changed button text to: " + TOO_BAD_TEXT); // Debug log
    }
    else
    {
        Debug.LogError("WaveManager: No TextMeshProUGUI component found on scaredButton!");
    }
}

    private void SetupInitialGameState()
    {
        // Setup UI
        UpdateWaveCounter();
        UpdateKillCounter();
        UpdateButtonsText();

        // Hide wave complete panel
        waveCompletePanel.SetActive(false);

        // Show start game panel
        startGamePanel.SetActive(true);

        // Disable player attacking until game starts
        if (playerCharacter != null)
        {
            playerCharacter.DisableAttacking();
        }

        // Stop zombie spawner
        if (zombieSpawner != null)
        {
            zombieSpawner.StopSpawning();
        }
    }

    private void OnStartGameClicked()
    {
        // Play start game sound
        AudioManager.Instance.PlaySoundEffect("ZombieGrowl");

        // Hide start panel
        startGamePanel.SetActive(false);

        // Enable player attacking
        if (playerCharacter != null)
        {
            playerCharacter.EnableAttacking();
        }

        // Start zombie spawning
        if (zombieSpawner != null)
        {
            zombieSpawner.StartSpawning();
        }

        hasGameStarted = true;
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
        if (!hasGameStarted) return;  // Ignore kills before game starts

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
            zombieSpawner.IncreaseDifficulty(currentWave);
            zombieSpawner.StartSpawning();
        }
    }

    private void OnUpgradeDamageClicked()
    {
        // Play UpgradeDamage sound
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
        // Play AdvanceWave sound
        AudioManager.Instance.PlaySoundEffect("AdvanceWave");
        StartNextWave();
    }
}