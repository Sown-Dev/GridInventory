using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 velocity;
    private float damage;

    public void Init(Vector2 initialVelocity, float projectileDamage, Quaternion rotation)
    {
        velocity = initialVelocity;
        damage = projectileDamage;
        transform.rotation = rotation;
    }

    void Update()
    {
        // Move the projectile every frame based on its velocity
        transform.Translate(velocity * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Apply your damage logic here using the 'damage' variable
        // Then destroy the projectile
        Destroy(gameObject);
    }
}