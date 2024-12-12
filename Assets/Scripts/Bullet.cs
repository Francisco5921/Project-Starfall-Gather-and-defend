using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 0;
    [SerializeField] private float lifespan = 1f;

    void Start()
    {
        Destroy(gameObject, lifespan); // Destroy the bullet after `lifespan` seconds
    }

    public void SetDamage(int towerDamage)
    {
        damage = towerDamage;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<EnemyMovement>(out EnemyMovement enemy))
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
