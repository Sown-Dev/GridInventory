
    using DefaultNamespace;
    using UnityEngine;

    public class Unit: Damagable
    {
        public Rigidbody2D rb;
        
        public override void Start()
        {
            base.Start();
            rb = GetComponent<Rigidbody2D>();
           
        }

    }
    
    public class Damagable : MonoBehaviour, IDamageable
    {
        public int health;
        public virtual int maxHealth { get; set; } 

        [field:SerializeField]
        public Faction faction { get; set; }
        public virtual void TakeDamage(float damage)
        {
            DamageFlash();
            health -= (int)damage;
            if (health <= 0)
            {
                Die();
            }
        }

        public virtual void DamageFlash()
        {
            
        }
        
        public virtual void Start()
        {
            health=maxHealth;
        }

        public virtual void Die()
        {
            Destroy(gameObject);
        }
    }
