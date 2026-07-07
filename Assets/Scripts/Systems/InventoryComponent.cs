using UnityEngine;

public class InventoryComponent : MonoBehaviour
{
    public bool GetsSaved;

    public Inventory inventory;
    public string name;
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;

    public void Awake()
    {
        if (GetsSaved)
        {
            if (PlayerPrefs.HasKey(GetUniqueID()))
            {
                string json = PlayerPrefs.GetString(GetUniqueID());
                inventory = JsonUtility.FromJson<Inventory>(json);
                inventory?.RemoveEmptyStacks();
            }
            else
            {
                InitializeInventory();
            }
        }
        else
        {
            InitializeInventory();
        }
    }

    public void InitializeInventory()
    {
        inventory = new Inventory
        {
            sizeX = this.sizeX,
            sizeY = this.sizeY
        };
    }

    public string GetUniqueID()
    {
        return name;
    }
    private bool opened = false;

    public void OpenInventoryUI()
    {
        UIManager.instance.OpenInventoryUITrifold(inventory);
        opened = true;
    }
    public void CloseInventoryUI()
    {
        UIManager.instance.CloseInventoryUITrifold();
        opened = false;
    }

    public void Save()
    {
        inventory?.RemoveEmptyStacks();
            string json = JsonUtility.ToJson(inventory);
            PlayerPrefs.SetString(GetUniqueID(), json);
            PlayerPrefs.Save();
        
    }
    public void OnApplicationQuit()
    {
        if (GetsSaved)
        {
            Save();
        }
    }
}