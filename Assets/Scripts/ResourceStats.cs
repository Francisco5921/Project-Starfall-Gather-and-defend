using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceStats : MonoBehaviour
{

    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int minResourceAmount = 1;
    [SerializeField] private int maxResourceAmount = 5;
    [SerializeField] public string resourceType;

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth; // Initialize current health
    }

    public int Gather()
    {
        if (currentHealth <= 0) return 0;

        currentHealth -= 1; // Reduce health each call

        // Returns random amount of resources to player
        int amount = Random.Range(minResourceAmount + 1, maxResourceAmount);

        if (currentHealth <= 0) // Destroy the object at 0 health
        {
            Destroy(gameObject); 
        }

        return amount; // Returns amount each call
    }


    public int GetCurrentHealth()
    {
        return currentHealth; // Returns health for player's controls
    }
}
