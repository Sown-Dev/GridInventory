using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : InventorySlotUI
{
    [SerializeField] private EquipmentType acceptedTypes = EquipmentType.None;

    private EquipmentSlot equipmentSlot = new EquipmentSlot();
    
    
    protected override void Awake()
    {
        base.Awake();
        equipmentSlot.acceptedTypes = acceptedTypes;
        slot = equipmentSlot;
    }

    public override void RefreshView()
    {
        base.RefreshView();
        /* if(slot.myItem != null)
        {
            backgroundImage.gameObject.SetActive(false);
        }
        else
        {
            backgroundImage.gameObject.SetActive(true);

        }*/
    }

    public override bool TryPlaceItem(ItemData item, Vector2 screenPosition)
    {
        equipmentSlot.acceptedTypes = acceptedTypes;
        return base.TryPlaceItem(item, screenPosition);
    }

    public override bool TrySwapItem(ItemData item, IItemContainerUI sourceContainer)
    {
        equipmentSlot.acceptedTypes = acceptedTypes;
        return base.TrySwapItem(item, sourceContainer);
    }

    public override bool CanAcceptItem(ItemData item, Vector2 screenPosition)
    {
        equipmentSlot.acceptedTypes = acceptedTypes;
        return base.CanAcceptItem(item, screenPosition);
    }
}