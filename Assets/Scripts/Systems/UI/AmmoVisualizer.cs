
    using UnityEngine;
    using UnityEngine.UI;

    public class AmmoVisualizer: MonoBehaviour
    {
        public int maxAmmo;
        public int currentAmmo;

        public RectTransform ammoList;

        public GameObject ammoIconPrefab;

        public void SetAmmo(int ammo)
        {
            SetAmmo(ammo, maxAmmo);
        }
        
        public void SetAmmo(int ammo, int max)
        {
            currentAmmo = ammo;
            maxAmmo = max;
            foreach( Transform child in ammoList)
            {
                GameObject.Destroy(child.gameObject);
            }

            for (int i = 0; i < maxAmmo; i++)
            {
                Image icon = GameObject.Instantiate(ammoIconPrefab, ammoList).GetComponentInChildren<Image>();
                if (i < currentAmmo)
                {
                    icon.color = Color.white;
                }
                else
                {
                    icon.color = Color.black;
                }
            }
        }
    }
