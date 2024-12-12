using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerStats : MonoBehaviour
{
    public int maxHealth = 100; // Maximum health of the structure


    // Initial cost to build
    public int woodCost = 10;
    public int metalCost = 10;
    public int fuelCost = 10;
    public int electronicsCost = 10;

    public GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    public float fireRate = 0.5f;
    public int bulletDamage = 1;
    [SerializeField] private float shootingRange = 5f;

    // Turret Sprites
    [SerializeField] private Sprite idleSprite;  // Default sprite
    [SerializeField] private Sprite shootSprite; // Sprite to show when firing

    // Main Base Sprites
    [SerializeField] private Sprite damagedSprite; // Sprite for cracks at ≤ 50% health
    [SerializeField] private Sprite criticalSprite; // Sprite for heavy damage at ≤ 25% health
    private SpriteRenderer spriteRenderer;       // Reference to the SpriteRenderer

    private float nextFireTime;
    private Transform targetEnemy;

    [SerializeField] private GameObject healthBarPrefab; // Prefab for the health bar
    private Slider healthBar;
    [SerializeField] private Vector3 healthOffset = new(0, 0, 0);

    [SerializeField] private bool isMainTower = false;
    private int currentHealth;

    [SerializeField] private AudioClip gunfireSound; // Sound effect for shooting
    private AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth; // Initialize current health

        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = gameObject.AddComponent<AudioSource>(); // Add AudioSource component
        audioSource.playOnAwake = false; // Ensure sound doesn't play on Awake

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (healthBarPrefab != null)
        {
            GameObject healthBarObject = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBarObject.transform.SetParent(null);
            healthBar = healthBarObject.GetComponentInChildren<Slider>();
            healthBar.gameObject.SetActive(false); // Initially hidden
        }

    }

    // Call this method to deal damage to the structure
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} took {damageAmount} damage. Current health: {currentHealth}");

        // Check if the structure is below the HP threshold to be destroyed
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        UpdateHealthBar();
        if (isMainTower) UpdateDamageState();
        if (!isMainTower)
        {
            FindTargetEnemy();
            RotateTowardsTarget();
            ShootEnemy();
        }

    }

    // Update health bar visibility and value
    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = (float)currentHealth / maxHealth;
            healthBar.transform.position = transform.position + healthOffset;
            // Show health bar if health is less than max
            healthBar.gameObject.SetActive(currentHealth < maxHealth);
        }
    }

    void UpdateSpriteScales()
    {
        // Match alternate sprites to the main object's scale
        if (spriteRenderer != null)
        {
            spriteRenderer.transform.localScale = transform.localScale;
        }
    }

    void UpdateDamageState()
    {
        if (spriteRenderer != null)
        {
            UpdateSpriteScales(); // Adjust the scale

            if (currentHealth <= maxHealth * 0.25f && criticalSprite != null)
            {
                spriteRenderer.sprite = criticalSprite; // Heavy damage sprite
            }
            else if (currentHealth <= maxHealth * 0.5f && damagedSprite != null)
            {
                spriteRenderer.sprite = damagedSprite; // Cracked sprite
            }
            else
            {
                spriteRenderer.sprite = idleSprite; // Default sprite
            }
        }
    }

    // Method to find the closest enemy within shooting range
    void FindTargetEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance && distanceToEnemy <= shootingRange)
            {
                shortestDistance = distanceToEnemy;
                closestEnemy = enemy.transform;
            }
        }

        targetEnemy = closestEnemy;
    }

    void RotateTowardsTarget()
    {
        if (targetEnemy != null)
        {
            Vector2 direction = (targetEnemy.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f)); // Offset for sprite facing up
        }
    }

    void ShootEnemy()
    {
        if (targetEnemy != null && Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            StartCoroutine(ShootingAnimation());

            if (gunfireSound != null)
            {
                audioSource.PlayOneShot(gunfireSound);
            }


            // Instantiate the bullet
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);

            // Handle single-target bullets
            if (bullet.TryGetComponent<Bullet>(out Bullet bulletScript))
            {
                bulletScript.SetDamage(GetComponent<TowerStats>().bulletDamage);
            }

            // Handle AoE bullets
            if (bullet.TryGetComponent<AOEBullet>(out AOEBullet aoeBulletScript))
            {
                aoeBulletScript.SetDamage(GetComponent<TowerStats>().bulletDamage);
            }

            // Add velocity for single-target or AoE bullets
            if (bullet.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                Vector2 direction = (targetEnemy.position - transform.position).normalized;
                rb.velocity = direction * bulletSpeed;
            }
        }
    }

    IEnumerator ShootingAnimation()
    {
        if (shootSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = shootSprite; // Switch to shooting sprite
            yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds
            spriteRenderer.sprite = idleSprite;  // Revert to idle sprite
        }
    }

    public void Repair()
    {
        // Repair up to maxHealth
        currentHealth = maxHealth;
        Debug.Log($"{gameObject.name} restored to {maxHealth} health. Current health: {currentHealth}");

    }

    //Returns health for use in other scripts
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }


    public int RepairCost(float baseCost)
    {
        // Calculate the maximum repair cost as 40% of the base resource cost
        float maxRepairCost = baseCost * 0.4f;

        // Determine the fraction of health that is missing
        float healthFractionMissing = 1 - GetHealthPercentage();

        // Calculate the repair cost based on how much health is missing
        int repairCost = Mathf.RoundToInt(maxRepairCost * healthFractionMissing);

        // Ensure that the repair cost does not exceed the maximum limit
        if (currentHealth <= maxHealth * 0.2f)
        {
            // Cap the repair cost to the max repair cost when health is at or below 20%
            repairCost = Mathf.Min(repairCost, Mathf.RoundToInt(maxRepairCost));
        }

        return repairCost;
    }


    public int RepairCostWood()
    {
        // Calculate the repair cost as up to 40% of the base wood cost
        return RepairCost(woodCost);
    }

    public int RepairCostMetal()
    {
        // Calculate the repair cost as up to 40% of the base metal cost
        return RepairCost(metalCost);
    }

    public int RepairCostFuel()
    {
        // Calculate the repair cost as up to 40% of the base fuel cost
        return RepairCost(fuelCost);
    }

    public int RepairCostElectronics()
    {
        // Calculate the repair cost as up to 40% of the base electronics cost
        return RepairCost(electronicsCost); ;
    }

    void OnDestroy()
    {
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject); // Destroy the health bar GameObject
        }
    }

}
