
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private float damage;
    private Rigidbody2D rb;
    
    public Faction faction;

    public GameObject trailChild;

    public GameObject impactEffectPrefab;

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

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Instantly kill physics so the bullet cannot bounce, slide, or deflect
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        // 2. Handle your damage
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }

        // 3. Get the perfect normal and hit point natively from the collision
        ContactPoint2D contact = collision.GetContact(0);

        // 4. Spawn the impact effect facing away from the wall
        GameObject impactGO = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        //impactGO.transform.up = contact.normal;

        // 5. Use your existing method to snap the trail to the exact hit point and destroy
        DestroyProjectile();
    }
    
    public void DestroyProjectile()
    {
        //ensures we see the bullet trail after impact
        trailChild.transform.parent = null; 
        trailChild.transform.position = transform.position;
        Destroy(trailChild, 2f);
        Destroy(gameObject);
        
    }
}