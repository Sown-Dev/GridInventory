public class DurabilityComponentDefinition : ComponentDefinition
{
    public int maxDurability;

    public override ItemComponent GenerateComponents(ItemData itemData)
    {
        return new DurabilityItemComponent
        {
            definitionID = ID,
            durability = maxDurability,
            maxDurability = maxDurability,
            myItemData = itemData,
        };
    }
    public int RandomDurability()
    {
        return UnityEngine.Random.Range(maxDurability/2, maxDurability);
    }
}