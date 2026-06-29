
using System;

    [Serializable]
    public class InventorySlot
    {
        
        //can be null if empty
        public ItemData myItem;
        
        
        public virtual bool canInsert(ItemData item)
        {
            return myItem == null;
        }
        
        public virtual bool Insert(ItemData item)
        {
            if (canInsert(item))
            {
                myItem = item;
                return true;
            }
            return false;
        }
        
    }
    
    //[Serializable]
    public class EquipmentSlot: InventorySlot
    {
        public EquipmentType acceptedTypes;
            
        
        public EquipmentSlot()
        {
            acceptedTypes = EquipmentType.None;
        }
        
        public EquipmentSlot(EquipmentType acceptedTypes)
        {
            this.acceptedTypes = acceptedTypes;
        }
        
        public override bool canInsert(ItemData item)
        {
            if (myItem != null)
            {
                return false;
            }

            if (item.HasComponent<EquipmentItemComponent>())
            {
                EquipmentItemComponent equipComp = (EquipmentItemComponent)item.GetComponent<EquipmentItemComponent>();
                return (acceptedTypes & ((EquipmentComponentDefinition)equipComp.GetDefinition()).equipmentType) != 0;
            }

            return false;
        }
    }
