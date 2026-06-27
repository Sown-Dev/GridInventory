using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform RectTransform { get; private set; }

    private ItemData item;
    private GridInventoryUI gridUI;
    private RectTransform canvasRect;
    private Vector2 dragOffset;
    private bool isDragging;
    private int origX, origY;
    private int origAmount;
    private bool origRotated;

    public Image itemIcon;
    public TMP_Text stackAmount;
    public TMP_Text weaponText;

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Init(ItemData item, GridInventoryUI gridUI)
    {
        this.item   = item;
        this.gridUI = gridUI;

        ItemDefinition def = ItemRegistry.instance.ByID(item.itemID);
        if (def != null && def.icon != null)
        {
            Image img = itemIcon;
            img.sprite  = def.icon;
            img.color   = Color.white;  // in case it was tinted transparent as a default
            img.SetNativeSize();
        }
        UpdateVisuals();
    }

    void Update()
    {
        if (!isDragging) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            item.rotated = !item.rotated;
            UpdateVisuals();
            UpdateDragPreview();
        }

        UpdateDragPreview();
    }

    void UpdateVisuals()
    {
        int w = item.rotated ? item.sizeY : item.sizeX;
        int h = item.rotated ? item.sizeX : item.sizeY;
        RectTransform.sizeDelta = new Vector2(w * gridUI.cellSize, h * gridUI.cellSize);

        if (itemIcon != null)
        {
            itemIcon.rectTransform.localRotation = item.rotated
                ? Quaternion.Euler(0f, 0f, 90f)
                : Quaternion.identity;
        }

        if (stackAmount != null)
        {
            stackAmount.text = item.amount > 1 ? item.amount.ToString() : string.Empty;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        origX       = item.posX;
        origY       = item.posY;
        origAmount  = item.amount;
        origRotated = item.rotated;

        gridUI.inventory.RemoveItem(item);
        isDragging = true;

        canvasRect = gridUI.rootCanvas.transform as RectTransform;
        RectTransform.SetParent(gridUI.rootCanvas.transform, true);
        RectTransform.SetAsLastSibling();

        Camera cam = gridUI.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : gridUI.rootCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, cam, out Vector2 pointerLocal);

        dragOffset = RectTransform.anchoredPosition - pointerLocal;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvasRect == null)
        {
            return;
        }

        Camera cam = gridUI.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : gridUI.rootCanvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, cam, out Vector2 pointerLocal))
        {
            return;
        }

        RectTransform.anchoredPosition = pointerLocal + dragOffset;

        Vector2Int cell = gridUI.ScreenToCell(eventData.position);
        item.posX = cell.x;
        item.posY = cell.y;

        UpdateDragPreview();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        gridUI.ClearHighlights();

        Vector2Int cell = gridUI.ScreenToCell(eventData.position);
        item.posX = cell.x;
        item.posY = cell.y;

        bool splitOnDrop = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && origAmount > 1;
        bool dropped = TryDrop(cell, splitOnDrop);

        if (!dropped)
        {
            item.posX    = origX;
            item.posY    = origY;
            item.amount  = origAmount;
            item.rotated = origRotated;
            gridUI.inventory.PlaceItem(item);
        }

        gridUI.RebuildItemUIs();
    }

    private void UpdateDragPreview()
    {
        bool canPlace = gridUI.inventory.CanPlace(item);
        bool canStack = gridUI.inventory.CanStackAt(item, item.posX, item.posY);
        bool splitOnDrop = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && origAmount > 1;

        GridInventoryUI.HighlightState state = GridInventoryUI.HighlightState.Invalid;
        if (canPlace || canStack)
        {
            state = (canStack || splitOnDrop)
                ? GridInventoryUI.HighlightState.Special
                : GridInventoryUI.HighlightState.Valid;
        }

        gridUI.UpdateHighlights(item, state);
    }

    private bool TryDrop(Vector2Int dropCell, bool splitOnDrop)
    {
        bool keepOriginHalf = splitOnDrop && (dropCell.x != origX || dropCell.y != origY || item.rotated != origRotated);

        int droppedAmount = origAmount;
        int remainingAtOrigin = 0;

        if (keepOriginHalf)
        {
            droppedAmount = Mathf.Max(1, origAmount / 2);
            remainingAtOrigin = origAmount - droppedAmount;
        }

        item.amount = droppedAmount;
        item.posX = dropCell.x;
        item.posY = dropCell.y;

        if (!gridUI.inventory.TryPlaceOrStackAt(item, dropCell.x, dropCell.y))
        {
            item.amount = origAmount;
            return false;
        }

        if (remainingAtOrigin > 0)
        {
            ItemData originHalf = new ItemData
            {
                itemID = item.itemID,
                sizeX = item.sizeX,
                sizeY = item.sizeY,
                amount = remainingAtOrigin,
                value = item.value,
                posX = origX,
                posY = origY,
                rotated = origRotated
            };

            if (!gridUI.inventory.PlaceItem(originHalf))
            {
                return false;
            }
        }

        return true;
    }
}