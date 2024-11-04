using UnityEngine;
using System.Collections;

public class HeroKnight : MonoBehaviour
{
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] GameObject m_slideDust;

    [Header("Combat Settings")]
    [SerializeField] private Transform m_swordHitbox;
    [SerializeField] private float m_attackRange = 0.5f;
    [SerializeField] private LayerMask m_enemyLayers;
    [SerializeField] private int m_baseDamage = 50;
    [SerializeField] private int m_damageUpgradeAmount = 25;

    // Property to access current damage
    public int CurrentDamage { get; private set; }

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private bool m_grounded = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private int m_upgradeLevel = 0;

    private readonly int m_IsRunning = Animator.StringToHash("IsRunning");

    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        CurrentDamage = m_baseDamage;

        // Set gravity scale to 0 to allow free vertical movement
        m_body2d.gravityScale = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        // Calculate movement vector
        Vector2 movement = new Vector2(inputX, inputY).normalized;

        // Move
        m_body2d.velocity = movement * m_speed;

        // Swap direction of sprite depending on walk direction
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

        // -- Handle Animations --
        // Death
        if (Input.GetKeyDown("e"))
        {
            m_animator.SetTrigger("Death");
        }

        // Attack
        else if(Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && canAttack)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;

            // Perform the attack
            Attack();
        }

        // Set running animation based on movement
        bool isMoving = movement.magnitude > Mathf.Epsilon;
        m_animator.SetBool(m_IsRunning, isMoving);    }

    private bool canAttack = true;

    public void EnableAttacking()
    {
        canAttack = true;
        Debug.Log("HeroKnight: Attacking enabled");
    }

    public void DisableAttacking()
    {
        canAttack = false;
        Debug.Log("HeroKnight: Attacking disabled");
    }

    void Attack()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager instance is null!");
            return;
        }

        // Play sword swing sound
        AudioManager.Instance.PlaySoundEffect("SwordSwing");

        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(m_swordHitbox.position, m_attackRange, m_enemyLayers);

        // Damage them
        foreach(Collider2D enemy in hitEnemies)
        {
            ZombieHealth zombieHealth = enemy.GetComponent<ZombieHealth>();
            if (zombieHealth != null)
            {
                zombieHealth.TakeDamage(CurrentDamage);
                Debug.Log($"Hit zombie with {CurrentDamage} damage!");
            }
        }
    }

    public void UpgradeDamage()
    {
        m_upgradeLevel++;
        CurrentDamage = m_baseDamage + (m_damageUpgradeAmount * m_upgradeLevel);
        Debug.Log($"Sword damage upgraded to {CurrentDamage}!");
    }

    public string GetDamageUpgradeText()
    {
        return $"Upgrade Sword DMG (+{m_damageUpgradeAmount})";
    }

    // Draw the attack range in the editor
    void OnDrawGizmosSelected()
    {
        if (m_swordHitbox == null)
            return;

        Gizmos.DrawWireSphere(m_swordHitbox.position, m_attackRange);
    }

    // Animation Events
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
            spawnPosition = m_groundSensor.transform.position;
        else
            spawnPosition = m_groundSensor.transform.position;

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }
}