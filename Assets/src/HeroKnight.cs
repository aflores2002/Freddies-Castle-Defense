using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// HeroKnight class handles player character movement, combat, and animations
public class HeroKnight : MonoBehaviour
{
    // Movement-related serialized fields
    [SerializeField] float m_speed = 4.0f; // Base movement speed for character

    [Header("Combat Settings")]
    [SerializeField] private Transform m_swordHitbox;        // Position for sword attack hitbox
    [SerializeField] private float m_attackRange = 0.5f;     // Radius of attack circle
    [SerializeField] private LayerMask m_enemyLayers;        // Layers to check for enemies during attacks
    [SerializeField] private int m_baseDamage = 50;          // Starting damage value
    [SerializeField] private int m_damageUpgradeAmount = 25; // Amount damage increases per upgrade

    [Header("Sound Settings")]
    [SerializeField] private float stepRate = 0.05f;   // Time between footstep sounds
    private float lastStepTime;                        // Tracks when the last footstep sound played

    // Public property to access current damage value
    public int CurrentDamage { get; private set; }

    // Component references and state variables
    private Animator m_animator;              // Reference to the character's animator
    private Rigidbody2D m_body2d;             // Reference to the character's rigidbody
    private Sensor_HeroKnight m_groundSensor; // Reference to ground detection sensor
    private int m_facingDirection = 1;        // 1 for right, -1 for left
    private int m_currentAttack = 0;          // Tracks current attack in combo (1-3)
    private float m_timeSinceAttack = 0.0f;   // Timer for attack combo system
    private int m_upgradeLevel = 0;           // Tracks number of damage upgrades
    private bool canAttack = true;            // Controls whether attacks are allowed

    // Cached animator parameter hash for better performance
    private readonly int m_IsRunning = Animator.StringToHash("IsRunning");

    void Start()
    {
        // Initialize component references
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        CurrentDamage = m_baseDamage;             // Set initial damage value
        lastStepTime = Time.time;                 // Initialize footstep timer

        // Disable gravity for top-down movement
        m_body2d.gravityScale = 0f;
    }

    // Handles playing footstep sounds based on movement
    private void HandleFootsteps()
    {
        float timeSinceLastStep = Time.time - lastStepTime;

        // Only play footstep if enough time has passed since last step
        if (timeSinceLastStep >= stepRate)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySoundEffect("Step");
                lastStepTime = Time.time;
                Debug.Log($"Step played. Rate: {stepRate}, Time since last: {timeSinceLastStep:F3}");
            }
        }
    }

    void Update()
    {
        // Update attack combo timer
        m_timeSinceAttack += Time.deltaTime;

        // Get input for movement
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        // Normalize diagonal movement to prevent faster diagonal speed
        Vector2 movement = new Vector2(inputX, inputY).normalized;

        // Apply movement to rigidbody
        m_body2d.velocity = movement * m_speed;

        // Handle footstep sounds when moving
        bool isMoving = movement.magnitude > Mathf.Epsilon;
        if (isMoving)
        {
            HandleFootsteps();
        }

        // Update character facing direction based on horizontal input
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // Update running animation state
        m_animator.SetBool(m_IsRunning, isMoving);

        // Handle death animation trigger
        if (Input.GetKeyDown("e"))
        {
            m_animator.SetTrigger("Death");
        }

        // Handle attack input and combo system
        else if(Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && canAttack)
        {
            m_currentAttack++;

            // Reset combo after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset combo if too much time has passed
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Trigger appropriate attack animation
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset attack timer
            m_timeSinceAttack = 0.0f;

            // Execute attack logic
            Attack();
        }
    }

    // Enables attack functionality (called by WaveManager)
    public void EnableAttacking()
    {
        canAttack = true;
        Debug.Log("HeroKnight: Attacking enabled");
    }

    // Disables attack functionality (called by WaveManager)
    public void DisableAttacking()
    {
        canAttack = false;
        Debug.Log("HeroKnight: Attacking disabled");
    }

    // Handles attack logic and damage done
    void Attack()
    {
        // Verify AudioManager exists
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager instance is null!");
            return;
        }

        // Play attack sound effect
        AudioManager.Instance.PlaySoundEffect("SwordSwing");

        Debug.Log($"Attack initiated with damage: {CurrentDamage}");

        // Use HashSet to prevent hitting the same zombie multiple times in one attack
        HashSet<GameObject> hitZombies = new HashSet<GameObject>();

        // Check for enemies within attack range
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(m_swordHitbox.position, m_attackRange, m_enemyLayers);
        Debug.Log($"Found {hitColliders.Length} colliders in range");

        // Apply damage to each unique zombie hit
        foreach(Collider2D collider in hitColliders)
        {
            if (hitZombies.Add(collider.gameObject))
            {
                ZombieHealth zombieHealth = collider.gameObject.GetComponent<ZombieHealth>();
                if (zombieHealth != null)
                {
                    zombieHealth.TakeDamage(CurrentDamage);
                    Debug.Log($"Hit zombie with {CurrentDamage} damage!");
                }
            }
        }
    }

    // Increases the knight's attack damage
    public void UpgradeDamage()
    {
        m_upgradeLevel++;
        CurrentDamage = m_baseDamage + (m_damageUpgradeAmount * m_upgradeLevel);
        Debug.Log($"Sword damage upgraded to {CurrentDamage}!");
    }

    // Returns the text to display on the damage upgrade button
    public string GetDamageUpgradeText()
    {
        return $"Upgrade Sword DMG (+{m_damageUpgradeAmount})";
    }
}