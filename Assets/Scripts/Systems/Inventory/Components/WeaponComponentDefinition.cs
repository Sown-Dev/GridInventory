

using System;
using UnityEngine;


[CreateAssetMenu(fileName = "Gun", menuName = "Inventory/Weapon Component")]

public class WeaponComponentDefinition : DurabilityComponentDefinition
{
    public int MagSize = 30;
    public int baseDamage = 10;
    public float baseFireRate = 300;
    public Inventory Upgrades;

    public int[] CompatibleAmmo;

    public override ItemComponent GenerateComponentS()
    {
        return new GunItemComponent
        {
            definitionID = ID,
            maxDurability = maxDurability,
            durability =  RandomDurability(),
            ammoSlot= new InventorySlot
            {
                maxStackSize = MagSize
            }
        };
    }
}

public class DurabilityComponentDefinition : ComponentDefinition
{
    public int maxDurability;

    public override ItemComponent GenerateComponentS()
    {
        return new DurabilityItemComponent
        {
            definitionID = ID,
            durability = maxDurability,
            maxDurability = maxDurability
        };
    }
    public int RandomDurability()
    {
        return UnityEngine.Random.Range(maxDurability/2, maxDurability);
    }
}
