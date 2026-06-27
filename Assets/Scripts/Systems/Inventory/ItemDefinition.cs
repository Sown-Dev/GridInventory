using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDefinition", menuName = "Inventory/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public int itemID;
    public int value=10;
    public int sizeX=1;
    public int sizeY=1;
    public int maxAmount=1;
    public Sprite icon;
}