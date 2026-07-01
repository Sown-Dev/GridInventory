
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : StatsUnit
{
    public static Player instance;
    
    
    public Inventory Inventory;
    
    int accessorySlotCount = 2;

   /* public EquipmentSlot[] EquipmentSlots =
    {
        new EquipmentSlot(EquipmentType.Helmet),
        new EquipmentSlot(EquipmentType.Chest),
        new EquipmentSlot(EquipmentType.Backpack),
        new EquipmentSlot(EquipmentType.Accessory)
    };*/
    public EquipmentSlot HelmetSlot = new EquipmentSlot(EquipmentType.Helmet);
    public EquipmentSlot ChestSlot = new EquipmentSlot(EquipmentType.Chest);

    public WeaponSlot WeaponSlot1 = new WeaponSlot();
    
    public WeaponSlot WeaponSlot2 = new WeaponSlot();

    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private float moveInput;
    private bool jumpRequested;



   void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of Player detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    public override void Start()
    {
        base.Start();
    }

    
    
    public void OnEnable()
    {
        HelmetSlot.OnChanged += OnEquipmentChanged;
        ChestSlot.OnChanged += OnEquipmentChanged;
        WeaponSlot1.OnChanged += OnEquipmentChanged;
        WeaponSlot2.OnChanged += OnEquipmentChanged;
        InitializeInventoryForTesting();
    }
    
    public void OnDisable()
    {
        HelmetSlot.OnChanged -= OnEquipmentChanged;
        ChestSlot.OnChanged -= OnEquipmentChanged;
        WeaponSlot1.OnChanged -= OnEquipmentChanged;
        WeaponSlot2.OnChanged -= OnEquipmentChanged;
    }
    
    public virtual void OnEquipmentChanged()
    {
        Debug.Log("Called OnEquipmentChanged");
        CalculateStats();
    }
    
    #region WeaponEquiping
    
    public InventorySlot EquippedSlot;
    public bool EquipSlot(int slotIndex)
    {
        if( slotIndex == 1)
        {
            EquippedSlot = WeaponSlot1;
            return true;
        }
        else if (slotIndex == 2)
        {
            EquippedSlot = WeaponSlot2;
            return true;
        }
        else if (slotIndex == 3)
        {
            EquippedSlot = null;
            return true;
        }
        
        else
            return false;
    }
    
    public SpriteRenderer gunSpriteRenderer; // Assign this in the inspector
    public GameObject gunRoot;

    protected void HandleEquipped()
    {
        if(EquippedSlot is WeaponSlot weaponSlot)
        {
            if (WeaponSlot1.IsEmpty())
            {
                return;
            }
        
            // 1) draw weapon model
            gunSpriteRenderer.sprite = WeaponSlot1.myItem.GetIcon();
            gunRoot.SetActive(true);
        
            // set rotation:
            // Get the mouse position in world space
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
            // Calculate the direction vector from the gun to the mouse
            Vector3 direction = mouseWorldPosition - gunRoot.transform.position;
        
            // Calculate the rotation angle in degrees
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
            // Apply the rotation on the Z axis
            gunRoot.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        
        
            if( Input.GetButtonDown("Fire1"))
            {
                Debug.Log("Firing weapon!");
            }
            
        }
        else
        {
            gunRoot.SetActive(false);
            Debug.Log("No weapon equipped.");
        }
    }

    #endregion
    
    public override void CalculateStats()
    {
        base.CalculateStats();
        
        finalStats.Combine(  HelmetSlot.GetDefinition()? .stats);
        finalStats.Combine(  ChestSlot.GetDefinition()? .stats);
        ApplyStats();
    }

    private void InitializeInventoryForTesting()
    {
        InitializeInventorySetup();
        PopulateInventoryWithTestItems();
    }

    private void InitializeInventorySetup()
    {
        Inventory ??= new Inventory();

        List<ItemDefinition> definitions = Registry.instance != null
            ? Registry.instance.GetAllDefinitions()
            : new List<ItemDefinition>();

        int widthLimit = 9;
        int widestItem = 1;

        foreach (ItemDefinition definition in definitions)
        {
            if (definition == null)
            {
                continue;
            }

            widestItem = Mathf.Max(widestItem, definition.sizeX);
        }

        widthLimit = Mathf.Max(widthLimit, widestItem);

        int requiredHeight = 16;
        int previewX = 0;
        int previewRowHeight = 0;

        foreach (ItemDefinition definition in definitions)
        {
            if (definition == null)
            {
                continue;
            }

            int itemWidth = definition.sizeX;
            int itemHeight = definition.sizeY;

            if (previewX > 0 && previewX + itemWidth > widthLimit)
            {
                requiredHeight += previewRowHeight;
                previewX = 0;
                previewRowHeight = 0;
            }

            previewRowHeight = Mathf.Max(previewRowHeight, itemHeight);
            previewX += itemWidth;
        }

        requiredHeight += previewRowHeight;

        Inventory.sizeX = widthLimit;
        Inventory.sizeY = requiredHeight;
    }

    private void PopulateInventoryWithTestItems()
    {
        if (Inventory == null)
        {
            return;
        }

        List<ItemDefinition> definitions = Registry.instance != null
            ? Registry.instance.GetAllDefinitions()
            : new List<ItemDefinition>();

        int cursorX = 0;
        int cursorY = 0;
        int rowHeight = 0;

        foreach (ItemDefinition definition in definitions)
        {
            if (definition == null)
            {
                continue;
            }

            int itemWidth = definition.sizeX;
            int itemHeight = definition.sizeY;

            for (int copy = 0; copy < 2; copy++)
            {
                if (cursorX > 0 && cursorX + itemWidth > Inventory.sizeX)
                {
                    cursorX = 0;
                    cursorY += rowHeight;
                    rowHeight = 0;
                }

                rowHeight = Mathf.Max(rowHeight, itemHeight);

                ItemData item = definition.GenerateData();
                item.amount = Mathf.Max(1, definition.maxAmount);
                item.posX = cursorX;
                item.posY = cursorY;

                if (!Inventory.TryPlaceWithStacking(item))
                {
                    Debug.LogWarning($"Could not place test item {definition.name} in player inventory.");
                }

                cursorX += itemWidth;
            }
        }
    }

    public void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true;
        }
        
        if( Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipSlot(2);
        }
        
        HandleEquipped();
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (jumpRequested && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        jumpRequested = false;
    }

    private bool IsGrounded()
    {
        Vector2 checkPosition = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position;
        return Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer) != null;
    }
}
