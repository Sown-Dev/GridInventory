using System;

namespace DefaultNamespace.Systems.UI
{
    public class PlayerEquipmentTrifold: UITrifold
    {
        public EquipmentSlotUI slotPrefab;


        public EquipmentSlotUI helmetSlot;
        public EquipmentSlotUI chestSlot;
        public InventorySlotUI weaponSlot1;
        public InventorySlotUI weaponSlot2;
        
        public Player Player;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if( Player != null)
            {
               helmetSlot.BindSlot( Player.HelmetSlot);
               chestSlot.BindSlot( Player.ChestSlot);
                weaponSlot1.BindSlot( Player.WeaponSlot1);
                weaponSlot2.BindSlot( Player.WeaponSlot2);
            }
        }
    }
}