using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IItemContainerUI
{
    [SerializeField] private RectTransform slotRoot;
    [SerializeField] private RectTransform itemRoot;
    [SerializeField] private GameObject itemUIPrefab;
    [SerializeField] private Image previewImage;
    
    [SerializeField] private Image backgroundImage;
    
    [SerializeField] private bool overrideRotation = false;
    [SerializeField] private bool rotationOverrideValue = false;
    [SerializeField] private bool updateConstantly = false;

    [Header("Preview Colors")]
    [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color specialColor = new Color(1f, 0.9f, 0f, 0.35f);

    
    protected InventorySlot slot = new InventorySlot();
    private ItemUI currentItemUI;

    public RectTransform ContainerRect => slotRoot != null ? slotRoot : transform as RectTransform;

    protected virtual void Awake()
    {
        if (slotRoot == null)
        {
            slotRoot = transform as RectTransform;
        }

        if (itemRoot == null)
        {
            itemRoot = slotRoot;
        }
    }

    protected virtual void OnEnable()
    {
        RefreshView();
    }

    protected virtual void Update()
    {
        if (updateConstantly && overrideRotation && !slot.IsEmpty())
        {
            ApplyRotationOverride(slot.myItem);
        }
    }
    
    public void BindSlot(InventorySlot slot)
    {
        this.slot = slot;
        ApplyRotationOverride(this.slot.myItem);
        RefreshView();
    }

    public bool ContainsScreenPoint(Vector2 screenPosition)
    {
        if (slotRoot == null)
        {
            return false;
        }

        Canvas canvas = GetComponentInParent<Canvas>(true);
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        return RectTransformUtility.RectangleContainsScreenPoint(slotRoot, screenPosition, cam);
    }

    public virtual bool TryRemoveItem(ItemData item)
    {
        if (slot.myItem != item)
        {
            return false;
        }

        slot.myItem = null;
        slot.OnChanged?.Invoke();
        
        return true;
    }

    public virtual bool CanAcceptItem(ItemData item, Vector2 screenPosition)
    {
        return ContainsScreenPoint(screenPosition) && item != null && item.amount > 0 && (slot.canInsert(item) || CanLoadAmmoIntoWeapon(item) || CanSwapItem(item));
    }

    public virtual void UpdateDropPreview(ItemData item, Vector2 screenPosition, bool valid)
    {
        if (previewImage == null)
        {
            return;
        }

        if (!valid)
        {
            previewImage.color = invalidColor;
            return;
        }
        
        

        previewImage.color = CanLoadAmmoIntoWeapon(item) ? specialColor : validColor;
        
        
    }

    public virtual void ClearDropPreview()
    {
        if (previewImage == null)
        {
            return;
        }

        previewImage.color = defaultColor;
    }

    public virtual bool TryPlaceItem(ItemData item, Vector2 screenPosition)
    {
        if (!ContainsScreenPoint(screenPosition) || item == null || item.amount <= 0)
        {
            return false;
        }

        if (CanLoadAmmoIntoWeapon(item))
        {
            GunItemComponent gunComponent = slot.myItem.GetComponent<GunItemComponent>();
            return gunComponent != null && gunComponent.TryInsertAmmo(item);
        }

        if (!slot.canInsert(item))
        {
            return false;
        }

        if (slot.Insert(item))
        {
            ApplyRotationOverride(item);
            return true;
        }

        return false;
    }

    public virtual bool TrySwapItem(ItemData item, IItemContainerUI sourceContainer)
    {
        if (item == null || item.amount <= 0 || slot.IsEmpty() || CanLoadAmmoIntoWeapon(item) || !CanSwapItem(item))
        {
            return false;
        }

        ItemData displacedItem = slot.myItem;
        slot.myItem = null;
        slot.OnChanged?.Invoke();

        if (slot.Insert(item))
        {
            if (sourceContainer != null)
            {
                if (sourceContainer.TryRestoreItem(displacedItem))
                {
                    ApplyRotationOverride(item);
                    return true;
                }

                if (sourceContainer is GridInventoryUI gridInventoryUI && gridInventoryUI.TryPlaceItemAnywhere(displacedItem))
                {
                    ApplyRotationOverride(item);
                    return true;
                }
            }

            slot.myItem = null;
            slot.OnChanged?.Invoke();
        }

        slot.Insert(displacedItem);
        return false;
    }

    public virtual bool TryRestoreItem(ItemData item)
    {
        if (item == null || item.amount <= 0 || !slot.IsEmpty() || !slot.canInsert(item))
        {
            return false;
        }

        if (slot.Insert(item))
        {
            ApplyRotationOverride(item);
            return true;
        }

        return false;
    }

    public virtual void RefreshView()
    {
        if (currentItemUI != null)
        {
            Destroy(currentItemUI.gameObject);
            currentItemUI = null;
        }

        bool slotEmpty = slot.IsEmpty();

        if (slotEmpty || itemUIPrefab == null || itemRoot == null || slot.myItem == null || slot.myItem.itemID == 0) 
        {
            ClearDropPreview();
            return;
        }

        GameObject go = Instantiate(itemUIPrefab, itemRoot);
        currentItemUI = go.GetComponent<ItemUI>();

        if (currentItemUI != null)
        {
            // ItemUI.Init -> UpdateVisuals owns all of this ItemUI's RectTransform
            // sizing/anchoring (via ContainerRect) — don't set it here too, or the
            // two will fight and desync the moment UpdateVisuals runs again later
            // from an ItemData.OnChanged event (e.g. firing a weapon) without a
            // matching RefreshView call.
            currentItemUI.Init(slot.myItem, this, null);
        }

       
        ClearDropPreview();
        
        if( backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive( slot.IsEmpty());
        }

    }

    private bool CanLoadAmmoIntoWeapon(ItemData item)
    {
        if (item == null || item.amount <= 0 || slot.IsEmpty() || !slot.myItem.HasComponent<GunItemComponent>())
        {
            return false;
        }

        GunItemComponent gunComponent = slot.myItem.GetComponent<GunItemComponent>();
        return gunComponent != null && gunComponent.CanAcceptAmmo(item);
    }

    protected virtual bool CanSwapItem(ItemData item)
    {
        return item != null && item.amount > 0 && !slot.IsEmpty() && slot.CanInsertIfEmpty(item) && !CanLoadAmmoIntoWeapon(item);
    }

    private void ApplyRotationOverride(ItemData item)
    {
        if (!overrideRotation || item == null)
        {
            return;
        }

        if (item.rotated != rotationOverrideValue)
        {
            item.rotated = rotationOverrideValue;
        }
    }
}