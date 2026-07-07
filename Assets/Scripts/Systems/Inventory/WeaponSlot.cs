
public class WeaponSlot: InventorySlot
{
            
        
    public WeaponSlot()
    {
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

        if (item.HasComponent<GunItemComponent>())
        {
            return true;
        }

        return false;
    }

    public override bool CanInsertIfEmpty(ItemData item)
    {
        return IsEmpty() && item != null && item.amount > 0 && item.HasComponent<GunItemComponent>();
    }

    
}