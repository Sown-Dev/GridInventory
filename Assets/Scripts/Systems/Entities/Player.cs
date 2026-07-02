using System;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponFireMode
{
    Semi,
    Auto
}

public class Player : StatsUnit
{
    public static Player instance;

    public Collider2D playerCollider;
    public Inventory Inventory;

    int accessorySlotCount = 2;

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

        if (gunRoot != null)
        {
            gunRestLocalPosition = gunRoot.transform.localPosition;
        }

        // Seed aim direction immediately so origin/direction are valid before the first Update.
        UpdateAimDirection();
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

    public GameObject projectilePrefab;
    public Transform gunMuzzle;
    public float projectileSpeed = 20f;
    private float nextFireTime = 0f;

    public SpriteRenderer gunSpriteRenderer;
    public GameObject gunRoot;

    // --- Recoil / spread runtime state ---
    private Vector3 gunRestLocalPosition;
    private Vector2 lastAimDirection = Vector2.right;   // raw, unrotated: origin -> mouse. Updated every frame.
    private float currentSpread = 0f;
    private float currentRecoilKickback = 0f;
    private float currentRecoilRotation = 0f;
    private float currentSignedRecoilRotation = 0f;

    private const float kickbackDistancePerStrength = 0.08f;
    private const float rotationKickPerStrength = 4f;
    private const float spreadBuildPerStrength = 0.35f;
    private const float baseRecoverySpeed = 12f;
    private const float maxRecoilAngleDegrees = 35f;

    // --- Crosshair-facing read access ---
    public Vector2 AimOrigin => gunMuzzle != null ? (Vector2)gunMuzzle.position
        : (gunRoot != null ? (Vector2)gunRoot.transform.position : (Vector2)transform.position);
    public float RecoilKickback => currentRecoilKickback;
    public float RecoilAngle => currentSignedRecoilRotation;

    // Direction actually used for both the visual gun/crosshair AND the fired bullet.
    public Vector2 RecoiledAimDirection
    {
        get
        {
            float rad = currentSignedRecoilRotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(
                lastAimDirection.x * cos - lastAimDirection.y * sin,
                lastAimDirection.x * sin + lastAimDirection.y * cos
            );
        }
    }

    public bool EquipSlot(int slotIndex)
    {
        bool success;

        switch (slotIndex)
        {
            case 1: EquippedSlot = WeaponSlot1; success = true; break;
            case 2: EquippedSlot = WeaponSlot2; success = true; break;
            case 3: EquippedSlot = null; success = true; break;
            default: success = false; break;
        }

        if (success)
        {
            ResetWeaponFeel();
        }

        return success;
    }

    private void ResetWeaponFeel()
    {
        currentSpread = 0f;
        currentRecoilKickback = 0f;
        currentRecoilRotation = 0f;
        currentSignedRecoilRotation = 0f;
    }

    // Runs every frame regardless of weapon state. This is the fix: aim tracking must not
    // depend on having a valid equipped weapon, or it freezes at its default value when unarmed.
    private void UpdateAimDirection()
    {
        if (Camera.main == null)
        {
            return;
        }

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 origin = AimOrigin;
        Vector2 toMouse = (Vector2)mouseWorldPosition - origin;

        if (toMouse.sqrMagnitude > 0.0001f)
        {
            lastAimDirection = toMouse.normalized;
        }
    }

    protected void HandleEquipped()
    {
        if (EquippedSlot is WeaponSlot weaponSlot)
        {
            if (weaponSlot.IsEmpty())
            {
                if (gunRoot != null) gunRoot.SetActive(false);
                return;
            }

            GunItemComponent gunComponent = weaponSlot.myItem.GetComponent<GunItemComponent>();
            if (gunComponent == null)
            {
                if (gunRoot != null) gunRoot.SetActive(false);
                return;
            }

            var weaponDef = gunComponent.GetDefinition<WeaponComponentDefinition>();
            if (weaponDef == null)
            {
                if (gunRoot != null) gunRoot.SetActive(false);
                return;
            }

            if (gunRoot != null) gunRoot.SetActive(true);
            if (gunSpriteRenderer != null) gunSpriteRenderer.sprite = weaponSlot.myItem.GetIcon();

            UpdateGunVisualAndRecoil(weaponDef);

            bool triggerPulled = weaponDef.fireMode == WeaponFireMode.Auto
                ? Input.GetButton("Fire1")
                : Input.GetButtonDown("Fire1");

            if (triggerPulled && Time.time >= nextFireTime)
            {
                TryFireWeapon(gunComponent, weaponDef);
            }
        }
        else
        {
            if (gunRoot != null) gunRoot.SetActive(false);
        }
    }

