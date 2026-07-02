
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private float damage;
    private Rigidbody2D rb;
    
    public Faction faction;

    void Awake()
    {
        // Grab the Rigidbody2D reference as soon as the object is created
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void Init(Vector2 initialVelocity, float projectileDamage, Quaternion rotation, Faction projectileFaction = Faction.Friendly)
    {
        Init( initialVelocity, projectileDamage, rotation, null, projectileFaction);
    }

    public void Init(Vector2 initialVelocity, float projectileDamage, Quaternion rotation, Collider2D caller, Faction projectileFaction=Faction.Friendly)
    {
        // Ignore collisions with the caller
        if (caller != null)
        {
            Physics2D.IgnoreCollision(caller, GetComponent<Collider2D>());
        }
        
        damage = projectileDamage;
        transform.rotation = rotation;
        
        // Apply the velocity directly to the Rigidbody2D
        rb.linearVelocity = initialVelocity;
        
        faction = projectileFaction;
        gameObject.layer =faction.GetLayer();
        //set layer
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Projectile collided with {collision.gameObject.name}");
        if (collision.gameObject.GetComponent<IDamageable>() != null)
        {
            collision.gameObject.GetComponent<IDamageable>().TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}