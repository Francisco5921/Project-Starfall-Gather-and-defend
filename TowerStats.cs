using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerStats : MonoBehaviour
{
    [SerializeField] public int maxHealth = 100; // Maximum health of the structure
    [SerializeField] private int damage = 10;

    // Initial cost to build
    [SerializeField] public int woodCost = 10;
    [SerializeField] public int metalCost = 10;
    [SerializeField] public int fuelCost = 10;
    [SerializeField] public int electronicsCost = 10;


    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth; // Initialize current health
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

    void AttackEnemy(GameObject enemy)
    {
        EnemyMovement enemyHealth = enemy.GetComponent<EnemyMovement>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage); // Deal damage to the enemy                                    
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

}
