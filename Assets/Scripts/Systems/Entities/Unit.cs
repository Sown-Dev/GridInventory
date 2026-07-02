
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
