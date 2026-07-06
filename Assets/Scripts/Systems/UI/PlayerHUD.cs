
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class PlayerHUD : MonoBehaviour
    {
        public Player PlayerRef;
        
        public Image healthBar;
        public TMP_Text healthText;

        public void Update()
        {
            UpdateHUD();
        }

        public void UpdateHUD()
        {
            if (PlayerRef == null) return;

            float healthPercent = PlayerRef.health / (float) PlayerRef.maxHealth;
            healthBar.fillAmount = healthPercent;
            healthText.text = $"{PlayerRef.health}/{PlayerRef.maxHealth}";
        }
    }
