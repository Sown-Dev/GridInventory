using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IItemContainerUI
{
    [SerializeField] private RectTransform slotRoot;
    [SerializeField] private RectTransform itemRoot;
    [SerializeField] private GameObject itemUIPrefab;
    [SerializeField] private Image previewImage;
    
    [SerializeField] private Image backgroundImage;
    
    [SerializeField] private bool flipOnInsert = false;

    [Header("Preview Colors")]
    [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color specialColor = new Color(1f, 0.9f, 0f, 0.35f);

    protected InventorySlot slot = new InventorySlot();
    private ItemUI currentItemUI;

    public RectTransform ContainerRect => slotRoot != null ? slotRoot : transform as RectTransform;


    private RectTransform myRt;
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
        myRt=GetComponent<RectTransform>();
    }

    protected virtual void OnEnable()
    {
        RefreshView();
    }
    
    public void BindSlot(InventorySlot slot)
    {
        this.slot = slot;
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
        return ContainsScreenPoint(screenPosition) && (slot.canInsert(item) || CanLoadAmmoIntoWeapon(item));
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
        item.rotated = flipOnInsert || item.rotated;
        if (!ContainsScreenPoint(screenPosition))
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

        return slot.Insert(item);
    }

    public virtual bool TryRestoreItem(ItemData item)
    {
        if (slot.myItem != null || !slot.canInsert(item))
        {
            return false;
        }

        return slot.Insert(item);
    }

    public virtual void RefreshView()
    {
        if (currentItemUI != null)
        {
            Destroy(currentItemUI.gameObject);
            currentItemUI = null;
        }

        if (slot.myItem == null || itemUIPrefab == null || itemRoot == null)
        {
            ClearDropPreview();
            return;
        }

        GameObject go = Instantiate(itemUIPrefab, itemRoot);
        currentItemUI = go.GetComponent<ItemUI>();

        if (currentItemUI != null)
        {
            currentItemUI.Init(slot.myItem, this, null);
            RectTransform rt = currentItemUI.RectTransform;
            rt.sizeDelta = myRt.sizeDelta;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

       
        ClearDropPreview();
        
        if( backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive( slot.IsEmpty());
        }

    }

    private bool CanLoadAmmoIntoWeapon(ItemData item)
    {
        if (item == null || slot.myItem == null || !slot.myItem.HasComponent<GunItemComponent>())
        {
            return false;
        }

        GunItemComponent gunComponent = slot.myItem.GetComponent<GunItemComponent>();
        return gunComponent != null && gunComponent.CanAcceptAmmo(item);
    }
}

