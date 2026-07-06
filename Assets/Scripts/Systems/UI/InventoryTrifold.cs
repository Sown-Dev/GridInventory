
    public class InventoryTrifold : UITrifold
    {
        
        public GridInventoryUI gridInventory;
        
        
        public void BindInventory(Inventory inventory)
        {
            gridInventory.BindInventory(inventory);
            
        }
    }
