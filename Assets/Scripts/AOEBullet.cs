using System;
using UnityEngine;

public class AOEBullet : MonoBehaviour
{
    public int damage = 0;
    public float explosionRadius = 2f; // Radius of the AoE effect
    public LayerMask enemyLayer;      // Specify the layer for enemies to avoid affecting unintended objects
    public GameObject explosionEffect;

    [SerializeField] private float lifespan = 1f;

    [SerializeField] private AudioClip explosionSound; // Sound effect for explosion
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>(); // Add AudioSource component
        audioSource.playOnAwake = false;
        Destroy(gameObject, lifespan); // Destroy the bullet after `lifespan` seconds
    }


    public void SetDamage(int towerDamage)
    {
        damage = towerDamage;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<EnemyMovement>(out _))
        {
            // Trigger AoE effect
            Explode();
            // Destroy the bullet
            Destroy(gameObject);
        }
    }

    private void Explode()
    {

        // Play explosion sound
        if (explosionSound != null)
        {
            // Create a temporary GameObject for the sound
            GameObject soundObject = new("ExplosionSound");
            AudioSource tempAudioSource = soundObject.AddComponent<AudioSource>();

            tempAudioSource.clip = explosionSound;
            tempAudioSource.playOnAwake = false;
            tempAudioSource.spatialBlend = 0f; // Ensure it's a 2D sound
            tempAudioSource.Play();

            // Destroy the temporary GameObject after the sound finishes playing
            Destroy(soundObject, explosionSound.length);
        }

        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);

            // Scale the explosion to match the explosionRadius
            if (explosion.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                
                ps.Play();
            }

            Destroy(explosion, 2f); // Destroy the particle system after 2 seconds to clean up
        }


        // Find all objects in the explosion radius on the specified layer
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

        foreach (Collider2D collider in hitEnemies)
        {
            if (collider.TryGetComponent<EnemyMovement>(out EnemyMovement enemy))
            {
                // Apply damage to each enemy
                enemy.TakeDamage(damage);
            }
        }
    }

    // Debugging visualization in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
