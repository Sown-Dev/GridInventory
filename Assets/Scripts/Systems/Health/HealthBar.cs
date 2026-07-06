using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Damageable damageable;

    [Header("Health Bar Settings")]
    public float offsetY = 1.5f;

    [Header("Show-For-Duration Mode")]
    [Tooltip("If true: bar appears on hit, stays for showDuration, then fades out. If false: bar appears on hit and stays until healed to full.")]
    public bool showForDuration = false;
    public float showDuration = 2f;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.1f;
    public float fadeOutDuration = 0.4f;

    private HealthBarUI healthBarUI;
    private int lastHealth;
    private bool hasBeenDamaged;

    private void Start()
    {
        if (damageable == null)
        {
            damageable = GetComponent<Damageable>();
        }

        if (HealthBarManager.Instance != null)
        {
            healthBarUI = HealthBarManager.Instance.Get();
        }

        if (damageable != null)
        {
            lastHealth = damageable.health;
            hasBeenDamaged = damageable.health < damageable.maxHealth;

            if (healthBarUI != null)
            {
                healthBarUI.SetFillImmediate((float)damageable.health / damageable.maxHealth);
                healthBarUI.SetAlphaImmediate(hasBeenDamaged && !showForDuration ? 1f : 0f);
            }
        }
    }

    private void Update()
    {
        if (damageable == null || healthBarUI == null) return;

        if (lastHealth != damageable.health)
        {
            int previousHealth = lastHealth;
            lastHealth = damageable.health;
            healthBarUI.AnimateFillTo((float)damageable.health / damageable.maxHealth);
            OnHealthChanged(previousHealth, damageable.health);
        }
    }

    private void LateUpdate()
    {
        if (healthBarUI == null) return;

        Vector3 worldPos = transform.position + Vector3.up * offsetY;
        healthBarUI.SetWorldPosition(worldPos);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(HideHealthBar));
    }

    private void OnDestroy()
    {
        if (healthBarUI != null && HealthBarManager.Instance != null)
        {
            HealthBarManager.Instance.Release(healthBarUI);
            healthBarUI = null;
        }
    }

    private void OnHealthChanged(int previousHealth, int currentHealth)
    {
        bool tookDamage = currentHealth < previousHealth;
        bool isFullHealth = currentHealth >= damageable.maxHealth;

        if (tookDamage)
        {
            healthBarUI.Flash();
        }

        if (currentHealth < damageable.maxHealth)
        {
            hasBeenDamaged = true;
        }

        if (showForDuration)
        {
            if (tookDamage)
            {
                CancelInvoke(nameof(HideHealthBar));
                ShowHealthBar();
                Invoke(nameof(HideHealthBar), showDuration);
            }
        }
        else
        {
            if (isFullHealth)
            {
                hasBeenDamaged = false;
                HideHealthBar();
            }
            else if (hasBeenDamaged)
            {
                ShowHealthBar();
            }
        }
    }

    public void ShowHealthBar() => healthBarUI?.FadeTo(1f, fadeInDuration);
    public void HideHealthBar() => healthBarUI?.FadeTo(0f, fadeOutDuration);
}