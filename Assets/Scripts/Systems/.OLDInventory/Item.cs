using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item: ScriptableObject
{
    //public string name;
    public Sprite Icon;

    public int Width;
    public int Height;

    public int maxStack = 1;

 
}