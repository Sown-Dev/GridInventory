using UnityEngine;

[CreateAssetMenu(fileName = "Knife", menuName = "Inventory/Melee Component")]
public class MeleeComponentDefinition : ComponentDefinition
{
    public float range;
    public float damage;
    public float attackSpeed;
    public Stats equipStats;

    public override ItemComponent GenerateComponents(ItemData itemData)
    {
        return new MeleeItemComponent
        {
            definitionID = ID,
            myItemData = itemData
        };
    }
}



public class MeleeItemComponent : ItemComponent
{


}


public class MeleeSlot : InventorySlot
{


    public override bool canInsert(ItemData item)
    {
        if (myItem != null)
        {
            return false;
        }

        if (item.HasComponent<MeleeItemComponent>())
        {
            return true;
        }

        return false;
    }

    public override bool CanInsertIfEmpty(ItemData item)
    {
        return item != null && item.HasComponent<MeleeItemComponent>();
    }


}