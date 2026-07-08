

using System;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "Gun", menuName = "Inventory/Weapon Component")]

public class WeaponComponentDefinition : DurabilityComponentDefinition
{
    public int MagSize = 30;
    public int baseDamage = 10;
    public float baseFireRate = 300;
    
    
    [SerializeField]
    public Stats baseStats ;

    public WeaponFireMode fireMode = WeaponFireMode.Semi;
    public float baseSpreadDegrees = 20f;            // hard cap on total spread cone
    public float maxSpreadDegrees = 20f;   // ceiling spread can build up to under sustained fire
    public float spreadPerShot = 3f;             // degrees added to spread on every shot fired
    public float spraySpreadBonusPerShot = 0.4f; // EXTRA degrees per shot, scaling with how long you've been spraying
    public float spreadRecoverySpeed = 6f;       // degrees/sec spread decays once you stop firing (separate from recoilRecovery)
    public float recoilStrengthVertical = 1f;    // drives upward/angular kick (the arc/climb)
    public float recoilStrengthHorizontal = 1f;  // drives outward kickback (away from player, along aim line)
    public float recoilRecovery = 1f;            // still shared — how fast both settle back down// scales how fast kickback/rotation/spread settle back down
    public float reloadDuration = 2f;
    
    public Inventory Upgrades;

    public int[] CompatibleAmmo;

    public override ItemComponent GenerateComponents(ItemData itemData)
    {
        return new GunItemComponent
        {
            definitionID = ID,
            maxDurability = maxDurability,
            durability =  RandomDurability(),
            ammoSlot= new InventorySlot
            {
                maxStackSize = MagSize,
                myItem = null,
            },
            myItemData= itemData
        };
    }

    [ContextMenu("Generate Stats")]
    public void GenerateStats()
    {
        baseStats = new Stats();
        baseStats.stats.Add(new Statistic(Statstype.Damage, (Double)baseDamage, Stats.StatsOperation.Add)); 
        baseStats.stats.Add(new Statistic(Statstype.FireRate, (Double)baseFireRate/60, Stats.StatsOperation.Add));
        baseStats.stats.Add(new Statistic(Statstype.MagSize, (Double)MagSize, Stats.StatsOperation.Add));
        baseStats.stats.Add(new Statistic(Statstype.ReloadSpeed, (Double)reloadDuration, Stats.StatsOperation.Add));
    }
    void OnValidate()
    {
        GenerateStats();
    }
}

