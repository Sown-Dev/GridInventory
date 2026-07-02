

using System;
using UnityEngine;


[CreateAssetMenu(fileName = "Gun", menuName = "Inventory/Weapon Component")]

public class WeaponComponentDefinition : DurabilityComponentDefinition
{
    public int MagSize = 30;
    public int baseDamage = 10;
    public float baseFireRate = 300;
    
    
    public WeaponFireMode fireMode = WeaponFireMode.Semi;
    public float spreadDegrees = 6f;
    public float recoilStrengthVertical = 1f;    // drives upward/angular kick (the arc/climb)
    public float recoilStrengthHorizontal = 1f;  // drives outward kickback (away from player, along aim line)
    public float recoilRecovery = 1f;            // still shared — how fast both settle back down// scales how fast kickback/rotation/spread settle back down
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
