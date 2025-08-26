using GameStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeList : MonoBehaviour
{
    [SerializeField] private GameObject scrollFrame;
    [SerializeField] private Transform itemGroup;
    [SerializeField] private ScrollRect itemScroll;

    [SerializeField] private TMP_Dropdown sortDropdown;
    [SerializeField] private ToggleGroup sortGroup;

    private List<Toggle> sortToggles;

    private int filterIndex = 0;
    private int filterSubIndex = 0;

    private int sortIndex = 0;
    private SortDirectionType currentSortDirection = SortDirectionType.Descending;

    private Filter filter;

    private ItemAggregate itemAggregate;
    private ItemIterator itemIterator;
    private List<ItemInstance> scrolls;
    private Dictionary<Guid, ClickableFrame> itemFrames;

    private Inventory inventory;
    private Player player;

    public event Action<ItemInstance> OnRecipeSelected;

    private void Awake()
    {
        filter = GetComponentInChildren<Filter>(true);

        scrolls = new List<ItemInstance>();
        itemFrames = new Dictionary<Guid, ClickableFrame>();
    }

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
            sortIndex = index;
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
                    currentSortDirection = (SortDirectionType)index;
                    ApplyFilterAndSort();
                }
            });
        }
        
        // 기본 정렬 설정
        sortDropdown.value = 0; // Rarity
        if (sortToggles.Count > 0)
            sortToggles[0].isOn = true; // Descending

        inventory = Singleton.Inventory;
        player = Singleton.Player;

        scrolls = inventory.GetItemsByType(ItemType.Scroll);

        itemAggregate = new ItemAggregate();
        itemIterator = itemAggregate.CreateIterator(scrolls) as ItemIterator;

        inventory.OnInventoryItemAdded += (item) =>
        {
            var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(item.ItemID);

            if (ItemType.Scroll != (ItemType)item_selected.ItemType)
                return;

            scrolls.Add(item);
            ApplyFilterAndSort();
        };

        // Filter에서 현재 선택된 필터 정보를 받아와서 초기 설정
        int currentFilterIndex = filter.GetCurrentFilterIndex();
        int currentFilterSubIndex = filter.GetCurrentFilterSubIndex();
        SetSortOptions(currentFilterIndex, currentFilterSubIndex);

        // 인벤토리가 이미 로드된 경우 필터 적용
        if (scrolls != null && scrolls.Count > 0)
        {
            ApplyFilterAndSort();
        }

        sortDropdown.value = (int)SortType.Rarity;
        sortDropdown.onValueChanged.Invoke((int)SortType.Rarity);

        sortToggles[(int)SortDirectionType.Descending].isOn = true;
        sortToggles[(int)SortDirectionType.Descending].onValueChanged.Invoke(true);

        InitFrameList();
    }

    public async void AddItemFrame(ItemInstance _item)
    {
        GameObject obj = Instantiate(scrollFrame, itemGroup);
        CreationItemFrame frame = obj.GetComponent<CreationItemFrame>();
        await frame.SetFrameData(_item);
        frame.GetComponent<Toggle>().group = itemGroup.GetComponent<ToggleGroup>();
        itemFrames.Add(_item.InventoryID, frame);

        frame.OnFrameSelected += (item) =>
        {
            OnRecipeSelected?.Invoke(item);
        };

        ApplyFilterAndSort();
    }

    public void InitFrameList()
    {
        if (scrollFrame == null) Debug.LogError("[Inventory] scrollFrame is null. Resources 경로를 확인하세요.");
        if (itemGroup == null) Debug.LogError("[Inventory] itemGroup 참조 누락되었습니다.");

        var children = itemGroup.GetComponentsInChildren<ClickableFrame>();

        int count = children.Length;

        for (int i = count - 1; i >= 0; i--)
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

        itemScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (Mathf.Ceil(scrolls.Count * 0.25f) * 130 - 20));
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

        switch ((CreationScrollType)_index)
        {
            default:
            case CreationScrollType.None:
            case CreationScrollType.Weapon:
                {
                    sortDropdown.AddOptions(new List<string>
                    {
                        localeTable.Get((int)SortScrollType.Rarity + 200, locale),
                        localeTable.Get((int)SortScrollType.DamageMin + 200, locale),
                        localeTable.Get((int)SortScrollType.DamageMax + 200, locale),
                        localeTable.Get((int)SortScrollType.DefenseMin + 200, locale),
                        localeTable.Get((int)SortScrollType.DefenseMax + 200, locale)
                    });
                }
                break;
            case CreationScrollType.Skill:
                {
                    sortDropdown.AddOptions(new List<string>
                    {
                        localeTable.Get((int)SortScrollType.Rarity + 200, locale)
                    });
                }
                break;
        }
    }

    private void ApplyFilterAndSort()
    {
        List<ItemInstance> filteredScrolls = FilterItems(scrolls, filterIndex, filterSubIndex);
        List<ItemInstance> sortedScrolls = SortItems(filteredScrolls);

        UpdateUI(sortedScrolls);
    }

    private List<ItemInstance> FilterItems(List<ItemInstance> _scrolls, int _filterIndex, int _filterSubIndex)
    {
        if (_scrolls == null) return new List<ItemInstance>();

        var tableDataManager = Singleton.Get<TableDataManager>();
        var result = new List<ItemInstance>();

        foreach (var scroll in _scrolls)
        {
            var scrollData = tableDataManager.Table.Scroll.Get(scroll.ItemID);
            if (scrollData == null)
                continue;

            var itemData = tableDataManager.Table.Item.Get(scrollData.CreationItemID);
            if (itemData == null)
                continue;

            var scrollItemType = (ItemType)itemData.ItemType;

            switch ((CreationScrollType)_filterIndex)
            {
                case CreationScrollType.Weapon:
                    if (scrollItemType == ItemType.Weapon)
                    {
                        if ((WeaponFilterType)_filterSubIndex == WeaponFilterType.All)
                        {
                            result.Add(scroll);
                        }
                        else
                        {
                            var weaponData = tableDataManager.Table.Weapon.Get(scrollData.CreationItemID);
                            if (weaponData != null && (WeaponType)weaponData.WeaponType == (WeaponType)(_filterSubIndex))
                            {
                                result.Add(scroll);
                            }
                        }
                    }
                    break;
                case CreationScrollType.Skill:
                    if (scrollItemType == ItemType.Skill)
                        result.Add(scroll);
                    break;
                case CreationScrollType.None:
                    result = _scrolls.ToList();
                    return result;
            }
        }

        return result;
    }
    
    private List<ItemInstance> SortItems(List<ItemInstance> scrolls)
    {
        if (scrolls == null || scrolls.Count == 0) return new List<ItemInstance>();

        var tableDataManager = Singleton.Get<TableDataManager>();
        var sortedList = scrolls.ToList();

        switch ((SortScrollType)sortIndex)
        {
            case SortScrollType.Rarity:
                sortedList.Sort((a, b) =>
                {
                    var itemA = tableDataManager.Table.Item.Get(a.ItemID);
                    var itemB = tableDataManager.Table.Item.Get(b.ItemID);

                    int comparison = itemA.Rarity.CompareTo(itemB.Rarity);
                    return currentSortDirection == SortDirectionType.Ascending ? comparison : -comparison;
                });
                break;
                
            case SortScrollType.DamageMin:
            case SortScrollType.DamageMax:
            case SortScrollType.DefenseMin:
            case SortScrollType.DefenseMax:
                sortedList.Sort((a, b) =>
                {
                    var scrollA = tableDataManager.Table.Scroll.Get(a.ItemID);
                    var scrollB = tableDataManager.Table.Scroll.Get(b.ItemID);

                    var itemA = tableDataManager.Table.Item.Get(scrollA.CreationItemID);
                    var itemB = tableDataManager.Table.Item.Get(scrollB.CreationItemID);

                    bool isWeaponA = (ItemType)itemA.ItemType == ItemType.Weapon;
                    bool isWeaponB = (ItemType)itemB.ItemType == ItemType.Weapon;

                    if (!isWeaponA && !isWeaponB)
                        return 0;

                    if (!isWeaponA)
                        return 1;

                    if (!isWeaponB)
                        return -1;

                    int compareA = tableDataManager.Table.Weapon.Get(scrollA.CreationItemID).Damage_Min;
                    int compareB = tableDataManager.Table.Weapon.Get(scrollB.CreationItemID).Damage_Min;

                    if (sortIndex == (int)SortScrollType.DamageMax)
                    {
                        compareA = tableDataManager.Table.Weapon.Get(scrollA.CreationItemID).Damage_Max;
                        compareB = tableDataManager.Table.Weapon.Get(scrollB.CreationItemID).Damage_Max;
                    }
                    else if (sortIndex == (int)SortScrollType.DefenseMin)
                    {
                        compareA = tableDataManager.Table.Weapon.Get(scrollA.CreationItemID).Defense_Min;
                        compareB = tableDataManager.Table.Weapon.Get(scrollB.CreationItemID).Defense_Min;
                    } else if (sortIndex == (int)SortScrollType.DefenseMax)
                    {
                        compareA = tableDataManager.Table.Weapon.Get(scrollA.CreationItemID).Defense_Max;
                        compareB = tableDataManager.Table.Weapon.Get(scrollB.CreationItemID).Defense_Max;
                    }

                    int comparison = compareA.CompareTo(compareB);
                    return currentSortDirection == SortDirectionType.Ascending ? comparison : -comparison;
                });
                break;
        }
        return sortedList;
    }

    private void UpdateUI(List<ItemInstance> _sortedScrolls)
    {
        foreach (var frame in itemFrames.Values)
        {
            frame.gameObject.SetActive(false);
        }

        int count = _sortedScrolls.Count;
        for (int i = 0; i < count; i++)
        {
            ItemInstance item = _sortedScrolls[i];
            if (itemFrames.TryGetValue(item.InventoryID, out ClickableFrame frame))
            {
                frame.gameObject.SetActive(true);
                frame.transform.SetSiblingIndex(i);
            }
        }
    }
}
