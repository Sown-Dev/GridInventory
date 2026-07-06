using System;
using System.Collections.Generic;
using System.IO;
using Unity.Properties;
using Unity.Serialization.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum WeaponFireMode
{
    Semi,
    Auto
}

[Serializable]
[GeneratePropertyBag]
public class PlayerSaveData
{
    public float posX;
    public float posY;
    public float posZ;

    public Inventory inventory;

    public ItemData helmetItem;
    public ItemData chestItem;
    public ItemData weaponSlot1Item;
    public ItemData weaponSlot2Item;

    // 0 = nothing equipped, 1 = WeaponSlot1, 2 = WeaponSlot2
    public int equippedSlotIndex;
}

public class Player : StatsUnit
{
    public static Player instance;

    #region Core References

    public Collider2D playerCollider;
    public Inventory Inventory;

    int accessorySlotCount = 2;

    public EquipmentSlot HelmetSlot = new EquipmentSlot(EquipmentType.Helmet);
    public EquipmentSlot ChestSlot = new EquipmentSlot(EquipmentType.Chest);

    public WeaponSlot WeaponSlot1 = new WeaponSlot();
    public WeaponSlot WeaponSlot2 = new WeaponSlot();

    #endregion

    #region Movement

    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private float moveInput;
    private bool jumpRequested;

    #endregion

