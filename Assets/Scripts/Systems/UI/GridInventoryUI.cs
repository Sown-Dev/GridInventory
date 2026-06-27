using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridInventoryUI : MonoBehaviour
{
    public enum HighlightState
    {
        Invalid,
        Valid,
        Special
    }

    public Inventory inventory;
    public GameObject itemUIPrefab;
    public RectTransform gridPanel;
    public Canvas rootCanvas;
    public int cellSize = 64;

    [Header("Highlight Colors")]
    public Color defaultColor = new Color(1f, 1f, 1f, 0f);
    public Color validColor   = new Color(0f, 1f, 0f, 0.3f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.3f);
    public Color specialColor = new Color(1f, 0.9f, 0f, 0.35f);

    private Image[] cellHighlights;
    private Dictionary<ItemData, ItemUI> itemUIMap = new Dictionary<ItemData, ItemUI>();

    void OnEnable()
    {
        BuildCellHighlights();
        foreach (ItemData item in inventory.inv)
            AddItemUI(item);
    }

    void OnDisable()
    {
        foreach (var ui in itemUIMap.Values)
            if (ui != null) Destroy(ui.gameObject);
        itemUIMap.Clear();

        if (cellHighlights != null)
            foreach (var h in cellHighlights)
                if (h != null) Destroy(h.gameObject);

        cellHighlights = null;
    }

    void BuildCellHighlights()
    {
        cellHighlights = new Image[inventory.sizeX * inventory.sizeY];

        gridPanel.sizeDelta = new Vector2(cellSize * inventory.sizeX, cellSize * inventory.sizeY);
        
        for (int y = 0; y < inventory.sizeY; y++)
        {
            for (int x = 0; x < inventory.sizeX; x++)
            {
                GameObject go = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(gridPanel, false);

                RectTransform rt  = go.GetComponent<RectTransform>();
                rt.anchorMin      = new Vector2(0, 1);
                rt.anchorMax      = new Vector2(0, 1);
                rt.pivot          = new Vector2(0, 1);
                rt.sizeDelta      = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);

                Image img         = go.GetComponent<Image>();
                img.color         = defaultColor;
                img.raycastTarget = false;

                cellHighlights[y * inventory.sizeX + x] = img;
            }
        }
    }

    public void UpdateHighlights(ItemData item, HighlightState state)
    {
        ClearHighlights();

        int w = item.rotated ? item.sizeY : item.sizeX;
        int h = item.rotated ? item.sizeX : item.sizeY;
        Color c = state == HighlightState.Valid
            ? validColor
            : state == HighlightState.Special
                ? specialColor
                : invalidColor;

        for (int x = item.posX; x < item.posX + w; x++)
            for (int y = item.posY; y < item.posY + h; y++)
                if (x >= 0 && x < inventory.sizeX && y >= 0 && y < inventory.sizeY)
                    cellHighlights[y * inventory.sizeX + x].color = c;
    }

    public void ClearHighlights()
    {
        if (cellHighlights == null) return;
        foreach (var h in cellHighlights)
            h.color = defaultColor;
    }

    public void AddItemUI(ItemData item)
    {
        GameObject go = Instantiate(itemUIPrefab, gridPanel);
        ItemUI ui     = go.GetComponent<ItemUI>();
        ui.Init(item, this);
        SnapToGrid(ui.RectTransform, item);
        itemUIMap[item] = ui;
    }

    public void RebuildItemUIs()
    {
        foreach (var ui in itemUIMap.Values)
        {
            if (ui != null)
            {
                Destroy(ui.gameObject);
            }
        }

        itemUIMap.Clear();

        foreach (ItemData item in inventory.inv)
        {
            AddItemUI(item);
        }
    }

    public void RemoveItemUI(ItemData item)
    {
        if (!itemUIMap.TryGetValue(item, out ItemUI ui)) return;
        Destroy(ui.gameObject);
        itemUIMap.Remove(item);
    }

    // Public because ItemUI calls this after a drag resolves
    public void SnapToGrid(RectTransform rt, ItemData item)
    {
        rt.anchorMin      = new Vector2(0, 1);
        rt.anchorMax      = new Vector2(0, 1);
        rt.pivot          = new Vector2(0, 1);
        int w             = item.rotated ? item.sizeY : item.sizeX;
        int h             = item.rotated ? item.sizeX : item.sizeY;
        rt.sizeDelta      = new Vector2(w * cellSize, h * cellSize);
        rt.anchoredPosition = new Vector2(item.posX * cellSize, -item.posY * cellSize);
    }

    public bool TryPlaceItem(ItemData item)
    {
        if (!inventory.PlaceItem(item)) return false;
        AddItemUI(item);
        return true;
    }

    public void TryRemoveItem(ItemData item)
    {
        if (!inventory.RemoveItem(item)) return;
        RemoveItemUI(item);
    }

    public Vector2Int ScreenToCell(Vector2 screenPos)
    {
        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridPanel, screenPos, cam, out Vector2 local))
        {
            return new Vector2Int(-1, -1);
        }

        int cellX = Mathf.FloorToInt(local.x / cellSize);
        int cellY = Mathf.FloorToInt(-local.y / cellSize);
        
        return new Vector2Int(cellX, cellY);
    }
}