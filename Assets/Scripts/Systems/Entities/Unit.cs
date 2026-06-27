
    using DefaultNamespace;
    using UnityEngine;

    public class Unit: MonoBehaviour, IDamageable
    {
        public Rigidbody2D rb;
        
        public int health;
        public int maxHealth;
        [field:SerializeField]
        public Faction faction { get; set; }
        public virtual void TakeDamage(float damage)
        {
            health -= (int)damage;
            if (health <= 0)
            {
                Die();
            }
        }
        
        public virtual void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            health=maxHealth;
        }

        public virtual void Die()
        {
            Destroy(gameObject);
        }
    }