    #region Unity Lifecycle

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
            return;
        }

        if (File.Exists(SaveFilePath))
        {
            LoadGame();
        }
        else
        {
            InitializeNewGame();
        }
    }

    public override void Start()
    {
        base.Start();

        // Interpolation smooths transform.position reads that happen on Update()'s clock
        // (crosshair, aim origin, gun visuals, etc.) against the physics step's fixed clock.
        // Without this, anything reading the player's position outside FixedUpdate sees
        // sub-pixel stutter — normally invisible, but a Pixel Perfect Camera quantizes that
        // stutter onto the pixel grid, where it shows up as visible jitter.
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (gunRoot != null)
        {
            gunRestLocalPosition = gunRoot.transform.localPosition;
        }

        UpdateAimDirection();
    }

    public void OnEnable()
    {
        HelmetSlot.OnChanged += OnEquipmentChanged;
        ChestSlot.OnChanged += OnEquipmentChanged;
        WeaponSlot1.OnChanged += OnEquipmentChanged;
        WeaponSlot2.OnChanged += OnEquipmentChanged;

        // Only seed test items on a genuinely fresh game — a loaded save already has its
        // real inventory populated in Awake, and this would otherwise stomp it.
        if (!loadedFromSave)
        {
            InitializeInventoryForTesting();
        }
    }

    public void OnDisable()
    {
        HelmetSlot.OnChanged -= OnEquipmentChanged;
        ChestSlot.OnChanged -= OnEquipmentChanged;
        WeaponSlot1.OnChanged -= OnEquipmentChanged;
        WeaponSlot2.OnChanged -= OnEquipmentChanged;
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

        if (Input.GetKeyDown(laserToggleKey))
        {
            laserSightEnabled = !laserSightEnabled;
        }

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

    #endregion

    #region Save / Load

    private const string SaveFileName = "playersave.json";
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private bool loadedFromSave = false;

    private void LoadGame()
    {
        string json;
        try
        {
            json = File.ReadAllText(SaveFilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to read save file, initializing new game instead. {e}");
            InitializeNewGame();
            return;
        }

        PlayerSaveData data;
        try
        {
            data = JsonSerialization.FromJson<PlayerSaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to parse save data, initializing new game instead. {e}");
            InitializeNewGame();
            return;
        }

        if (data == null)
        {
            Debug.LogWarning("Save data was null after parsing, initializing new game instead.");
            InitializeNewGame();
            return;
        }

        ApplyLoadedData(data);
        loadedFromSave = true;
    }

    private void ApplyLoadedData(PlayerSaveData data)
    {
        transform.position = new Vector3(data.posX, data.posY, data.posZ);

        Inventory = data.inventory ?? new Inventory();

        HelmetSlot.myItem = data.helmetItem;
        ChestSlot.myItem = data.chestItem;
        WeaponSlot1.myItem = data.weaponSlot1Item;
        WeaponSlot2.myItem = data.weaponSlot2Item;

        // Bypasses EquipSlot's "already equipped, no-op" guard since EquippedSlot starts null
        // here regardless of what was saved.
        if (data.equippedSlotIndex == 1 || data.equippedSlotIndex == 2)
        {
            EquipSlot(data.equippedSlotIndex);
        }

        // EquipSlot only resets weapon feel / ammo visuals, not stats — and HelmetSlot/ChestSlot
        // items were assigned directly above without going through Insert(), so OnChanged never
        // fired for them. Call this once, manually, to pick up both.
        OnEquipmentChanged();
    }

    private void InitializeNewGame()
    {
        // No new-game setup needed yet beyond leaving the player at its scene-placed position.
        // Future new-game defaults (starting inventory, stats, etc.) go here.
    }

    public void SaveGame()
    {
        int equippedIndex = 0;
        if (EquippedSlot == WeaponSlot1) equippedIndex = 1;
        else if (EquippedSlot == WeaponSlot2) equippedIndex = 2;

        PlayerSaveData data = new PlayerSaveData
        {
            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,
            inventory = Inventory,
            helmetItem = HelmetSlot.myItem,
            chestItem = ChestSlot.myItem,
            weaponSlot1Item = WeaponSlot1.myItem,
            weaponSlot2Item = WeaponSlot2.myItem,
            equippedSlotIndex = equippedIndex
        };

        string json = JsonSerialization.ToJson(data);

        try
        {
            File.WriteAllText(SaveFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write save file: {e}");
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

#if UNITY_EDITOR
    [ContextMenu("Save Game")]
    private void SaveGameFromEditor()
    {
        SaveGame();
        Debug.Log($"Player saved from editor context menu to {SaveFilePath}");
    }

    [ContextMenu("Delete Save File")]
    private void DeleteSaveFileFromEditor()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log("Save file deleted.");
        }
        else
        {
            Debug.Log("No save file to delete.");
        }
    }
#endif

    #endregion

    #region Equipment / Stats

    public virtual void OnEquipmentChanged()
    {
        Debug.Log("Called OnEquipmentChanged");
        CalculateStats();
        SetAmmoVisualizer();

    }

    public override void CalculateStats()
    {
        base.CalculateStats();

        finalStats.Combine(HelmetSlot.GetDefinition()?.stats);
        finalStats.Combine(ChestSlot.GetDefinition()?.stats);
        ApplyStats();
    }

    #endregion

    #region Weapon Equipping / Firing

    [Header("Laser Sight")]
    public LineRenderer laserLineRenderer;
    public LayerMask laserHitMask;
    public float laserMaxDistance = 50f;
    public bool laserSightEnabled = true;
    [SerializeField] private KeyCode laserToggleKey = KeyCode.L;
    
    
    
    public InventorySlot EquippedSlot;

    public GameObject projectilePrefab;
    public Transform gunMuzzle;
    public float projectileSpeed = 120f;
    private float nextFireTime = 0f;

    public SpriteRenderer gunSpriteRenderer;
    public GameObject gunRoot;

    public Transform aimOrigin;

    private Vector3 gunRestLocalPosition;
    private Vector2 lastAimDirection = Vector2.right;
    private float currentSpread = 0f;
    private float currentRecoilKickback = 0f;
    private float currentRecoilRotation = 0f;
    private float currentSignedRecoilRotation = 0f;

    private int consecutiveShotsFired = 0;
    private float lastShotTime = -999f;

    private const float maxRecoilAngleDegrees = 60f;
    private const float maxRecoilKickbackStacks = 4f;
    private const float baseRecoverySpeed = 8f;
    private const float kickbackRecoverySpeedScale = 0.5f;
    private const float sprayHoldWindow = 0.15f;

    private const float gunVisualKickbackScale = 0.3f;
    private const float gunVisualMaxOffsetDistance = 0.4f;
    private const float gunVisualRotationDampenScale = 1.5f;

    private const float aimDirectionDeadzoneRadius = 0.35f;

    public Vector2 AimOrigin => aimOrigin != null ? (Vector2)aimOrigin.position : (Vector2)transform.position;
    public float RecoilKickback => currentRecoilKickback;
    public float RecoilAngle => currentSignedRecoilRotation;
    public float CurrentSpreadDegrees => currentSpread;

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
            case 1:
                if (EquippedSlot == WeaponSlot1) return false;
                EquippedSlot = WeaponSlot1;
                success = true;
                break;
            case 2:
                if (EquippedSlot == WeaponSlot2) return false;
                EquippedSlot = WeaponSlot2;
                success = true;
                break;
            case 3:
                if (EquippedSlot == null) return false;
                EquippedSlot = null;
                success = true;
                break;
            default:
                success = false;
                break;
        }

        if (success)
        {
            ResetWeaponFeel(GetEquippedWeaponDef());
            SetAmmoVisualizer();
        }

        return success;
    }

    public void SetAmmoVisualizer()
    {
        ammoVisualizer.SetAmmo(EquippedSlot?.myItem?.GetComponent<GunItemComponent>()?.AmmoCount() ?? 0,
            EquippedSlot?.myItem?.GetComponent<GunItemComponent>()?.GetDefinition<WeaponComponentDefinition>()?.MagSize ?? 0);
    }

    private WeaponComponentDefinition GetEquippedWeaponDef()
    {
        if (EquippedSlot is WeaponSlot weaponSlot && !weaponSlot.IsEmpty())
        {
            GunItemComponent gunComponent = weaponSlot.myItem.GetComponent<GunItemComponent>();
            return gunComponent != null ? gunComponent.GetDefinition<WeaponComponentDefinition>() : null;
        }
        return null;
    }

    private void ResetWeaponFeel(WeaponComponentDefinition weaponDef)
    {
        currentSpread = weaponDef != null ? weaponDef.baseSpreadDegrees : 0f;
        currentRecoilKickback = 0f;
        currentRecoilRotation = 0f;
        currentSignedRecoilRotation = 0f;
        consecutiveShotsFired = 0;
        lastShotTime = -999f;
    }

    [Header("Reload")]
    [SerializeField] private KeyCode reloadKey = KeyCode.R;

    private bool isReloading = false;
    private float reloadEndTime = -1f;
    private GunItemComponent reloadingGunComponent;
    private WeaponComponentDefinition reloadingWeaponDef;
    private int reloadingAmmoItemID = -1;

    [Header("Reload UI")]
    [SerializeField] private GameObject reloadBarRoot;
    [SerializeField] private Image reloadBarFill;

    [SerializeField] private AmmoVisualizer ammoVisualizer;

    public bool IsReloading => isReloading;
    public float ReloadProgress01 => !isReloading || reloadingWeaponDef == null
        ? 0f
        : Mathf.Clamp01(1f - (reloadEndTime - Time.time) / reloadingWeaponDef.reloadDuration);

    private void UpdateReloadUI()
    {
        if (reloadBarRoot == null) return;

        if (isReloading)
        {
            if (!reloadBarRoot.activeSelf) reloadBarRoot.SetActive(true);
            if (reloadBarFill != null) reloadBarFill.fillAmount = ReloadProgress01;
        }
        else
        {
            if (reloadBarRoot.activeSelf) reloadBarRoot.SetActive(false);
        }
    }

    private void TryStartReload(GunItemComponent gunComponent, WeaponComponentDefinition weaponDef)
    {
        if (gunComponent.IsAmmoFull()) return;

        if (!gunComponent.TryGetReloadAmmoID(Inventory, weaponDef, out int ammoItemID))
        {
            Debug.Log("No compatible ammo to reload.");
            return;
        }

        isReloading = true;
        reloadEndTime = Time.time + weaponDef.reloadDuration;
        reloadingGunComponent = gunComponent;
        reloadingWeaponDef = weaponDef;
        reloadingAmmoItemID = ammoItemID;
    }

    private void CompleteReload()
    {
        isReloading = false;

        if (reloadingGunComponent != null && Inventory != null)
            reloadingGunComponent.FinishReload(Inventory, reloadingAmmoItemID);

        reloadingGunComponent = null;
        reloadingWeaponDef = null;
        reloadingAmmoItemID = -1;
        SetAmmoVisualizer();
    }

    private void CancelReload()
    {
        isReloading = false;
        reloadEndTime = -1f;
        reloadingGunComponent = null;
        reloadingWeaponDef = null;
        reloadingAmmoItemID = -1;
    }

    private void UpdateAimDirection()
    {
        if (Camera.main == null)
        {
            return;
        }

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 origin = AimOrigin;
        Vector2 toMouse = (Vector2)mouseWorldPosition - origin;

        if (toMouse.sqrMagnitude > aimDirectionDeadzoneRadius * aimDirectionDeadzoneRadius)
        {
            lastAimDirection = toMouse.normalized;
        }
    }

    protected void HandleEquipped()
    {
        if (isReloading)
        {
            GunItemComponent currentGun = null;
            if (EquippedSlot is WeaponSlot activeSlot && !activeSlot.IsEmpty())
                currentGun = activeSlot.myItem.GetComponent<GunItemComponent>();

            if (currentGun == null || currentGun != reloadingGunComponent)
                CancelReload();
        }

        UpdateReloadUI();

        if (EquippedSlot is WeaponSlot weaponSlot)
        {
            if (weaponSlot.IsEmpty()) { DisableWeaponVisuals(); return; }

            GunItemComponent gunComponent = weaponSlot.myItem.GetComponent<GunItemComponent>();
            if (gunComponent == null) { DisableWeaponVisuals(); return; }

            var weaponDef = gunComponent.GetDefinition<WeaponComponentDefinition>();
            if (weaponDef == null) { DisableWeaponVisuals(); return; }

            if (gunRoot != null) gunRoot.SetActive(true);
            if (gunSpriteRenderer != null) gunSpriteRenderer.sprite = weaponSlot.myItem.GetIcon();
            if (laserLineRenderer != null) laserLineRenderer.enabled = laserSightEnabled;

            UpdateGunVisualAndRecoil(weaponDef);
            UpdateSpreadBloom(weaponDef);
            UpdateLaserSight();

            if (Input.GetKeyDown(reloadKey) && !isReloading)
            {
                TryStartReload(gunComponent, weaponDef);
            }

            if (isReloading)
            {
                if (Time.time >= reloadEndTime) CompleteReload();
                else return;
            }

            bool triggerPulled = weaponDef.fireMode == WeaponFireMode.Auto
                ? Input.GetButton("Fire1")
                : Input.GetButtonDown("Fire1");

            bool pointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            if (triggerPulled && !pointerOverUI && Time.time >= nextFireTime)
                TryFireWeapon(gunComponent, weaponDef);
        }
        else
        {
            DisableWeaponVisuals();
        }
    }

    private void DisableWeaponVisuals()
    {
        if (gunRoot != null) gunRoot.SetActive(false);
        if (laserLineRenderer != null) laserLineRenderer.enabled = false;
    }

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
            float gunScale = 0.33f;
            gunSpriteRenderer.transform.localScale = new Vector3(-gunScale, facingLeft? -gunScale: gunScale, gunScale);
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.transform.localScale = new Vector3(facingLeft ? -1f : 1f, 1f, 1f);
        }
        
        float recoilSign = facingLeft ? -1f : 1f;
        currentSignedRecoilRotation = currentRecoilRotation * recoilSign;

        Vector2 recoiledDir = RecoiledAimDirection;
        float dampenedVisualAngle = rawAimAngle + currentSignedRecoilRotation * gunVisualRotationDampenScale;
        gunRoot.transform.rotation = Quaternion.Euler(0f, 0f, dampenedVisualAngle);

        Vector3 worldRecoilOffset = -(Vector3)recoiledDir * currentRecoilKickback * gunVisualKickbackScale;
        Vector3 localRecoilOffset = gunRoot.transform.parent != null
            ? gunRoot.transform.parent.InverseTransformVector(worldRecoilOffset)
            : worldRecoilOffset;

        localRecoilOffset = Vector3.ClampMagnitude(localRecoilOffset, gunVisualMaxOffsetDistance);
        gunRoot.transform.localPosition = gunRestLocalPosition + localRecoilOffset;

        float decayFactor = Mathf.Exp(-baseRecoverySpeed * weaponDef.recoilRecovery * Time.deltaTime);
        currentRecoilKickback *= Mathf.Pow(decayFactor, kickbackRecoverySpeedScale);
        currentRecoilRotation *= decayFactor;

        const float snapEpsilon = 0.001f;
        if (currentRecoilKickback < snapEpsilon) currentRecoilKickback = 0f;
        if (currentRecoilRotation < snapEpsilon) currentRecoilRotation = 0f;
    }

    private void UpdateSpreadBloom(WeaponComponentDefinition weaponDef)
    {
        float idleTime = Time.time - lastShotTime;
        if (idleTime > sprayHoldWindow)
        {
            consecutiveShotsFired = 0;
            float decayFactor = Mathf.Exp(-weaponDef.spreadRecoverySpeed * Time.deltaTime);
            currentSpread = weaponDef.baseSpreadDegrees + (currentSpread - weaponDef.baseSpreadDegrees) * decayFactor;
            if (currentSpread < weaponDef.baseSpreadDegrees + 0.01f) currentSpread = weaponDef.baseSpreadDegrees;
        }
    }

    private void UpdateLaserSight()
    {
        if (laserLineRenderer == null || gunMuzzle == null || !laserSightEnabled)
        {
            if (laserLineRenderer != null) laserLineRenderer.enabled = false;
            return;
        }

        Vector2 origin = gunMuzzle.position;
        Vector2 direction = RecoiledAimDirection;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, laserMaxDistance, laserHitMask);
        Vector2 endPoint = hit.collider != null ? hit.point : origin + direction * laserMaxDistance;

        laserLineRenderer.positionCount = 2;
        laserLineRenderer.SetPosition(0, origin);
        laserLineRenderer.SetPosition(1, endPoint);
    }

    private void TryFireWeapon(GunItemComponent gunComponent, WeaponComponentDefinition weaponDef)
    {
        if (!gunComponent.UseAmmo(true))
        {
            return;
        }

        gunComponent.UseAmmo(false);
        EquippedSlot.OnChanged.Invoke();
        
        gunComponent.myItemData.InvokeOnChanged();

        SetAmmoVisualizer();

        float spreadOffset = UnityEngine.Random.Range(-currentSpread, currentSpread);
        Vector2 shotDirection = (Quaternion.Euler(0f, 0f, spreadOffset) * RecoiledAimDirection).normalized;
        Quaternion shotRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(shotDirection.y, shotDirection.x) * Mathf.Rad2Deg);

        Shoot(shotDirection, weaponDef.baseDamage, shotRotation);

        currentRecoilKickback = Mathf.Clamp(currentRecoilKickback + weaponDef.recoilStrengthHorizontal, 0f, weaponDef.recoilStrengthHorizontal * maxRecoilKickbackStacks);
        currentRecoilRotation = Mathf.Clamp(currentRecoilRotation + weaponDef.recoilStrengthVertical, 0f, maxRecoilAngleDegrees);

        bool isSpraying = (Time.time - lastShotTime) <= sprayHoldWindow;
        consecutiveShotsFired = isSpraying ? consecutiveShotsFired + 1 : 1;
        lastShotTime = Time.time;

        float sprayBonus = weaponDef.spraySpreadBonusPerShot * (consecutiveShotsFired - 1);
        currentSpread = Mathf.Clamp(currentSpread + weaponDef.spreadPerShot + sprayBonus, weaponDef.baseSpreadDegrees, weaponDef.maxSpreadDegrees);

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

    #region Inventory Setup (Testing)

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

    #endregion
}