    // Only handles the visual gun sprite + recoil decay now. Does NOT touch lastAimDirection.
    private void UpdateGunVisualAndRecoil(WeaponComponentDefinition weaponDef)
    {
        if (gunRoot == null)
        {
            return;
        }

        float rawAimAngle = Mathf.Atan2(lastAimDirection.y, lastAimDirection.x) * Mathf.Rad2Deg;

        bool facingLeft = Mathf.Abs(rawAimAngle) > 90f;
        if (gunSpriteRenderer != null)
        {
            gunSpriteRenderer.flipY = facingLeft;
        }

        float recoilSign = facingLeft ? -1f : 1f;
        currentSignedRecoilRotation = currentRecoilRotation * recoilSign;

        Vector2 recoiledDir = RecoiledAimDirection;
        float appliedAngle = Mathf.Atan2(recoiledDir.y, recoiledDir.x) * Mathf.Rad2Deg;
        gunRoot.transform.rotation = Quaternion.Euler(0f, 0f, appliedAngle);

        Vector3 recoilOffset = -(Vector3)recoiledDir * currentRecoilKickback;
        gunRoot.transform.localPosition = gunRestLocalPosition + recoilOffset;

        float recoverySpeed = baseRecoverySpeed * weaponDef.recoilRecovery;
        currentRecoilKickback = Mathf.MoveTowards(currentRecoilKickback, 0f, recoverySpeed * Time.deltaTime * kickbackDistancePerStrength);
        currentRecoilRotation = Mathf.MoveTowards(currentRecoilRotation, 0f, recoverySpeed * Time.deltaTime);
        currentSpread = Mathf.MoveTowards(currentSpread, 0f, recoverySpeed * Time.deltaTime);
    }

    private void TryFireWeapon(GunItemComponent gunComponent, WeaponComponentDefinition weaponDef)
    {
        if (!gunComponent.UseAmmo(true))
        {
            Debug.Log("Out of ammo!");
            return;
        }

        gunComponent.UseAmmo(false);
        EquippedSlot.OnChanged.Invoke();

        float spreadOffset = UnityEngine.Random.Range(-currentSpread, currentSpread);
        Vector2 shotDirection = (Quaternion.Euler(0f, 0f, spreadOffset) * RecoiledAimDirection).normalized;
        Quaternion shotRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(shotDirection.y, shotDirection.x) * Mathf.Rad2Deg);

        Shoot(shotDirection, weaponDef.baseDamage, shotRotation);

        // Horizontal kickback (distance, away from player along the aim line) and
        // vertical kick (angle, the climb) now scale off separate coefficients.
        currentRecoilKickback = Mathf.Clamp(currentRecoilKickback + kickbackDistancePerStrength * weaponDef.recoilStrengthHorizontal, 0f, kickbackDistancePerStrength * weaponDef.recoilStrengthHorizontal * 4f);
        currentRecoilRotation = Mathf.Clamp(currentRecoilRotation + rotationKickPerStrength * weaponDef.recoilStrengthVertical, 0f, maxRecoilAngleDegrees);
        currentSpread = Mathf.Clamp(currentSpread + spreadBuildPerStrength * weaponDef.recoilStrengthVertical, 0f, weaponDef.spreadDegrees);

        float fireDelay = weaponDef.baseFireRate > 0f ? 60f / weaponDef.baseFireRate : 0.1f;
        nextFireTime = Time.time + fireDelay;
    }

    private void Shoot(Vector2 normalizedDirection, float damage, Quaternion rotation)
    {
        if (projectilePrefab != null && gunMuzzle != null)
        {
            GameObject projObj = Instantiate(projectilePrefab, gunMuzzle.position, rotation);
            Projectile projectile = projObj.GetComponent<Projectile>();

            if (projectile != null)
            {
                Vector2 velocity = normalizedDirection * projectileSpeed;
                projectile.Init(velocity, damage, rotation, playerCollider, faction);
            }
        }
        else
        {
            Debug.LogWarning("Missing projectile prefab or gun muzzle reference.");
        }
    }
    #endregion

    public override void CalculateStats()
    {
        base.CalculateStats();

        finalStats.Combine(HelmetSlot.GetDefinition()?.stats);
        finalStats.Combine(ChestSlot.GetDefinition()?.stats);
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
            if (definition == null) continue;
            widestItem = Mathf.Max(widestItem, definition.sizeX);
        }

        widthLimit = Mathf.Max(widthLimit, widestItem);

        int requiredHeight = 16;
        int previewX = 0;
        int previewRowHeight = 0;

        foreach (ItemDefinition definition in definitions)
        {
            if (definition == null) continue;

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
        if (Inventory == null) return;

        List<ItemDefinition> definitions = Registry.instance != null
            ? Registry.instance.GetAllDefinitions()
            : new List<ItemDefinition>();

        int cursorX = 0;
        int cursorY = 0;
        int rowHeight = 0;

        foreach (ItemDefinition definition in definitions)
        {
            if (definition == null) continue;

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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipSlot(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            EquipSlot(3);
        }

        // Always track aim direction, whether or not a weapon is equipped.
        UpdateAimDirection();

        HandleEquipped();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

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