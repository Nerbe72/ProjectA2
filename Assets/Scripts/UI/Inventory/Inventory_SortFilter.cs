using ExitGames.Client.Photon;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class Inventory : WindowBase
{
    [SerializeField] private ToggleGroup filterGroup;
    [SerializeField] private TMP_Dropdown sortDropdown;
    [SerializeField] private ToggleGroup sortGroup;

    private List<Toggle> filterToggles;
    private List<Toggle> sortToggles;

    private ItemFilterType currentFilter = ItemFilterType.All;
    private WeaponFilterType currentWeaponFilter = WeaponFilterType.All;
    private SortMainType currentSortMain = SortMainType.Rarity;
    private SortDirectionType currentSortDirection = SortDirectionType.Descending;

    private void Start()
    {
        // 필터 타입
        filterToggles = filterGroup.GetComponentsInChildren<Toggle>().ToList();

        for(int i = 0; i < filterToggles.Count; i++)
        {
            var index = i;
            filterToggles[index].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    SetFilter((ItemFilterType)index);
                    ApplyFilterAndSort();
                }
            });

            if ((ItemFilterType)index == ItemFilterType.Weapon)
            {
                var eventTrigger = filterToggles[index].GetComponent<EventTrigger>();
                var entry = new EventTrigger.Entry();

                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((data) =>
                {
                    if (filterToggles[index].isOn)
                    {
                        CycleWeaponFilterType();
                        UpdateWeaponFilterImage();
                        ApplyFilterAndSort();
                    }
                });

                eventTrigger.triggers.Add(entry);
            }
        }

        Image weaponFilterImage = filterToggles[1].GetComponentInChildren<Image>();
        GameObject[] weaponFilterIndicators = new GameObject[System.Enum.GetValues(typeof(WeaponFilterType)).Length];
        
        // 무기 타입별
        Button weaponToggleButton = filterToggles[1].GetComponent<Button>();
        if (weaponToggleButton == null)
        {
            weaponToggleButton = filterToggles[1].gameObject.AddComponent<Button>();
        }
        
        // 정렬 타입
        sortDropdown.onValueChanged.AddListener((index) =>
        {
            SetSortMain((SortMainType)index);
            ApplyFilterAndSort();
        });

        // 정렬 방향
        sortToggles = sortGroup.GetComponentsInChildren<Toggle>().ToList();
        for (int i = 0; i < sortToggles.Count; i++)
        {
            var index = i;
            sortToggles[index].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    SetSortDirection((SortDirectionType)index);
                    ApplyFilterAndSort();
                }
            });
        }

        // 초기화
        filterToggles[0].isOn = true;
        filterToggles[0].onValueChanged.Invoke(true);

        sortDropdown.value = (int)SortMainType.Rarity;
        sortDropdown.onValueChanged.Invoke((int)SortMainType.Rarity);

        sortToggles[(int)SortDirectionType.Descending].isOn = true;
        sortToggles[(int)SortDirectionType.Descending].onValueChanged.Invoke(true);
    }

    private void ApplyFilterAndSort()
    {
        if (items == null || items.Count == 0)
            return;

        List<ItemInstance> filteredItems = FilterItems(items.AsReadOnly());
        List<ItemInstance> sortedItems = SortItems(filteredItems);
        
        UpdateInventoryUI(sortedItems);
    }
    
    private void UpdateInventoryUI(List<ItemInstance> _sortedItems)
    {
        foreach (var frame in itemFrames.Values)
        {
            frame.gameObject.SetActive(false);
        }
        
        for (int i = 0; i < _sortedItems.Count; i++)
        {
            ItemInstance item = _sortedItems[i];
            if (itemFrames.TryGetValue(item.InventoryID, out ItemFrame frame))
            {
                frame.gameObject.SetActive(true);
                frame.transform.SetSiblingIndex(i);
            }
        }
    }

    private List<ItemInstance> FilterItems(IReadOnlyList<ItemInstance> _items)
    {
        if (_items == null) return new List<ItemInstance>();

        TableDataManager tableDataManager = Singleton.Get<TableDataManager>();
        List<ItemInstance> result = new List<ItemInstance>();

        int count = _items.Count;
        for (int i = 0; i < count; i++)
        {
            var item = _items[i];
            var itemData = tableDataManager.Table.Item.Get(item.ItemID);

            switch (currentFilter)
            {
                case ItemFilterType.All:
                    result.Add(_items[i]);
                    break;
                case ItemFilterType.Weapon:
                    {
                        if ((ItemType)itemData.ItemType == ItemType.Weapon)
                        {
                            if (currentWeaponFilter == WeaponFilterType.All)
                            {
                                result.Add(item);
                            }
                            else
                            {
                                var weaponData = tableDataManager.Table.Weapon.Get(item.ItemID);
                                if (IsWeaponMatched((WeaponType)weaponData.WeaponType, currentWeaponFilter))
                                {
                                    result.Add(item);
                                }
                            }
                        }
                    }
                    break;
                case ItemFilterType.Potion:
                    if ((ItemType)itemData.ItemType == ItemType.Potion)
                        result.Add(item);
                    break;
            }
        }

        return result;
    }

    private bool IsWeaponMatched(WeaponType _weaponType, WeaponFilterType _filterType)
    {
        switch (_filterType)
        {
            case WeaponFilterType.All:
            default:
                return true;
            case WeaponFilterType.Melee:
                return _weaponType == WeaponType.Melee;
            case WeaponFilterType.Bow:
                return _weaponType == WeaponType.Bow;
            case WeaponFilterType.Magic:
                return _weaponType == WeaponType.Magic;
        }
    }

    private void SetFilter(ItemFilterType _type)
    {
        var localeTable = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;

        if (localeTable == null)
            return;

        sortDropdown.ClearOptions();
        currentFilter = _type;

        switch (_type)
        {
            case ItemFilterType.All:
            case ItemFilterType.Weapon:
                {
                    sortDropdown.AddOptions(new List<string>
                    {
                        localeTable.Get((int)SortMainType.Rarity + 100, locale),
                        localeTable.Get((int)SortMainType.Damage + 100, locale),
                        localeTable.Get((int)SortMainType.Defense + 100, locale)
                    });
                }
                break;
            case ItemFilterType.Potion:
                {
                    sortDropdown.AddOptions(new List<string>
                    {
                        localeTable.Get((int)SortMainType.Rarity + 100, locale)
                    });
                }
                break;
        }

        // default dropdown value
        sortDropdown.value = 0;
    }

    private void SetSortMain(SortMainType _sortMainType)
    {
        currentSortMain = _sortMainType;
    }

    private void SetSortDirection(SortDirectionType _sortDirectionType)
    {
        currentSortDirection = _sortDirectionType;
    }
    
    private void CycleWeaponFilterType()
    {
        int nextTypeIndex = ((int)currentWeaponFilter + 1) % System.Enum.GetValues(typeof(WeaponFilterType)).Length;
        currentWeaponFilter = (WeaponFilterType)nextTypeIndex;
    }
    
    private void UpdateWeaponFilterImage()
    {
        //체크박스 색상
        Image weaponFilterImage = filterToggles[(int)ItemFilterType.Weapon].graphic.GetComponent<Image>();
        if (weaponFilterImage == null) return;
        
        switch (currentWeaponFilter)
        {
            case WeaponFilterType.All:
                weaponFilterImage.color = Color.white;
                break;
            case WeaponFilterType.Melee:
                weaponFilterImage.color = Color.grey;
                break;
            case WeaponFilterType.Bow:
                weaponFilterImage.color = Color.green;
                break;
            case WeaponFilterType.Magic:
                weaponFilterImage.color = Color.red;
                break;
        }

        // 아이콘 변경
        Image weaponFilterIcon = filterToggles[(int)ItemFilterType.Weapon].GetComponentInChildren<Finder>().GetComponent<Image>();
        Sprite weaponFilterSprite = ResourceLoader.Load<Sprite>(currentWeaponFilter.ToString(), LoadType.Icon);

        if (weaponFilterSprite != null)
        {
            weaponFilterIcon.sprite = weaponFilterSprite;
        }
        else
        {
            weaponFilterIcon.color = Color.clear;
        }
    }

    private List<ItemInstance> SortItems(IReadOnlyList<ItemInstance> _items)
    {
        if (_items == null || _items.Count == 0)
            return new List<ItemInstance>();

        List<ItemInstance> result = new List<ItemInstance>(_items);
        TableDataManager tableDataManager = Singleton.Get<TableDataManager>();

        Player player = Singleton.Player;

        switch (currentSortMain)
        {
            case SortMainType.Rarity:
            if (currentSortDirection == SortDirectionType.Descending)
            {
                result.Sort((a, b) => tableDataManager.Table.Item.Get(b.ItemID).Rarity.CompareTo(
                            tableDataManager.Table.Item.Get(a.ItemID).Rarity)); // 내림차순: 높은 레어도가 앞으로
            }
            else
            {
                result.Sort((a, b) => tableDataManager.Table.Item.Get(a.ItemID).Rarity.CompareTo(
                            tableDataManager.Table.Item.Get(b.ItemID).Rarity)); // 오름차순: 낮은 레어도가 앞으로
            }
                break;
                
            case SortMainType.Damage:
                if (currentSortDirection == SortDirectionType.Descending)
                {
                    result.Sort((a, b) => {
                        bool isWeaponA = a is WeaponItemInstance;
                        bool isWeaponB = b is WeaponItemInstance;
                        
                        if (!isWeaponA && !isWeaponB)
                            return 0;
                        
                        if (!isWeaponA)
                            return 1;
                        
                        if (!isWeaponB)
                            return -1;
                        
                        int damageA = player.GetCalculatedDamage(a as WeaponItemInstance);
                        int damageB = player.GetCalculatedDamage(b as WeaponItemInstance);
                        return damageB.CompareTo(damageA);
                    });
                }
                else
                {
                    result.Sort((a, b) => {
                        bool isWeaponA = a is WeaponItemInstance;
                        bool isWeaponB = b is WeaponItemInstance;
                        
                        if (!isWeaponA && !isWeaponB) return 0;
                        
                        if (!isWeaponA) return 1;
                        
                        if (!isWeaponB) return -1;
                        
                        int damageA = player.GetCalculatedDamage(a as WeaponItemInstance);
                        int damageB = player.GetCalculatedDamage(b as WeaponItemInstance);
                        return damageB.CompareTo(damageA);
                    });
                }
                break;
                
            case SortMainType.Defense:
                if (currentSortDirection == SortDirectionType.Descending)
                {
                    result.Sort((a, b) => {
                        bool isWeaponA = a is WeaponItemInstance;
                        bool isWeaponB = b is WeaponItemInstance;
                        
                        if (!isWeaponA && !isWeaponB) return 0;
                        
                        if (!isWeaponA) return 1;
                        
                        if (!isWeaponB) return -1;
                        
                        int defenseA = player.GetCalculatedDefense(a as WeaponItemInstance);
                        int defenseB = player.GetCalculatedDefense(b as WeaponItemInstance);
                        return defenseB.CompareTo(defenseA);
                    });
                }
                else
                {
                    result.Sort((a, b) => {
                        bool isWeaponA = a is WeaponItemInstance;
                        bool isWeaponB = b is WeaponItemInstance;
                        
                        if (!isWeaponA && !isWeaponB) return 0;
                        
                        if (!isWeaponA) return 1;
                        
                        if (!isWeaponB) return -1;
                        
                        int defenseA = player.GetCalculatedDefense(a as WeaponItemInstance);
                        int defenseB = player.GetCalculatedDefense(b as WeaponItemInstance);
                        return defenseA.CompareTo(defenseB);
                    });
                }
                break;
        }
        
        return result;
    }
}
