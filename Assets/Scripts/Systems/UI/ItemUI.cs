using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
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
    public Image durabilityBar;

    private Material mat;

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (itemIcon != null && itemIcon.material != null)
        {
            mat = new Material(itemIcon.material);
            itemIcon.material = mat;
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
            img.color   = Color.white;
            img.SetNativeSize();

            if (mat != null)
            {
                Vector2 spriteSize = def.icon.textureRect.size;
                mat.SetVector("_SpriteDimensions", new Vector4(spriteSize.x, spriteSize.y, 0, 0));
            }
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
        
        if(item.HasComponent<GunItemComponent>())
        {
            weaponText.text = item.GetComponent<GunItemComponent>().AmmoCount() + "/" +item.GetComponent<GunItemComponent>().MagSize();
        }
        else
        {
            weaponText.text = "";
        }

        if(item.HasComponent<DurabilityItemComponent>())
        {
            durabilityBar.enabled  = true; 
            durabilityBar.fillAmount = (float)item.GetComponent<DurabilityItemComponent>().durability / (float)item.GetComponent<DurabilityItemComponent>().maxDurability;
            durabilityBar.color = Color.Lerp(Color.red, new Color( 0.5f, 0.8f,0.5f), durabilityBar.fillAmount);
        }
        else
        {
            durabilityBar.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;

        if (mat != null)
        {
            mat.SetFloat("_OutlineThickness", 1);
            mat.SetColor("_OutlineColor", Color.white);
        }

        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (mat != null)
        {
            mat.SetFloat("_OutlineThickness", 0);
            mat.SetColor("_OutlineColor", Color.clear);
        }

        HideTooltip();
    }

    private void ShowTooltip()
    {
        TooltipManager.Instance.ShowItem(item, RectTransform.position, gameObject, useOffset:true, useWorldSpace: false);
    }

    private void HideTooltip()
    {
        TooltipManager.Instance.Hide();
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

        HideTooltip();
        if (mat != null)
        {
            mat.SetFloat("_OutlineThickness", 0);
            mat.SetColor("_OutlineColor", Color.clear);
        }

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

        // 1. Calculate Split Logic
        bool splitOnDrop = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && origAmount > 1;
        int droppedAmount = origAmount;
        int remainingAtOrigin = 0;

        if (splitOnDrop)
        {
            droppedAmount = Mathf.Max(1, origAmount / 2);
            remainingAtOrigin = origAmount - droppedAmount;
            item.amount = droppedAmount;
        }

        // 2. Attempt Placement
        bool dropped = currentDropTarget != null && currentDropTarget.TryPlaceItem(item, eventData.position);

        if (dropped)
        {
            // 3. Handle Leftovers
            if (splitOnDrop && remainingAtOrigin > 0 && sourceContainer != null)
            {
                // Create a duplicate item for the remaining half.
                // Note: If ItemData requires deep copying (due to components), 
                // replace this block with an item.Clone() method if you have one.
                ItemData originHalf = new ItemData
                {
                    itemID = item.itemID,
                    sizeX = item.sizeX,
                    sizeY = item.sizeY,
                    amount = remainingAtOrigin,
                    posX = origX,
                    posY = origY,
                    rotated = origRotated
                };

                sourceContainer.TryRestoreItem(originHalf);
            }
        }
        else if (sourceContainer != null)
        {
            // 4. Handle Failed Placement
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

        if (EventSystem.current.IsPointerOverGameObject() && RectTransformUtility.RectangleContainsScreenPoint(RectTransform, Input.mousePosition))
        {
            if (mat != null)
            {
                mat.SetFloat("_OutlineThickness", 1);
                mat.SetColor("_OutlineColor", Color.white);
            }
            ShowTooltip();
        }
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

        // Temporarily adjust item amount so the container preview accurately evaluates weight/limits
        bool splitOnDrop = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && origAmount > 1;
        if (splitOnDrop)
        {
            item.amount = Mathf.Max(1, origAmount / 2);
        }

        bool valid = currentDropTarget.CanAcceptItem(item, Input.mousePosition);
        currentDropTarget.UpdateDropPreview(item, Input.mousePosition, valid);

        if (splitOnDrop)
        {
            item.amount = origAmount;
        }
    }
}