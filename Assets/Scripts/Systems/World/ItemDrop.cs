public class ItemDrop: Interactable{
    public ItemData item;
    

    public void Init( ItemData drop){
        item=drop;
        sr.sprite=drop.GetIcon();

    }

    public override void Interact(){

        if(Player.instance.Inventory.AddItem(item)){
            base.Interact();
        }
    }
}