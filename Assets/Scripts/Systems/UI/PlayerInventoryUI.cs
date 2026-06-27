using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private GridInventoryUI gridInventoryUI;
    [SerializeField] private GameObject inventoryPanel;

    private void Awake()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void Start()
    {
        BindInventory();
    }

    private void Update()
    {
        BindInventory();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void BindInventory()
    {
        if (gridInventoryUI == null && inventoryPanel != null)
        {
            gridInventoryUI = inventoryPanel.GetComponentInChildren<GridInventoryUI>(true);
        }

        if (player == null || gridInventoryUI == null || player.Inventory == null)
        {
            return;
        }

        if (gridInventoryUI.inventory != player.Inventory)
        {
            gridInventoryUI.inventory = player.Inventory;
        }

        if (inventoryPanel != null && gridInventoryUI.inventory != null)
        {
            RectTransform panelRT = inventoryPanel.GetComponent<RectTransform>();
            if (panelRT != null)
            {
                int gridWidth = gridInventoryUI.inventory.sizeX * gridInventoryUI.cellSize;
                int gridHeight = gridInventoryUI.inventory.sizeY * gridInventoryUI.cellSize;
                panelRT.sizeDelta = new Vector2(gridWidth, gridHeight);
            }
        }
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            return;
        }

        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }
}