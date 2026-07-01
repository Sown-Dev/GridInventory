
public class WeaponSlot: InventorySlot
{
            
        
    public WeaponSlot()
    {
    }
        
   
        
    public override bool canInsert(ItemData item)
    {
        if (myItem != null)
        {
            return false;
        }

        if (item.HasComponent<GunItemComponent>())
        {
            return true;
        }

        return false;
    }

    
}