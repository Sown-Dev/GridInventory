using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform RectTransform { get; private set; }

    private ItemData item;
    private IItemContainerUI sourceContainer;
    private IItemContainerUI currentDropTarget;
    private GridInventoryUI gridUI;
    private RectTransform canvasRect;
    private Vector3 dragWorldOffset;
    private CanvasGroup canvasGroup;
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
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Init(ItemData item, IItemContainerUI container, GridInventoryUI gridUI = null)
    {
        this.item   = item;
        this.sourceContainer = container;
        this.gridUI = gridUI ?? container as GridInventoryUI;

        ItemDefinition def = Registry.instance != null
            ? Registry.instance.ByID(item.itemID)
            : null;

        if (def != null && def.icon != null && itemIcon != null)
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
        if (gridUI != null)
        {
            int w = item.rotated ? item.sizeY : item.sizeX;
            int h = item.rotated ? item.sizeX : item.sizeY;
            RectTransform.sizeDelta = new Vector2(w * gridUI.cellSize, h * gridUI.cellSize);
        }
        else if (itemIcon != null)
        {
            RectTransform.sizeDelta = itemIcon.rectTransform.sizeDelta;
        }

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

        sourceContainer = sourceContainer ?? ItemContainerUIUtility.ResolveContainerInParents(transform.parent);
        sourceContainer?.TryRemoveItem(item);
        isDragging = true;
        currentDropTarget = null;

        canvasGroup.blocksRaycasts = false;

        Canvas rootCanvas = GetComponentInParent<Canvas>(true);
        canvasRect = rootCanvas != null ? rootCanvas.transform as RectTransform : null;

        Camera cam = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;

        if (canvasRect != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect, eventData.position, cam, out Vector3 pointerWorld))
        {
            dragWorldOffset = RectTransform.position - pointerWorld;
        }

        if (rootCanvas != null)
        {
            RectTransform.SetParent(rootCanvas.transform, true);
        }

        RectTransform.SetAsLastSibling();

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvasRect == null)
        {
            return;
        }

        Canvas rootCanvas = canvasRect.GetComponent<Canvas>();
        Camera cam = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect, eventData.position, cam, out Vector3 worldPoint))
        {
            RectTransform.position = worldPoint + dragWorldOffset;
        }

        UpdateDragPreview();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        currentDropTarget?.ClearDropPreview();

        bool dropped = currentDropTarget != null && currentDropTarget.TryPlaceItem(item, eventData.position);

        if (!dropped && sourceContainer != null)
        {
            item.posX    = origX;
            item.posY    = origY;
            item.amount  = origAmount;
            item.rotated = origRotated;
            sourceContainer.TryRestoreItem(item);
        }

        if (sourceContainer != null)
        {
            sourceContainer.RefreshView();
        }

        if (currentDropTarget != null && currentDropTarget != sourceContainer)
        {
            currentDropTarget.RefreshView();
        }

        currentDropTarget = null;
        sourceContainer = null;
    }

    private void UpdateDragPreview()
    {
        if (sourceContainer == null)
        {
            currentDropTarget?.ClearDropPreview();
            currentDropTarget = null;
            return;
        }

        IItemContainerUI hoveredContainer = ItemContainerUIUtility.ResolveContainerAtScreenPoint(Input.mousePosition);

        if (hoveredContainer != currentDropTarget)
        {
            currentDropTarget?.ClearDropPreview();
            currentDropTarget = hoveredContainer;
        }

        if (currentDropTarget == null)
        {
            return;
        }

        bool valid = currentDropTarget.CanAcceptItem(item, Input.mousePosition);
        currentDropTarget.UpdateDropPreview(item, Input.mousePosition, valid);
    }
}