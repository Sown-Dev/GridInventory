
using System;
using Unity.VisualScripting;

[Serializable]
    public class InventorySlot
    {
        
        //can be null if empty
        public ItemData myItem;

        [DoNotSerialize]
        public Action OnChanged;

        public int maxStackSize = -1;
        
        public virtual bool canInsert(ItemData item)
        {
            return myItem == null;
        }
        
        public virtual bool Insert(ItemData item)
        {
            if (canInsert(item))
            {
                myItem = item;
                OnChanged?.Invoke();
                return true;
            }
            return false;
        }
        public bool IsEmpty()
        {
            return myItem == null;
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

        public EquipmentComponentDefinition GetDefinition()
        {
            if (myItem != null && myItem.HasComponent<EquipmentItemComponent>())
            {
                EquipmentItemComponent equipComp = (EquipmentItemComponent)myItem.GetComponent<EquipmentItemComponent>();
                return (EquipmentComponentDefinition)equipComp.GetDefinition();
            }
            return null;
        }
    }
