using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using GameStuff;

public partial class Inventory : WindowBase
{
    #region Inventory_Items
    private List<ItemInstance> items;
    private uint Currency;
    private System.Guid equippedWeaponId = System.Guid.Empty;
    private int equippedIndex = -1;
    private Dictionary<Guid, WeaponEnhancementAdapter> weaponAdapters;
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
    public event System.Action<ItemInstance> OnInventoryItemAdded;
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
        weaponAdapters = new Dictionary<Guid, WeaponEnhancementAdapter>();
        
        equippedWeaponId = System.Guid.Empty;

        filter = GetComponentInChildren<Filter>(true);

        framePrefab = Resources.Load<GameObject>(Path.Combine("Prefabs", "UI", "Inventory", "ItemFrame"));

        WindowType = WindowType.NormalWindow;

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
            else if (item is MaterialItemInstance material)
            {
                inventory_saving.materials.Add(material);
            }
            else if (item is ScrollItemInstance scroll)
            {
                inventory_saving.scrolls.Add(scroll);
            }
            else if (item is SkillItemInstance skill)
            {
                inventory_saving.skills.Add(skill);
            }
        }

        inventory_saving.Currency = Currency;

        // 어댑터 데이터 저장
        foreach (var adapterEntry in weaponAdapters)
        {
            var adapter = adapterEntry.Value;
            adapter.weaponInventoryIDString = adapter.weaponInventoryID.ToString();
            inventory_saving.weaponAdapters.Add(adapter);
        }

        string json = JsonUtility.ToJson(inventory_saving, true);
        File.WriteAllText(savePath, json);
        Debug.Log("<color=green>인벤토리 저장됨</color>");
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
                if (!Guid.TryParse(weapon.InventoryIDString, out Guid weaponGuid))
                {
                    weaponGuid = Guid.NewGuid();
                    Debug.LogWarning($"무기 InventoryID 변환 실패, 새로운 GUID 생성: {weaponGuid}");
                }
                weapon.InventoryID = weaponGuid;
                AddItem(weapon);
            }
        }

        if (inventory_loaded.scrolls.Count > 0)
        {
            foreach (var scroll in inventory_loaded.scrolls)
            {
                if (!Guid.TryParse(scroll.InventoryIDString, out Guid scrollGuid))
                {
                    scrollGuid = Guid.NewGuid();
                    Debug.LogWarning($"스크롤 InventoryID 변환 실패, 새로운 GUID 생성: {scrollGuid}");
                }
                scroll.InventoryID = scrollGuid;
                AddItem(scroll);
            }
        }

        if (inventory_loaded.potions.Count > 0)
        {
            foreach (var potion in inventory_loaded.potions)
            {
                if (!Guid.TryParse(potion.InventoryIDString, out Guid potionGuid))
                {
                    potionGuid = Guid.NewGuid();
                    Debug.LogWarning($"포션 InventoryID 변환 실패, 새로운 GUID 생성: {potionGuid}");
                }
                potion.InventoryID = potionGuid;
                AddItem(potion);
            }
        }

        if (inventory_loaded.skills.Count > 0)
        {
            foreach (var skill in inventory_loaded.skills)
            {
                if (!Guid.TryParse(skill.InventoryIDString, out Guid skillGuid))
                {
                    skillGuid = Guid.NewGuid();
                    Debug.LogWarning($"스킬 InventoryID 변환 실패, 새로운 GUID 생성: {skillGuid}");
                }
                skill.InventoryID = skillGuid;
                AddItem(skill);
            }
        }

        if (inventory_loaded.materials.Count > 0)
        {
            foreach (var material in inventory_loaded.materials)
            {
                if (!Guid.TryParse(material.InventoryIDString, out Guid materialGuid))
                {
                    materialGuid = Guid.NewGuid();
                    Debug.LogWarning($"재료 InventoryID 변환 실패, 새로운 GUID 생성: {materialGuid}");
                }
                material.InventoryID = materialGuid;
                AddItem(material);
            }
        }

        // 아이템 로드 직후에 어댑터 데이터 로드
        LoadWeaponAdapters(inventory_loaded);

        InitInventoryFrame();
        Debug.Log("<color=green>인벤토리 데이터 로드 완료</color>");

        SetCurrency(inventory_loaded.Currency);

        Debug.Log("<color=green>재화 로드 완료</color>");

        List<Task> prefabTasks = new List<Task>();
        //프리팹 로드
        int count = items.Count;
        for (int i = 0; i < count; i++)
        {
            if (!Guid.TryParse(items[i].InventoryIDString, out Guid itemGuid))
            {
                itemGuid = Guid.NewGuid();
                Debug.LogWarning($"아이템 InventoryID 변환 실패, 새로운 GUID 생성: {itemGuid}");
            }
            items[i].InventoryID = itemGuid;
            if (items[i] is WeaponItemInstance weapon)
            {
                prefabTasks.Add(weapon.LoadPrefabAsync());
            }
        }

        int prefabCount = prefabTasks.Count;
        while (true)
        {
            bool loadRemained = false;
            for (int i = 0; i < prefabCount; i++)
            {
                if (!prefabTasks[i].IsCompleted)
                {
                    loadRemained = true;
                    break;
                }
            }

            if (!loadRemained) break;

            await Task.Yield();
        }
        
        Debug.Log("<color=green>보유중인 무기 인스턴스화(프리팹 로드) 완료</color>");

    }

    #endregion

    private void LoadWeaponAdapters(InventoryWrapper inventory_loaded)
    {
        // 1단계: 저장된 어댑터 데이터들을 모두 로드해서 딕셔너리에 넣기
        foreach (var savedAdapter in inventory_loaded.weaponAdapters)
        {
            if (Guid.TryParse(savedAdapter.weaponInventoryIDString, out Guid weaponGuid))
            {
                // 해당 무기 인스턴스를 찾기
                foreach (var item in items)
                {
                    if (item is WeaponItemInstance weapon && weapon.InventoryID == weaponGuid)
                    {
                        // 어댑터 생성하고 딕셔너리에 저장
                        var adapter = new WeaponEnhancementAdapter();
                        adapter.Init(weapon);
                        adapter.weaponInventoryID = weaponGuid;
                        adapter.weaponInventoryIDString = savedAdapter.weaponInventoryIDString;
                        adapter.enhancedLevel = savedAdapter.enhancedLevel;
                        adapter.enchantedSkillIds = new List<int>(savedAdapter.enchantedSkillIds);
                        weaponAdapters[weaponGuid] = adapter;
                        break;
                    }
                }
            }
        }
        
        // 2단계: 모든 무기에 대해 어댑터가 있는지 확인하고 없으면 새로 생성
        foreach (var item in items)
        {
            if (item is WeaponItemInstance weapon)
            {
                if (!weaponAdapters.ContainsKey(weapon.InventoryID))
                {
                    // 새 어댑터 생성
                    var adapter = new WeaponEnhancementAdapter();
                    adapter.Init(weapon);
                    adapter.weaponInventoryID = weapon.InventoryID;
                    adapter.weaponInventoryIDString = weapon.InventoryID.ToString();
                    adapter.enhancedLevel = 0;
                    adapter.enchantedSkillIds = new List<int>();
                    weaponAdapters[weapon.InventoryID] = adapter;
                    Debug.Log($"새 어댑터 생성: 무기 ID {weapon.ItemID}");
                }
            }
        }
    }

    public void InitInventoryFrame()
    {
        Debug.Log($"[Inventory] InitInventoryFrame 시작. 아이템 개수: {items.Count}");
        if (framePrefab == null) Debug.LogError("[Inventory] framePrefab이 null입니다. Resources 경로를 확인하세요.");
        if (itemGroup == null) Debug.LogError("[Inventory] itemGroup이 null입니다. Inspector에서 확인해주세요.");

        var existingFrames = itemGroup.GetComponentsInChildren<ItemFrame>();
        
        itemIterator.Reset();
        int itemIndex = 0;
        
        while (itemIterator.HasNext())
        {
            var item = itemIterator.Next();
            
            if (itemIndex < existingFrames.Length)
            {
                var existingFrame = existingFrames[itemIndex];
                existingFrame.gameObject.SetActive(true);
                _ = existingFrame.SetFrameData(item);
            }
            else
            {
                AddItemFrame(item);
            }
            
            itemIndex++;
        }
        
        for (int i = itemIndex; i < existingFrames.Length; i++)
        {
            existingFrames[i].gameObject.SetActive(false);
        }

        RefreshContentHeight();
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
            case ItemType.Skill:
                UseSkill(_index);
                break;
            case ItemType.Material:
                UseMaterial(_index);
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
        if (equippedWeaponId != System.Guid.Empty && itemFrames.ContainsKey(equippedWeaponId))
        {
            itemFrames[equippedWeaponId].UnEquipped();
        }

        equippedWeaponId = instance.InventoryID;
        equippedIndex = _index;

        // 새로 장착된 무기에 E 표시 추가
        if (itemFrames.ContainsKey(instance.InventoryID))
        {
            itemFrames[instance.InventoryID].SetEquipped();
        }

        OnWeaponEquipped.Invoke(instance);
        Singleton.Player?.SavePlayerDataWithoutPosition();
    }

    public void UsePotion(int _index)
    {
        if (_index < 0 || _index >= items.Count) return;

        var potion = items[_index] as PotionItemInstance;
        if (potion == null) return;

        if (potion.CurrentStack == 0) return;

        bool used = potion.OnUse();
        
        // 스택이 0이 되면 아이템 제거
        if (used && potion.CurrentStack <= 0)
        {
            RemoveItem(_index);
        }
        else
        {
            // UI 업데이트
            if (itemFrames.TryGetValue(potion.InventoryID, out ItemFrame frame))
            {
                frame.SetItemDetail(potion.CurrentStack.ToString());
            }
        }
    }

    public void UseSkill(int _index)
    {
        if (_index < 0 || _index >= items.Count) return;

        var skill = items[_index] as SkillItemInstance;
        if (skill == null) return;

        if (skill.CurrentStack == 0) return;

        bool used = skill.OnUse();
        
        // 스택이 0이 되면 아이템 제거
        if (used && skill.CurrentStack <= 0)
        {
            RemoveItem(_index);
        }
        else
        {
            // UI 업데이트
            if (itemFrames.TryGetValue(skill.InventoryID, out ItemFrame frame))
            {
                frame.SetItemDetail($"{skill.CurrentStack}/{skill.MaxStackSize}");
            }
        }
    }

    public void UseMaterial(int _index)
    {
        if (_index < 0 || _index >= items.Count) return;

        var material = items[_index] as MaterialItemInstance;
        if (material == null) return;

        if (material.CurrentStack == 0) return;

        bool used = material.OnUse();
        
        // 스택이 0이 되면 아이템 제거
        if (used && material.CurrentStack <= 0)
        {
            RemoveItem(_index);
        }
        else
        {
            // UI 업데이트
            if (itemFrames.TryGetValue(material.InventoryID, out ItemFrame frame))
            {
                frame.SetItemDetail($"{material.CurrentStack}/{material.MaxStackSize}");
            }
        }
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
    #endregion

    public void AddItem(ItemInstance _item)
    {
        items.Add(_item);
        OnInventoryItemAdded?.Invoke(_item);
        
        AddItemFrame(_item);
    }

    public async void AddItemFrame(ItemInstance _item)
    {
        GameObject obj = Instantiate(framePrefab, itemGroup);
        ItemFrame frame = obj.GetComponent<ItemFrame>();
        await frame.SetFrameData(_item);
        frame.GetComponent<Toggle>().group = itemGroup.GetComponent<ToggleGroup>();
        frame.OnFrameSelected += (itemInstance) =>
        {
            SelectItem(itemInstance);
        };

        frame.OnRightClick += (itemInstance, frameTransform) =>
        {
            ShowItemMenu(itemInstance, frameTransform);
        };

        itemFrames.Add(_item.InventoryID, frame);
        ApplyFilterAndSort();
        RefreshContentHeight();
    }

    public void TakeItem(ItemInstance _instance)
    {
		if (_instance is ScrollItemInstance)
		{
			if (HasItem(_instance.ItemID))
				return;
		}

        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(_instance.ItemID);

        bool stackableItemAdded = false;
        itemIterator.Reset();
        while (itemIterator.HasNext())
        {
            ItemInstance instance = itemIterator.Next();

            // 스택 가능한 아이템들 처리
            if (instance is IStackable existingStackable && _instance is IStackable newStackable)
            {
                if (instance.ItemID == _instance.ItemID)
                {
                    // 동일한 아이템이면 스택 증가
                    existingStackable.AddMaxStackSize(newStackable.MaxStackSize);
                    existingStackable.CurrentStack += newStackable.CurrentStack;
                    stackableItemAdded = true;
                    
                    ApplyFilterAndSort();
                    break;
                }
            }
        }

        if (!stackableItemAdded)
        {
            _instance.InventoryID = Guid.NewGuid();
            AddItem(_instance);
        }
        else
            ApplyFilterAndSort();
    }

    public bool HasItem(int _itemID)
    {
        itemIterator.Reset();
        while (itemIterator.HasNext())
        {
            var instance = itemIterator.Next();
            if (instance.ItemID == _itemID)
                return true;
        }

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
        if (equippedWeaponId == _weapon.InventoryID)
        {
            Debug.Log("장착된 무기는 분해할 수 없습니다.");
            return 0;
        }

        // todo: 분해 후 아이템 삭제
        // 계산식에 따른 가격 반환

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

    public int GetItemIndex(ItemInstance _item)
    {
        return items.IndexOf(_item);
    }

    public List<ItemInstance> GetItemsByType(ItemType _type)
    {
        switch( _type)
        {
            case ItemType.Weapon:
                return items.FindAll(item => item is WeaponItemInstance);
            case ItemType.Potion:
                return items.FindAll(item => item is PotionItemInstance);
            case ItemType.Material:
                return items.FindAll(item => item is MaterialItemInstance);
            case ItemType.Scroll:
                return items.FindAll(item => item is ScrollItemInstance);
            case ItemType.Skill:
                return items.FindAll(item => item is SkillItemInstance);
            default:
                return new List<ItemInstance>();
        }
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
                if (equippedWeaponId != System.Guid.Empty && itemFrames.ContainsKey(equippedWeaponId))
                {
                    itemFrames[equippedWeaponId].UnEquipped();
                }

                equippedWeaponId = weapon.InventoryID;
                equippedIndex = i;

                if (itemFrames.ContainsKey(weapon.InventoryID))
                {
                    itemFrames[weapon.InventoryID].SetEquipped();
                }

                OnWeaponEquipped.Invoke(weapon);
                Singleton.Player?.SavePlayerDataWithoutPosition();
                break;
            }
        }
    }

    public WeaponItemInstance GetEquippedWeapon()
    {
        if (equippedWeaponId == System.Guid.Empty) return null;
        return GetWeaponByInventoryID(equippedWeaponId);
    }

    public Guid GetEquippedWeaponInventoryID()
    {
        return equippedWeaponId;
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

    public List<ItemInstance> GetAllInventoryItems()
    {
        return items;
    }

    public WeaponItemInstance GetWeaponByInventoryID(Guid _inventoryID)
    {
        int count = items.Count;
        for (int i = 0; i < count; i++)
        {
            if (items[i] is WeaponItemInstance weapon && weapon.InventoryID == _inventoryID)
                return weapon;
        }

        return null;
    }

    public WeaponEnhancementAdapter GetWeaponAdapter(WeaponItemInstance _weapon)
    {
        if (_weapon == null) return null;
        
        // 어댑터가 없으면 생성
        if (!weaponAdapters.ContainsKey(_weapon.InventoryID))
        {
            var adapter = new WeaponEnhancementAdapter();
            adapter.Init(_weapon);
            weaponAdapters[_weapon.InventoryID] = adapter;
        }
        
        return weaponAdapters[_weapon.InventoryID];
    }
    
    public int RemoveItemByID(int itemId, int removeCount = 1)
    {
        if (removeCount <= 0)
        {
            Debug.LogWarning($"잘못된 제거 수량: {removeCount}");
            return 0;
        }

        int removedCount = 0;
        
        for (int i = items.Count - 1; i >= 0 && removedCount < removeCount; i--)
        {
            var item = items[i];
            if (item.ItemID != itemId) continue;

            if (item is IStackable stackableItem)
            {
                int currentStack = stackableItem.CurrentStack;
                int needToRemove = removeCount - removedCount;
                
                if (currentStack <= needToRemove)
                {
                    // 전체 스택 제거
                    removedCount += currentStack;
                    RemoveItemAtIndex(i);
                }
                else
                {
                    // 일부만 제거
                    stackableItem.CurrentStack -= needToRemove;
                    removedCount += needToRemove;
                    
                    // UI 업데이트 (수량 변경)
                    if (itemFrames.ContainsKey(item.InventoryID))
                    {
                        var frame = itemFrames[item.InventoryID];
                        frame?.SetItemDetail(stackableItem.CurrentStack.ToString());
                    }
                }
            }
            else
            {
                RemoveItemAtIndex(i);
                removedCount++;
            }
        }
        
        return removedCount;
    }
    
    private bool RemoveItemAtIndex(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            Debug.LogWarning($"잘못된 인덱스: {index}");
            return false;
        }

        var itemToRemove = items[index];
        
        if (itemToRemove is WeaponItemInstance weapon && weapon.InventoryID == equippedWeaponId)
        {
            equippedWeaponId = System.Guid.Empty;
            equippedIndex = -1;
            OnWeaponEquipped?.Invoke(null);
        }
        
        items.RemoveAt(index);
        
        if (itemFrames.ContainsKey(itemToRemove.InventoryID))
        {
            var frame = itemFrames[itemToRemove.InventoryID];
            itemFrames.Remove(itemToRemove.InventoryID);
            
            if (frame != null && frame.gameObject != null)
            {
                Destroy(frame.gameObject);
            }
        }
        
        RefreshContentHeight();
        
        return true;
    }
    
    public bool RemoveItem(ItemInstance item)
    {
        int index = GetItemIndex(item);
        if (index == -1)
        {
            Debug.LogWarning("제거하려는 아이템을 찾을 수 없습니다.");
            return false;
        }
        
        return RemoveItemAtIndex(index);
    }
    
    public bool RemoveItem(int index)
    {
        return RemoveItemAtIndex(index);
    }
    
    private void RefreshContentHeight()
    {
        int totalFrames = itemFrames.Count;
        itemScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            (Mathf.Ceil(totalFrames * 0.25f) * 130 - 20));
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }
}
