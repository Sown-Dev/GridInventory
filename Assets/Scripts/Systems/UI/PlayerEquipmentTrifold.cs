using System;

namespace DefaultNamespace.Systems.UI
{
    public class PlayerEquipmentTrifold: UITrifold
    {
        public EquipmentSlotUI slotPrefab;


        public EquipmentSlotUI helmetSlot;
        public EquipmentSlotUI chestSlot;
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
            }
        }
    }
}