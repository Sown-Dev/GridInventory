
using System;
using Newtonsoft.Json;
using Unity.Properties;
using Unity.Serialization;
using Unity.VisualScripting;

[Serializable]
[GeneratePropertyBag]
    public class InventorySlot
    {
        
        //can be null if empty
        public ItemData myItem;

        [DoNotSerialize]
        [JsonIgnore]
        [DontSerialize]
        public Action OnChanged;

        public int maxStackSize = -1;

        public bool IsItemValid(ItemData item)
        {
            return item != null && item.amount > 0;
        }

        public void Clear()
        {
            if (myItem == null)
            {
                return;
            }

            myItem = null;
            OnChanged?.Invoke();
        }

        public bool ConsumeStack(int amount)
        {
            if (myItem == null || amount <= 0)
            {
                return false;
            }

            myItem.amount -= amount;
            if (myItem.amount <= 0)
            {
                Clear();
            }
            else
            {
                OnChanged?.Invoke();
            }

            return true;
        }
        
        
        
        public virtual bool canInsert(ItemData item)
        {
            return IsEmpty() && IsItemValid(item);
        }

        public virtual bool CanInsertIfEmpty(ItemData item)
        {
            return IsEmpty() && IsItemValid(item);
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
            if( myItem == null)
            {
                return true;
            }

            if (myItem.amount <= 0)
            {
                myItem = null;
                return true;
            }
            //ik its stupid to check twice but im just leaving it instead of returning false 
            return myItem == null || myItem.amount <= 0;
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
            if (!IsEmpty())
            {
                return false;
            }

            if (item == null || item.amount <= 0)
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

        public override bool CanInsertIfEmpty(ItemData item)
        {
            if (!IsEmpty() || item == null || item.amount <= 0)
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
