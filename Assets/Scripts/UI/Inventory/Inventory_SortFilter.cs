using GameStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class Inventory : WindowBase
{
    [SerializeField] private TMP_Dropdown sortDropdown;
    [SerializeField] private ToggleGroup sortGroup;

    private List<Toggle> sortToggles;

    private int filterIndex = 0;
    private int filterSubIndex = 0;

    private int sortIndex = 0;
    private SortDirectionType currentSortDirection = SortDirectionType.Descending;

    private Filter filter;

    private void Start()
    {
        filter.OnFilterSelected += (index, subindex) =>
        {
            SetSortOptions(index, subindex);
            ApplyFilterAndSort();
        };

        // 정렬 타입
        sortDropdown.onValueChanged.AddListener((index) =>
        {
            SetSortMain((SortType)index);
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

        // Filter에서 현재 선택된 필터 정보를 받아와서 초기 설정
        int currentFilterIndex = filter.GetCurrentFilterIndex();
        int currentFilterSubIndex = filter.GetCurrentFilterSubIndex();
        SetSortOptions(currentFilterIndex, currentFilterSubIndex);

        // 인벤토리가 이미 로드된 경우 필터 적용
        if (items != null && items.Count > 0)
        {
            ApplyFilterAndSort();
        }

        sortDropdown.value = (int)SortType.Rarity;
        sortDropdown.onValueChanged.Invoke((int)SortType.Rarity);

        sortToggles[(int)SortDirectionType.Descending].isOn = true;
        sortToggles[(int)SortDirectionType.Descending].onValueChanged.Invoke(true);
    }

    private void ApplyFilterAndSort()
    {
        if (items == null || items.Count == 0)
            return;

        List<ItemInstance> filteredItems = FilterItems(items.AsReadOnly(), filterIndex, filterSubIndex);
        List<ItemInstance> sortedItems = SortItems(filteredItems);
        
        UpdateUI(sortedItems);
    }
    
    private void UpdateUI(List<ItemInstance> _sortedItems)
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

                if (item is WeaponItemInstance weapon)
                {
                    switch ((SortType)sortIndex)
                    {
                        case SortType.Rarity:
                            frame.SetItemDetail("");
                            break;
                        case SortType.Damage:
                            frame.SetItemDetail($"{Singleton.Player.GetCalculatedDamage(weapon)}");
                            break;
                        case SortType.Defense:
                            frame.SetItemDetail($"{Singleton.Player.GetCalculatedDefense(weapon)}");
                            break;
                    }
                } else if (item is IStackable stackable)
                {
                    frame.SetItemDetail($"{stackable.CurrentStack}/{stackable.MaxStackSize}");
                }
            }
        }
    }

    private List<ItemInstance> FilterItems(IReadOnlyList<ItemInstance> _items, int _filterIndex, int _filterSubIndex)
    {
        if (_items == null || _items.Count == 0) return new List<ItemInstance>();

        if ((ItemType)_filterIndex == ItemType.Total)
        {
            return _items.ToList();
        }

        TableDataManager tableDataManager = Singleton.Get<TableDataManager>();
        List<ItemInstance> result = new List<ItemInstance>();

        int count = _items.Count;
        for (int i = 0; i < count; i++)
        {
            var item = _items[i];
            var itemData = tableDataManager.Table.Item.Get(item.ItemID);

            switch ((ItemType)_filterIndex)
            {
                case ItemType.Weapon:
                    {
                        if ((ItemType)itemData.ItemType == (ItemType)_filterIndex)
                        {
                            if ((WeaponFilterType)_filterSubIndex == WeaponFilterType.All)
                            {
                                result.Add(item);
                            }
                            else
                            {
                                var weaponData = tableDataManager.Table.Weapon.Get(item.ItemID);
                                if (weaponData != null && IsWeaponMatched((WeaponType)weaponData.WeaponType, (WeaponFilterType)_filterSubIndex))
                                {
                                    result.Add(item);
                                }
                            }
                        }
                    }
                    break;
                default:
                case ItemType.Material:
                case ItemType.Scroll:
                case ItemType.Skill:
                case ItemType.Potion:
                    if ((ItemType)itemData.ItemType == (ItemType)_filterIndex)
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

    private void SetSortOptions(int _index, int _subIndex)
    {
        filterIndex = _index;
        filterSubIndex = _subIndex;

        var localeTable = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;

        if (localeTable == null)
            return;

        sortDropdown.ClearOptions();

        switch ((ItemType)_index)
        {
            case ItemType.Total:
            case ItemType.Weapon:
                {
                    sortDropdown.AddOptions(new List<string>
                    {
                        localeTable.Get((int)SortType.Rarity + 100, locale),
                        localeTable.Get((int)SortType.Damage + 100, locale),
                        localeTable.Get((int)SortType.Defense + 100, locale)
                    });
                }
                break;
            default:
                {
                    sortDropdown.AddOptions(new List<string>
                    {
                        localeTable.Get((int)SortType.Rarity + 100, locale)
                    });
                }
                break;
        }
    }

    private void SetSortMain(SortType _sortMainType)
    {
        sortIndex = (int)_sortMainType;
    }

    private void SetSortDirection(SortDirectionType _sortDirectionType)
    {
        currentSortDirection = _sortDirectionType;
    }
    
    private List<ItemInstance> SortItems(IReadOnlyList<ItemInstance> _items)
    {
        if (_items == null || _items.Count == 0)
            return new List<ItemInstance>();

        List<ItemInstance> result = new List<ItemInstance>(_items);
        TableDataManager tableDataManager = Singleton.Get<TableDataManager>();

        Player player = Singleton.Player;

        switch ((SortType)sortIndex)
        {
            case SortType.Rarity:
            if (currentSortDirection == SortDirectionType.Descending)
            {
                // 내림차순
                result.Sort((a, b) => tableDataManager.Table.Item.Get(b.ItemID).Rarity.CompareTo(
                        tableDataManager.Table.Item.Get(a.ItemID).Rarity));
            }
            else
            {
                // 오름차순
                result.Sort((a, b) => tableDataManager.Table.Item.Get(a.ItemID).Rarity.CompareTo(
                        tableDataManager.Table.Item.Get(b.ItemID).Rarity)); 
            }
                break;
                
            case SortType.Damage:
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
                
            case SortType.Defense:
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
