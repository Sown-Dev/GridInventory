
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{
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

    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private float moveInput;
    private bool jumpRequested;

    public override void Start()
    {
        base.Start();
        InitializeInventoryForTesting();
    }

    private void InitializeInventoryForTesting()
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
                if (cursorX > 0 && cursorX + itemWidth > widthLimit)
                {
                    cursorX = 0;
                    cursorY += rowHeight;
                    rowHeight = 0;
                }

                rowHeight = Mathf.Max(rowHeight, itemHeight);

                ItemData item = definition.GenerateData();
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
