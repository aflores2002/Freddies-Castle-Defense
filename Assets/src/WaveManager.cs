using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// WaveManager handles game progression through waves of zombies, UI updates, and player interactions
public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int baseZombiesPerWave = 5;    // Starting number of zombies in wave 1
    [SerializeField] private int zombieIncreasePerWave = 5; // Additional zombies added each wave

    [Header("Castle Healing")]
    [SerializeField] private int healAmount = 10; // Amount of health restored when healing castle

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveCounterText; // Displays current wave number
    [SerializeField] private TextMeshProUGUI killCounterText; // Displays zombie kill count
    [SerializeField] private GameObject waveCompletePanel;    // Panel shown between waves
    [SerializeField] private GameObject startGamePanel;       // Initial game start panel
    [SerializeField] private Button startGameButton;          // Button to begin the game
    [SerializeField] private Button scaredButton;             // "I'm Scared" button with changing text
    [SerializeField] private HeroKnight playerCharacter;      // Reference to player character
    [SerializeField] private Button upgradeDamageButton;      // Button to upgrade attack damage
    [SerializeField] private Button healCastleButton;         // Button to heal castle
    [SerializeField] private Button nextWaveButton;           // Button to start next wave

    // Game state tracking
    private int currentWave = 1;         // Current wave number
    private int currentKills = 0;        // Zombies kill count in current wave
    private int requiredKills;           // Kills needed to complete wave
    private bool isWaveComplete = false; // Tracks if current wave is complete
    private bool hasGameStarted = false; // Tracks if game has started

    // Component references
    private ZombieSpawner zombieSpawner; // Handles zombie spawning
    private CastleHealth castleHealth;   // Manages castle health
    private HeroKnight heroKnight;       // Player character reference

    // Constants for scared button text
    private const string SCARED_TEXT = "I'm Scared";
    private const string TOO_BAD_TEXT = "Too Bad Start The Game";

    private void Start()
    {
        // Find necessary components in scene
        zombieSpawner = FindObjectOfType<ZombieSpawner>();
        castleHealth = FindObjectOfType<CastleHealth>();
        heroKnight = FindObjectOfType<HeroKnight>();

        // Set up player character reference
        if (playerCharacter == null)
        {
            playerCharacter = heroKnight;
            if (playerCharacter == null)
            {
                Debug.LogError("WaveManager: playerCharacter not found!");
            }
        }

        // Initialize wave requirements
        requiredKills = baseZombiesPerWave;

        // Verify castle health component
        if (castleHealth == null)
        {
            Debug.LogError("WaveManager: CastleHealth component not found in scene!");
        }

        // Initialize game state and UI
        SetupInitialGameState();

        // Set up button click handlers
        SetupButtonListeners();
    }

    // Event handler for I'm Scared button
    private void OnScaredButtonClicked()
    {
        if (scaredButton == null)
        {
            Debug.LogError("WaveManager: scaredButton is null!");
            return;
        }

        // Find text component on button
        TextMeshProUGUI[] allTextComponents = scaredButton.GetComponentsInChildren<TextMeshProUGUI>();
        TextMeshProUGUI buttonText = null;

        // Get first valid text component
        foreach (TextMeshProUGUI textComponent in allTextComponents)
        {
            if (textComponent != null)
            {
                buttonText = textComponent;
                break;
            }
        }

        // Update button text
        if (buttonText != null)
        {
            Debug.Log($"Current button text: {buttonText.text}");
            buttonText.text = TOO_BAD_TEXT;
            Debug.Log("Changed button text to: " + TOO_BAD_TEXT);
        }
        else
        {
            Debug.LogError("WaveManager: No TextMeshProUGUI component found on scaredButton!");
        }
    }

    // Initialize game state and UI elements
    private void SetupInitialGameState()
    {
        UpdateWaveCounter();
        UpdateKillCounter();
        UpdateButtonsText();

        waveCompletePanel.SetActive(false);
        startGamePanel.SetActive(true);

        // Disable player attacks until game starts
        if (playerCharacter != null)
        {
            playerCharacter.DisableAttacking();
        }

        // Prevent zombie spawning until game starts
        if (zombieSpawner != null)
        {
            zombieSpawner.StopSpawning();
        }
    }

    // Start game button handler
    private void OnStartGameClicked()
    {
        AudioManager.Instance.PlaySoundEffect("ZombieGrowl");
        startGamePanel.SetActive(false);

        if (playerCharacter != null)
        {
            playerCharacter.EnableAttacking();
        }

        if (zombieSpawner != null)
        {
            zombieSpawner.StartSpawning();
        }

        hasGameStarted = true;
    }

    // Update all button text
    private void UpdateButtonsText()
    {
        UpdateHealButtonText();
        UpdateUpgradeButtonText();
    }

    // Update heal button text and interactability
    private void UpdateHealButtonText()
    {
        if (healCastleButton != null)
        {
            TextMeshProUGUI buttonText = healCastleButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Heal Castle (+{healAmount} HP)";
            }

            // Only enable heal button if castle isn't at full health
            healCastleButton.interactable = castleHealth != null &&
                                          castleHealth.CurrentHealth < castleHealth.MaximumHealth;
        }
    }

    // Update upgrade button text
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

    // Handle zombie kill events
    public void OnZombieKilled()
    {
        if (!hasGameStarted) return; // Ignore kills before game starts

        currentKills++;
        UpdateKillCounter();

        // Check for wave completion
        if (currentKills >= requiredKills && !isWaveComplete)
        {
            CompleteWave();
        }
    }

    // Handle wave completion
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

    // Update kill counter UI
    private void UpdateKillCounter()
    {
        killCounterText.text = $"Kills: {currentKills}/{requiredKills}";
    }

    // Update wave counter UI
    private void UpdateWaveCounter()
    {
        waveCounterText.text = $"Wave {currentWave}";
    }

    // Initialize next wave
    private void StartNextWave()
    {
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

        // Update wave numbers and requirements
        currentWave++;
        UpdateWaveCounter();
        requiredKills = baseZombiesPerWave + (zombieIncreasePerWave * (currentWave - 1));

        // Reset kill tracking
        currentKills = 0;
        isWaveComplete = false;
        UpdateKillCounter();

        // Start zombie spawning with increased difficulty
        if (zombieSpawner != null)
        {
            zombieSpawner.IncreaseDifficulty(currentWave);
            zombieSpawner.StartSpawning();
        }
    }

    // Upgrade button handler
    private void OnUpgradeDamageClicked()
    {
        AudioManager.Instance.PlaySoundEffect("UpgradeDamage");

        if (heroKnight != null)
        {
            heroKnight.UpgradeDamage();
            UpdateUpgradeButtonText();
            StartNextWave();
        }
    }

    // Heal button handler
    private void OnHealCastleClicked()
    {
        AudioManager.Instance.PlaySoundEffect("HealCastle");

        if (castleHealth != null && castleHealth.CurrentHealth < castleHealth.MaximumHealth)
        {
            castleHealth.Heal(healAmount);
            UpdateHealButtonText();
            StartNextWave();
        }
    }

    // Next wave button handler
    private void OnNextWaveClicked()
    {
        AudioManager.Instance.PlaySoundEffect("AdvanceWave");
        StartNextWave();
    }
}