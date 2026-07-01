using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridInventoryUI : MonoBehaviour, IItemContainerUI
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

    public RectTransform ContainerRect => gridPanel != null ? gridPanel : transform as RectTransform;

    private void Awake()
    {
        if (gridPanel == null)
        {
            gridPanel = transform as RectTransform;
        }

        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>(true);
        }
    }

    void OnEnable()
    {
        RebuildView();
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
        if (gridPanel == null || inventory == null)
        {
            return;
        }

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

    public void BindInventory(Inventory newInventory)
    {
        if (inventory == newInventory && cellHighlights != null)
        {
            return;
        }

        inventory = newInventory;
        RebuildView();
    }

    public void RebuildView()
    {
        if (gridPanel == null || inventory == null || inventory.sizeX <= 0 || inventory.sizeY <= 0)
        {
            return;
        }

        if (cellHighlights != null)
        {
            foreach (var highlight in cellHighlights)
            {
                if (highlight != null)
                {
                    Destroy(highlight.gameObject);
                }
            }
        }

        foreach (var ui in itemUIMap.Values)
        {
            if (ui != null)
            {
                Destroy(ui.gameObject);
            }
        }

        itemUIMap.Clear();
        BuildCellHighlights();

        foreach (ItemData item in inventory.inv)
        {
            AddItemUI(item);
        }
    }

    public bool ContainsScreenPoint(Vector2 screenPosition)
    {
        if (gridPanel == null)
        {
            return false;
        }

        Camera cam = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;

        return RectTransformUtility.RectangleContainsScreenPoint(gridPanel, screenPosition, cam);
    }

    public bool TryRemoveItem(ItemData item)
    {
        return inventory != null && inventory.RemoveItem(item);
    }

    public bool CanAcceptItem(ItemData item, Vector2 screenPosition)
    {
        if (TryGetAmmoLoadTarget(item, screenPosition, out _, out _))
        {
            return true;
        }

        Vector2Int cell = ScreenToCell(screenPosition);
        int originalX = item.posX;
        int originalY = item.posY;

        item.posX = cell.x;
        item.posY = cell.y;

        bool canPlace = inventory != null && (inventory.CanPlace(item) || inventory.CanStackAt(item, cell.x, cell.y));

        item.posX = originalX;
        item.posY = originalY;

        return canPlace;
    }

    public void UpdateDropPreview(ItemData item, Vector2 screenPosition, bool valid)
    {
        if (TryGetAmmoLoadTarget(item, screenPosition, out ItemData weaponItem, out _))
        {
            if (valid)
            {
                UpdateHighlights(weaponItem, HighlightState.Special);
            }
            else
            {
                ClearHighlights();
            }

            return;
        }

        Vector2Int cell = ScreenToCell(screenPosition);
        int originalX = item.posX;
        int originalY = item.posY;

        item.posX = cell.x;
        item.posY = cell.y;

        HighlightState state = valid
            ? (inventory != null && inventory.CanStackAt(item, cell.x, cell.y)
                ? HighlightState.Special
                : HighlightState.Valid)
            : HighlightState.Invalid;

        UpdateHighlights(item, state);

        item.posX = originalX;
        item.posY = originalY;
    }

    public void ClearDropPreview()
    {
        ClearHighlights();
    }

    public bool TryPlaceItem(ItemData item, Vector2 screenPosition)
    {
        if (inventory == null)
        {
            return false;
        }

        if (TryGetAmmoLoadTarget(item, screenPosition, out _, out GunItemComponent gunComponent))
        {
            return gunComponent.TryInsertAmmo(item);
        }

        Vector2Int cell = ScreenToCell(screenPosition);
        item.posX = cell.x;
        item.posY = cell.y;

        return inventory.TryPlaceOrStackAt(item, cell.x, cell.y);
    }

    public bool TryRestoreItem(ItemData item)
    {
        if (inventory == null)
        {
            return false;
        }

        return inventory.PlaceItem(item);
    }

    public void RefreshView()
    {
        RebuildView();
    }

    public void UpdateHighlights(ItemData item, HighlightState state)
    {
        ClearHighlights();

        if (cellHighlights == null || inventory == null)
        {
            return;
        }

        int w = item.rotated ? item.sizeY : item.sizeX;
        int h = item.rotated ? item.sizeX : item.sizeY;
        Color c = state == HighlightState.Valid
            ? validColor
            : state == HighlightState.Special
                ? specialColor
                : invalidColor;

        int startX = Mathf.Max(0, item.posX);
        int startY = Mathf.Max(0, item.posY);
        int endX = Mathf.Min(inventory.sizeX, item.posX + w);
        int endY = Mathf.Min(inventory.sizeY, item.posY + h);

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                int index = y * inventory.sizeX + x;
                if (index >= 0 && index < cellHighlights.Length)
                {
                    cellHighlights[index].color = c;
                }
            }
        }
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

    public Vector2Int ScreenToCell(Vector2 screenPos)
    {
        if (gridPanel == null)
        {
            return new Vector2Int(-1, -1);
        }

        Camera cam = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridPanel, screenPos, cam, out Vector2 local))
        {
            return new Vector2Int(-1, -1);
        }

        int cellX = Mathf.FloorToInt(local.x / cellSize);
        int cellY = Mathf.FloorToInt(-local.y / cellSize);
        
        return new Vector2Int(cellX, cellY);
    }

    private bool TryGetAmmoLoadTarget(ItemData item, Vector2 screenPosition, out ItemData weaponItem, out GunItemComponent gunComponent)
    {
        weaponItem = null;
        gunComponent = null;

        if (inventory == null || item == null)
        {
            return false;
        }

        Vector2Int cell = ScreenToCell(screenPosition);
        if (cell.x < 0 || cell.y < 0)
        {
            return false;
        }

        weaponItem = inventory.GetItemAt(cell.x, cell.y);
        if (weaponItem == null || !weaponItem.HasComponent<GunItemComponent>())
        {
            weaponItem = null;
            return false;
        }

        gunComponent = weaponItem.GetComponent<GunItemComponent>();
        return gunComponent != null && gunComponent.CanAcceptAmmo(item);
    }
}