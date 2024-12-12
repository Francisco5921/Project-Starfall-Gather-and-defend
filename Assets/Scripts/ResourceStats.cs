using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceStats : MonoBehaviour
{

    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int minResourceAmount = 1;
    [SerializeField] private int maxResourceAmount = 5;
    public string resourceType;

    [SerializeField] private AudioClip gatherSound; // Sound effect for explosion
    private AudioSource audioSource;

    private int currentHealth;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>(); // Add AudioSource component
        audioSource.playOnAwake = false;
        currentHealth = maxHealth; // Initialize current health

        StartCoroutine(SelfDestructAfterTime(20f));
    }

    public int Gather()
    {
        if (currentHealth <= 0) return 0;

        if (gatherSound != null)
        {
            audioSource.PlayOneShot(gatherSound);
        }

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

    private IEnumerator SelfDestructAfterTime(float time)
    {
        // Wait for the specified time
        yield return new WaitForSeconds(time);

        // Destroy the resource object
        Destroy(gameObject);
    }
}