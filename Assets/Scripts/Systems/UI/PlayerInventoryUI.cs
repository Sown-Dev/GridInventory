using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private GridInventoryUI gridInventoryUI;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private CanvasGroup wholeUiCanvasGroup;

    private Inventory boundInventory;

    private void Awake()
    {
        if (wholeUiCanvasGroup == null)
        {
            wholeUiCanvasGroup = GetComponentInParent<CanvasGroup>(true);
        }

        SetInventoryVisible(false);
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
            gridInventoryUI.BindInventory(player.Inventory);
        }

        if (boundInventory != player.Inventory)
        {
            boundInventory = player.Inventory;
            gridInventoryUI.RebuildView();
        }

        if (inventoryPanel != null && gridInventoryUI.inventory != null)
        {
            RectTransform panelRT = inventoryPanel.GetComponent<RectTransform>();
            if (panelRT != null)
            {
                ApplyPivotFromAnchors(panelRT);

                int gridWidth = gridInventoryUI.inventory.sizeX * gridInventoryUI.cellSize;
                int gridHeight = gridInventoryUI.inventory.sizeY * gridInventoryUI.cellSize;
                panelRT.sizeDelta = new Vector2(gridWidth, gridHeight);
            }
        }
    }

    private static void ApplyPivotFromAnchors(RectTransform rectTransform)
    {
        Vector2 anchorMin = rectTransform.anchorMin;
        Vector2 anchorMax = rectTransform.anchorMax;

        if (!Mathf.Approximately(anchorMin.x, anchorMax.x))
        {
            return;
        }

        if (!Mathf.Approximately(anchorMin.y, anchorMax.y))
        {
            return;
        }

        rectTransform.pivot = new Vector2(anchorMin.x, anchorMin.y);
    }

    private void ToggleInventory()
    {
        if (wholeUiCanvasGroup == null)
        {
            return;
        }

        SetInventoryVisible(!wholeUiCanvasGroup.interactable);
    }

    private void SetInventoryVisible(bool visible)
    {
        if (wholeUiCanvasGroup == null)
        {
            return;
        }

        wholeUiCanvasGroup.alpha = visible ? 1f : 0f;
        wholeUiCanvasGroup.interactable = visible;
        wholeUiCanvasGroup.blocksRaycasts = visible;
    }
}