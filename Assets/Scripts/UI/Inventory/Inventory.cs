using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class Inventory : WindowBase
{
    #region Inventory_Promised
    public int InitializationPriority => 1;
    public bool IsInitialized;
    #endregion

    #region Inventory_Items
    private List<ItemInstance> items;
    private uint Currency;
    private WeaponItemInstance equippedWeapon;
    private int equippedIndex = -1;
    #endregion

    private ItemAggregate itemAggregate;
    private ItemIterator itemIterator;

    #region Inventory_UI
    private Dictionary<Guid, ItemFrame> itemFrames;
    private ItemInstance selectedItem;
    private GameObject framePrefab;
    [SerializeField] private Transform itemGroup;
    [SerializeField] private ScrollRect itemScroll;
    [SerializeField] private ItemMenu itemMenu;
    [SerializeField] private GameObject descriptionFrame;

    private ItemDescription itemDescription;
    private Status basicStatus;
    #endregion

    #region Inventory_Action_Events
    public event System.Action<WeaponItemInstance> OnWeaponEquipped;
    public event System.Action OnWeaponUnequipped;
    public event System.Action<uint> OnCurrencyChanged;
    #endregion

    private void Awake()
    {
        if (Singleton.Inventory != null)
        {
            Destroy(gameObject);
            return;
        }

        Singleton.Inventory = this;
        DontDestroyOnLoad(gameObject);

        itemDescription = GetComponentInChildren<ItemDescription>(true);
        basicStatus = GetComponentInChildren<Status>();

        items = new List<ItemInstance>();
        itemFrames = new Dictionary<Guid, ItemFrame>();
        itemAggregate = new ItemAggregate();
        itemIterator = itemAggregate.CreateIterator(items) as ItemIterator;

        framePrefab = Resources.Load<GameObject>(Path.Combine("Prefabs", "UI", "Inventory", "ItemFrame"));

        WindowType = WindowType.NormalWindow;
        IsInitialized = false;

        gameObject.SetActive(false);
    }

    #region Inventory_Save_Load
    public void SaveInventoryData()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "inventory_save.json");
        
        var inventory_saving = new InventoryWrapper();
        
        foreach (var item in items)
        {
            if (item is WeaponItemInstance weapon)
            {
                inventory_saving.weapons.Add(weapon);
            }
            else if (item is PotionItemInstance potion)
            {
                inventory_saving.potions.Add(potion);
            }
        }
        
        inventory_saving.Currency = Currency;
        
        string json = JsonUtility.ToJson(inventory_saving, true);
        File.WriteAllText(savePath, json);
    }

    public async Task LoadInventoryData()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "inventory_save.json");
        
        if (!File.Exists(savePath))
        {
            var defaultSaveData = new InventoryWrapper();  
            string defaultJson = JsonUtility.ToJson(defaultSaveData, true);
            File.WriteAllText(savePath, defaultJson);
            Debug.Log("<color=yellow>빈 인벤토리 데이터 생성</color>");
        }

        string json = File.ReadAllText(savePath);
        var inventory_loaded = JsonUtility.FromJson<InventoryWrapper>(json);

        items.Clear();
        if (inventory_loaded.weapons.Count > 0)
        {
            foreach (var weapon in inventory_loaded.weapons)
            {
                AddItem(weapon);
            }
        }

        if (inventory_loaded.potions.Count > 0)
        {
            foreach (var potion in inventory_loaded.potions)
            {
                AddItem(potion);
            }
        }

        InitInventoryFrame();
        Debug.Log("<color=green>인벤토리 데이터 로드 완료</color>");

        SetCurrency(inventory_loaded.Currency);

        Debug.Log("<color=green>재화 로드 완료</color>");

        //프리팹 로딩
        int count = items.Count;
        for (int i = 0; i < count; i++)
        {
            items[i].InventoryID = new Guid(items[i].InventoryIDString);
            if (items[i] is WeaponItemInstance weapon)
            {
                await weapon.LoadPrefabAsync();
            }
        }

        Debug.Log("<color=green>보유중인 무기 인스턴스화(프리팹 로드) 완료</color>");

        IsInitialized = true;
    }

    #endregion

    public void InitInventoryFrame()
    {
        Debug.Log($"[Inventory] InitInventoryFrame 시작. 아이템 개수: {items.Count}");
        if (framePrefab == null) Debug.LogError("[Inventory] framePrefab이 null입니다! Resources 경로를 확인하세요.");
        if (itemGroup == null) Debug.LogError("[Inventory] itemGroup 참조가 Inspector에서 누락되었습니다.");

        var children = itemGroup.GetComponentsInChildren<ItemFrame>();

        int count = children.Length;

        for(int i = count - 1; i >= 0; i--)
        {
            Destroy(children[i].gameObject);
        }
        itemFrames.Clear();

        itemIterator.Reset();
        while (itemIterator.HasNext())
        {
            var item = itemIterator.Next();
            AddItemFrame(item);
        }

        itemScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (Mathf.Ceil(items.Count * 0.25f) * 130 - 20));
    }

    #region Inventory_Item_USE
    public void UseItem(int _index)
    {
        if (_index < 0 || _index >= items.Count) return;

        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(items[_index].ItemID);

        switch ((ItemType)item_selected.ItemType)
        {
            case ItemType.Weapon:
                EquipWeapon(_index);
                break;
            case ItemType.Potion:
                UsePotion(_index);
                break;
        }
    }

    public void EquipWeapon(int _index)
    {
        if (_index < 0 || _index >= items.Count) return;

        var instance = items[_index] as WeaponItemInstance;
        if (instance == null) return;

        if (!CheckCanEquip(instance)) return;

        // 이전에 장착된 무기의 표시를 제거
        if (equippedWeapon != null && itemFrames.ContainsKey(equippedWeapon.InventoryID))
        {
            itemFrames[equippedWeapon.InventoryID].UnEquipped();
        }

        equippedWeapon = instance;
        equippedIndex = _index;

        // 새로 장착된 무기에 E 표시 추가
        if (itemFrames.ContainsKey(instance.InventoryID))
        {
            itemFrames[instance.InventoryID].SetEquipped();
        }

        OnWeaponEquipped.Invoke(instance);
    }

    public void UsePotion(int _index)
    {
        if (_index < 0 || _index >= items.Count) return;

        var potion = items[_index] as PotionItemInstance;
        if (potion == null) return;

        if (potion.CurrentAmount == 0) return;

        potion.OnUse();
    }

    public bool CheckCanEquip(WeaponItemInstance _instance)
    {
        var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(_instance.ItemID);

        if (weapon_selected.Require_STR <= Singleton.Player.GetCurrentLevel(LevelType.Strength) &&
            weapon_selected.Require_DEX <= Singleton.Player.GetCurrentLevel(LevelType.Dexterity) &&
            weapon_selected.Require_INT <= Singleton.Player.GetCurrentLevel(LevelType.Intelligent)) return true;

        Debug.Log($"요구 스탯이 부족합니다");
        return false;
    }

    //반드시 하나의 무기는 장착하는것이 강제
    //public void UnequipWeapon()
    //{
    //    if (equippedWeapon == null) return;

    //    // 장착 해제된 무기의 E 표시 제거
    //    if (itemFrames.ContainsKey(equippedWeapon.InventoryID))
    //    {
    //        itemFrames[equippedWeapon.InventoryID].UnEquipped();
    //    }

    //    equippedWeapon = null;
    //    equippedIndex = -1;
    //}
    #endregion

    public void AddItem(ItemInstance _item)
    {
        items.Add(_item);
    }

    public void AddItemFrame(ItemInstance _item)
    {
        GameObject obj = Instantiate(framePrefab, itemGroup);
        ItemFrame frame = obj.GetComponent<ItemFrame>();
        frame.SetFrameData(_item);
        frame.GetComponent<Toggle>().group = itemGroup.GetComponent<ToggleGroup>();
        itemFrames.Add(_item.InventoryID, frame);
    }

    public void TakeItem(ItemInstance _instance)
    {
        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(_instance.ItemID);

        bool potionAdded = false;
        itemIterator.Reset();
        while (itemIterator.HasNext())
        {
            ItemInstance instance = itemIterator.Next();

            if (instance is PotionItemInstance potion)
            {
                //동일한 아이템 획득시 카운트 증가
                if (potion.ItemID == _instance.ItemID)
                {
                    potion.MaxAmount += 1;
                    potionAdded = true;
                    break;
                }
            }
        }

        if (!potionAdded)
        {
            _instance.InventoryID = Guid.NewGuid();
            AddItem(_instance);
        }
    }

    public bool HasItem(int _itemID)
    {

        return false;
    }

    public int GetUpgradePrice(WeaponInstanceData _weapon)
    {
        return 0;
    }

    public int GetGrindReturnPrice(WeaponInstanceData _weapon)
    {
        return 0;
    }

    public int GrindWeapon(WeaponItemInstance _weapon)
    {
        if (equippedWeapon == _weapon)
        {
            Debug.Log("장착된 무기는 분해할 수 없습니다.");
            return 0;
        }

        //분해 후 아이템 삭제
        //계산식에 의해 가격 반환

        return 0;
    }

    public void SelectItem(ItemInstance _instance)
    {
        selectedItem = _instance;
        descriptionFrame.SetActive(_instance != null);
        itemDescription.UpdateDescription(_instance);

        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(_instance.ItemID);
        var weapon_selected = ((ItemType)item_selected.ItemType == ItemType.Weapon) ?
            Singleton.Get<TableDataManager>().Table.Weapon.Get(_instance.ItemID) : null;

        if (weapon_selected != null)
            basicStatus.UpdateStatus(_instance as WeaponItemInstance);
        else
            basicStatus.UpdateStatus();
    }

    public ItemInstance GetSelectedItem()
    {
        return selectedItem;
    }

    public void ShowItemMenu(ItemInstance _instance, RectTransform _frameTransform)
    {
        if (itemMenu == null) return;
        
        itemMenu.ShowItemMenu(_instance, _frameTransform.position);
    }

    private void SortItems(SortType _sortPriority, bool _ascendent)
    {

    }

    private void FilterItems(FilterType _filter)
    {

    }

    public int GetItemIndex(ItemInstance _item)
    {
        return items.IndexOf(_item);
    }

    public void SetIndicatorEquipped(Guid _inventoryID)
    {
        if (_inventoryID == Guid.Empty) return;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] is WeaponItemInstance weapon &&
                weapon.InventoryID == _inventoryID)
            {
                // 이전 장착 무기 표시 제거
                if (equippedWeapon != null && itemFrames.ContainsKey(equippedWeapon.InventoryID))
                {
                    itemFrames[equippedWeapon.InventoryID].UnEquipped();
                }

                equippedWeapon = weapon;
                equippedIndex = i;

                if (itemFrames.ContainsKey(weapon.InventoryID))
                {
                    itemFrames[weapon.InventoryID].SetEquipped();
                }

                OnWeaponEquipped.Invoke(weapon);
                break;
            }
        }
    }

    public WeaponItemInstance GetEquippedWeapon()
    {
        return equippedWeapon;
    }

    public Guid GetEquippedWeaponInvnetoryID()
    {
        if (equippedWeapon == null) return Guid.Empty;

        return equippedWeapon.InventoryID;
    }

    public void SetCurrency(uint _amount)
    {
        Currency = _amount;
        OnCurrencyChanged?.Invoke(Currency);
    }

    public void AddCurrency(uint _amount)
    {
        if (((UInt64)Currency + _amount) >= uint.MaxValue)
            Currency = uint.MaxValue;
        else
            Currency += _amount;

        OnCurrencyChanged?.Invoke(Currency);
    }

    public void MinusCurrency(uint _amount)
    {
        Currency = Currency - _amount;
        OnCurrencyChanged?.Invoke(Currency);
    }

    public uint GetCurrency()
    {
        return Currency;
    }

    public bool IsCurrencyEnough(uint _amount)
    {
        return Currency >= _amount;
    }

    public WeaponItemInstance GetWeaponByInventoryID(Guid _inventoryID)
    {
        //todo 이진탐색 고려
        int count = items.Count;
        for(int i = 0; i < count; i++)
        {
            if (items[i] is WeaponItemInstance weapon && weapon.InventoryID == _inventoryID)
                return weapon;
        }
            
        return null;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // 필요시 인벤토리 UI 데이터 갱신 등만 수행
    }
}
