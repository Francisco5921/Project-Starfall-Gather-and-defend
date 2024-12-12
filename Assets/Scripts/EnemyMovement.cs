using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class EnemyMovement : MonoBehaviour
{
    private Transform target; // The base or target position
    [SerializeField] private float speed = 2f; // Movement speed of the enemy
    [SerializeField] private float attackSpeed = 2f; // Time between attacks
    [SerializeField] private float attackRange = 5f; // Aggro range of enemy
    [SerializeField] private int damage = 10; // Damage value of enemy
    [SerializeField] private int maxHealth = 40; // Maximum health of the enemy

    private GameObject currentTower; // The tower the enemy is attacking
    private float attackTimer = 0f;
    private int currentHealth;

    [SerializeField] private GameObject healthBarPrefab; // Prefab for the health bar
    private Slider healthBar;
    [SerializeField] private Vector3 healthOffset = new(0, 0, 0);

    void Start()
    {
        currentHealth = maxHealth; // Initialize current health
        GameObject gameObjectTarget = GameObject.Find("PlayerBase");

        if (healthBarPrefab != null)
        {
            GameObject healthBarObject = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBarObject.transform.SetParent(null);
            healthBar = healthBarObject.GetComponentInChildren<Slider>();
            healthBar.gameObject.SetActive(false); // Initially hidden
        }


        if (gameObjectTarget != null)
        {
            target = gameObjectTarget.transform;
        }
        else
        {
            Debug.LogWarning("Target GameObject not found!");
        }
    }

    void Update()
    {

        UpdateHealthBar();

        if (target == null)
        {
            Destroy(gameObject); // If no base then destroy the enemy
            return;
        }

        if (currentTower == null)
        {
            currentTower = FoundTowers(); // Look for a new tower
            MoveToTarget(); // Move towards the player base
            MoveDirection();
        } 
        else 
        {

            HandleAttack(); // Attack the current tower
            if (currentTower != null) // Ensure currentTower is not null before facing direction
            {
                AttackDirection(currentTower.transform.position);
            }
        }
    }

    // Gets objects tagged as "Tower" and enables the enemy to attack them when in range
    private GameObject FoundTowers()
    {
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (GameObject tower in towers)
        {
            float distanceToTower = Vector2.Distance(transform.position, tower.transform.position); // Checks for tower distance and tower's range
            if (distanceToTower <= attackRange)
            {
                currentTower = tower; // Assign the tower within range
                return tower;         // Exit the loop after finding the first tower
            }
        }
        return null;
    }

    // Allows enemy to transform towards a designated target
    void MoveToTarget()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += speed * Time.deltaTime * (Vector3)direction;
    }

    // Make the enemy face the direction of movement
    void MoveDirection()
    {
        Vector2 direction = (target.position - transform.position).normalized; // Get the direction of movement
        if (direction != Vector2.zero) // Check to avoid NaN issues
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // Calculate the angle
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90)); // Rotate the enemy
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} took {damageAmount} damage. Current health: {currentHealth}");

        // Check if the enemy is below the HP threshold to be destroyed
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
            GameManager.onEnemyDestroy.Invoke();
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

    void HandleAttack()
    {
        attackTimer += Time.deltaTime; // Attacktimer increments over time
        if (attackTimer >= attackSpeed) // Attacks when attacktimer reaches the unit's attackspeed
        {
            AttackTower();
            attackTimer = 0f; // Resets attack timer for the next attack
        }
    }

    // Make the enemy face the target tower
    void AttackDirection(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized; // Get the direction to the tower
        if (direction != Vector2.zero) // Check to avoid NaN issues
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // Calculate the angle
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90)); // Rotate the enemy
        }
    }

    void AttackTower()
    {
        if (currentTower != null)
        {
            Debug.Log("Is attacking");

            // Get the StructureHealth component and deal damage
            //TowerStats towerHealth = currentTower.GetComponent<TowerStats>();  
            if (currentTower.TryGetComponent<TowerStats>(out TowerStats towerHealth))
            {
                towerHealth.TakeDamage(damage); // Deal damage to the tower                                    
            }
            
            // Check if the tower is destroyed 
            if (towerHealth != null && towerHealth.GetCurrentHealth() <= 0)  
            {
                currentTower = null; // Clear reference if the tower is destroyed   
                attackTimer = 0f; // Resets attack timer on destruction
            }
        }
    }

    void OnDestroy()
    {
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject); // Destroy the health bar GameObject
        }
    }
}

