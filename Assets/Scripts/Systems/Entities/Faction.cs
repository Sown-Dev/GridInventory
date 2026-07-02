using UnityEngine;

public enum Faction
{
    Friendly,
    Nemesis,
    Drone,
}
public static class FactionExtensions
{
    public static int GetLayer(this Faction faction)
    {
        switch (faction)
        {
            case Faction.Friendly:
                return   LayerMask.NameToLayer("Friendly");
            case Faction.Drone:
                return LayerMask.NameToLayer("Drone");
            case Faction.Nemesis:
                return LayerMask.GetMask("Nemesis");
            default:
                return 0;
        }
    }
}