
public class AttachmentComponentDefinition: ComponentDefinition
    {
        public Stats stats;
        public AttachmentType attachmentType;
        
        public override ItemComponent GenerateComponents(ItemData itemData)
        {
            return new AttachmentItemComponent
            {
                definitionID = ID,
                myItemData = itemData,
            };
        }
    }

public enum AttachmentType
{
    None=0,
    Scope = 1,
    Barrel = 2,
    Magazine = 4,
    Stock = 8,
}
