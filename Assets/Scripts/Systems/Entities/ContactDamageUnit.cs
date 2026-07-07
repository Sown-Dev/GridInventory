using UnityEngine;

namespace Systems.Entities
{
    public class ContactDamageUnit: Unit
    {
        public int contactDamage = 10;
        
        
        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(contactDamage);
                //knockback here
                rb.AddForce(-collision.contacts[0].normal * 5f, ForceMode2D.Impulse);
                rb.AddForce( Vector2.up * 2f, ForceMode2D.Impulse);
            }
            //todo: add knockback to the other unit and self
        }
    }
}