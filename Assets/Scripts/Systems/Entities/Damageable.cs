using UnityEngine;

public class Damagable : MonoBehaviour, IDamageable
{
    public int health;
    
    [field:SerializeField]
    public virtual int maxHealth { get; set; }

    [field: SerializeField]
    public Faction faction { get; set; }

    [Header("Flash Settings")]
    public SpriteRenderer spriteRenderer;
    public float flashRecoverySpeed = 5f; // Higher number means faster fade

    private float currentFlashAmount;
    private MaterialPropertyBlock propertyBlock;
    private static readonly int FlashAmountProp = Shader.PropertyToID("_FlashAmount");

    public virtual void Start()
    {
        health = maxHealth;
        propertyBlock = new MaterialPropertyBlock();
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public virtual void Update()
    {
        // Smoothly decay the flash amount over time
        if (currentFlashAmount > 0)
        {
            currentFlashAmount -= Time.deltaTime * flashRecoverySpeed;
            ApplyFlashAmount();
        }
    }

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
        // Instantly spike the flash to maximum
        currentFlashAmount = 1f; 
        ApplyFlashAmount();
    }

    private void ApplyFlashAmount()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.GetPropertyBlock(propertyBlock);
            
            // Clamp ensures it never drops below 0 visually
            propertyBlock.SetFloat(FlashAmountProp, Mathf.Clamp01(currentFlashAmount)); 
            
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}