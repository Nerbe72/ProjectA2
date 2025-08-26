using GameStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnhancementItemList : MonoBehaviour
{
    [SerializeField] private GameObject itemFramePrefab;
    [SerializeField] private Transform itemGroup;
    [SerializeField] private ScrollRect itemScroll;

    [SerializeField] private TMP_Dropdown sortDropdown;
    [SerializeField] private ToggleGroup sortGroup;

    private List<Toggle> sortToggles;

    private int filterSubIndex = 0; // WeaponFilterType
    private int sortIndex = 0;
    private SortDirectionType currentSortDirection = SortDirectionType.Descending;

    private Filter filter;

    private ItemAggregate itemAggregate;
    private ItemIterator itemIterator;
    private List<WeaponItemInstance> weapons;
    private Dictionary<Guid, ClickableFrame> itemFrames;

    private Inventory inventory;

    public event Action<WeaponItemInstance> OnWeaponSelected;

    // 녹색 프레임 관련 변수
    private Color originalRarityColor = Color.white;
    private Color selectedFrameColor = Color.green;

    private void Awake()
    {
        filter = GetComponentInChildren<Filter>(true);
        weapons = new List<WeaponItemInstance>();
        itemFrames = new Dictionary<Guid, ClickableFrame>();
    }

    private void Start()
    {
        filter.OnFilterSelected += (index, subindex) =>
        {
            filterSubIndex = subindex;
            SetSortOptions();
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

        // 무기 아이템만 가져오기 (RecipeList와 동일한 패턴)
        weapons = inventory.GetItemsByType(ItemType.Weapon).Cast<WeaponItemInstance>().ToList();

        itemAggregate = new ItemAggregate();
        itemIterator = itemAggregate.CreateIterator(weapons.Cast<ItemInstance>().ToList()) as ItemIterator;

        inventory.OnInventoryItemAdded += (item) =>
        {
            if (item is WeaponItemInstance weapon)
            {
                weapons.Add(weapon);
                ApplyFilterAndSort();
            }
        };

        // Filter에서 현재 선택된 필터 정보를 받아와서 초기 설정
        int currentFilterSubIndex = filter.GetCurrentFilterSubIndex();
        SetSortOptions();

        // 인벤토리가 이미 로드된 경우 필터 적용
        if (weapons != null && weapons.Count > 0)
        {
            ApplyFilterAndSort();
        }

        sortDropdown.value = (int)SortType.Rarity;
        sortDropdown.onValueChanged.Invoke((int)SortType.Rarity);

        sortToggles[(int)SortDirectionType.Descending].isOn = true;
        sortToggles[(int)SortDirectionType.Descending].onValueChanged.Invoke(true);

        InitFrameList();
    }

    // Toggle 상태 변화 처리
    private void OnToggleValueChanged(Toggle _toggle, bool _isOn)
    {
        if (_isOn)
        {
            // 모든 프레임의 색상을 원래대로 복원
            ResetAllFrameColors();
            
            // 선택된 프레임을 녹색으로 변경
            SetFrameColor(_toggle, selectedFrameColor);
            
            // 선택된 무기를 EnhancementDialogWindow에 전달
            var frame = _toggle.GetComponent<ClickableFrame>();
            if (frame != null)
            {
                var weaponInstance = frame.GetItemInstance() as WeaponItemInstance;
                if (weaponInstance != null)
                {
                    OnWeaponSelected?.Invoke(weaponInstance);
                }
            }
        }
    }

    // 프레임 색상 설정
    private void SetFrameColor(Toggle _toggle, Color _color)
    {
        if (_toggle == null) return;
        
        var frame = _toggle.GetComponent<ClickableFrame>();
        if (frame != null)
        {
            var itemInstance = frame.GetItemInstance();
            if (itemInstance != null)
            {
                var itemData = Singleton.Get<TableDataManager>().Table.Item.Get(itemInstance.ItemID);
                if (itemData != null)
                {
                    // itemRarityImage 찾기 (FrameBase에서 정의된 itemRarityImage)
                    var frameBase = _toggle.GetComponent<FrameBase>();
                    if (frameBase != null)
                    {
                        // FrameBase의 itemRarityImage에 접근하기 위해 리플렉션 사용
                        var rarityImageField = typeof(FrameBase).GetField("itemRarityImage", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (rarityImageField != null)
                        {
                            var rarityImage = rarityImageField.GetValue(frameBase) as Image;
                            if (rarityImage != null)
                            {
                                rarityImage.color = _color;
                            }
                        }
                    }
                }
            }
        }
    }

    // 모든 프레임 색상 초기화
    private void ResetAllFrameColors()
    {
        foreach (var frame in itemFrames.Values)
        {
            if (frame != null)
            {
                var itemInstance = frame.GetItemInstance();
                if (itemInstance != null)
                {
                    var itemData = Singleton.Get<TableDataManager>().Table.Item.Get(itemInstance.ItemID);
                    if (itemData != null)
                    {
                        // FrameBase의 itemRarityImage에 접근하기 위해 리플렉션 사용
                        var frameBase = frame.GetComponent<FrameBase>();
                        if (frameBase != null)
                        {
                            var rarityImageField = typeof(FrameBase).GetField("itemRarityImage", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (rarityImageField != null)
                            {
                                var rarityImage = rarityImageField.GetValue(frameBase) as Image;
                                if (rarityImage != null)
                                {
                                    rarityImage.color = ItemColor.GetColor((Rarity)itemData.Rarity);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public async void AddItemFrame(WeaponItemInstance _weapon)
    {
        GameObject obj = Instantiate(itemFramePrefab, itemGroup);
        ItemFrame frame = obj.GetComponent<ItemFrame>();
        await frame.SetFrameData(_weapon);
        
        Toggle toggle = frame.GetComponent<Toggle>();
        toggle.group = itemGroup.GetComponent<ToggleGroup>();
        
        toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(toggle, isOn));
        
        itemFrames.Add(_weapon.InventoryID, frame);

        frame.OnFrameSelected += (item) =>
        {
            OnWeaponSelected?.Invoke(item as WeaponItemInstance);
        };

        ApplyFilterAndSort();
    }

    public void InitFrameList()
    {
        if (itemFramePrefab == null) Debug.LogError("[EnhancementItemList] itemFramePrefab is null. Resources 경로를 확인하세요.");
        if (itemGroup == null) Debug.LogError("[EnhancementItemList] itemGroup 참조 누락되었습니다.");

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
            if (item is WeaponItemInstance weapon)
            {
                AddItemFrame(weapon);
            }
        }

        itemScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (Mathf.Ceil(weapons.Count * 0.25f) * 130 - 20));
    }

    private void SetSortOptions()
    {
        var localeTable = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;

        if (localeTable == null)
            return;

        sortDropdown.ClearOptions();

        // 무기 정렬 옵션
        sortDropdown.AddOptions(new List<string>
        {
            localeTable.Get((int)SortType.Rarity + 200, locale),
            localeTable.Get((int)SortType.Damage + 200, locale),
            localeTable.Get((int)SortType.Defense + 200, locale)
        });
    }

    private void ApplyFilterAndSort()
    {
        List<WeaponItemInstance> filteredWeapons = FilterWeapons(weapons, filterSubIndex);
        List<WeaponItemInstance> sortedWeapons = SortWeapons(filteredWeapons);

        UpdateUI(sortedWeapons);
    }

    private List<WeaponItemInstance> FilterWeapons(List<WeaponItemInstance> _weapons, int _filterSubIndex)
    {
        if (_weapons == null) return new List<WeaponItemInstance>();

        var tableDataManager = Singleton.Get<TableDataManager>();
        var result = new List<WeaponItemInstance>();

        foreach (var weapon in _weapons)
        {
            var weaponData = tableDataManager.Table.Weapon.Get(weapon.ItemID);
            if (weaponData == null)
                continue;

            if ((WeaponFilterType)_filterSubIndex == WeaponFilterType.All)
            {
                result.Add(weapon);
            }
            else if (IsWeaponMatched((WeaponType)weaponData.WeaponType, (WeaponFilterType)_filterSubIndex))
            {
                result.Add(weapon);
            }
        }

        return result;
    }

    private bool IsWeaponMatched(WeaponType _weaponType, WeaponFilterType _filterType)
    {
        switch (_filterType)
        {
            case WeaponFilterType.All:
                return true;
            case WeaponFilterType.Melee:
                return _weaponType == WeaponType.Melee;
            case WeaponFilterType.Bow:
                return _weaponType == WeaponType.Bow;
            case WeaponFilterType.Magic:
                return _weaponType == WeaponType.Magic;
            default:
                return false;
        }
    }
    
    private List<WeaponItemInstance> SortWeapons(List<WeaponItemInstance> _weapons)
    {
        if (_weapons == null || _weapons.Count == 0) return new List<WeaponItemInstance>();

        var tableDataManager = Singleton.Get<TableDataManager>();
        var sortedList = _weapons.ToList();

        switch ((SortType)sortIndex)
        {
            case SortType.Rarity:
                sortedList.Sort((a, b) =>
                {
                    var itemA = tableDataManager.Table.Item.Get(a.ItemID);
                    var itemB = tableDataManager.Table.Item.Get(b.ItemID);

                    int comparison = itemA.Rarity.CompareTo(itemB.Rarity);
                    return currentSortDirection == SortDirectionType.Ascending ? comparison : -comparison;
                });
                break;
                
            case SortType.Damage:
                sortedList.Sort((a, b) =>
                {
                    var weaponA = tableDataManager.Table.Weapon.Get(a.ItemID);
                    var weaponB = tableDataManager.Table.Weapon.Get(b.ItemID);

                    int compareA = weaponA.Damage_Min;
                    int compareB = weaponB.Damage_Min;

                    int comparison = compareA.CompareTo(compareB);
                    return currentSortDirection == SortDirectionType.Ascending ? comparison : -comparison;
                });
                break;
                
            case SortType.Defense:
                sortedList.Sort((a, b) =>
                {
                    var weaponA = tableDataManager.Table.Weapon.Get(a.ItemID);
                    var weaponB = tableDataManager.Table.Weapon.Get(b.ItemID);

                    int compareA = weaponA.Defense_Min;
                    int compareB = weaponB.Defense_Min;

                    int comparison = compareA.CompareTo(compareB);
                    return currentSortDirection == SortDirectionType.Ascending ? comparison : -comparison;
                });
                break;
        }
        return sortedList;
    }

    private void UpdateUI(List<WeaponItemInstance> _sortedWeapons)
    {
        foreach (var frame in itemFrames.Values)
        {
            frame.gameObject.SetActive(false);
        }

        int count = _sortedWeapons.Count;
        for (int i = 0; i < count; i++)
        {
            WeaponItemInstance weapon = _sortedWeapons[i];
            if (itemFrames.TryGetValue(weapon.InventoryID, out ClickableFrame frame))
            {
                frame.gameObject.SetActive(true);
                frame.transform.SetSiblingIndex(i);
            }
        }
    }

    public void RefreshWeaponList()
    {
        // RecipeList와 동일한 패턴으로 무기 리스트 새로고침
        weapons = inventory.GetItemsByType(ItemType.Weapon).Cast<WeaponItemInstance>().ToList();
        
        itemAggregate = new ItemAggregate();
        itemIterator = itemAggregate.CreateIterator(weapons.Cast<ItemInstance>().ToList()) as ItemIterator;
        
        InitFrameList();
        ApplyFilterAndSort();
    }

    public void RefreshWeaponData(WeaponItemInstance _weapon)
    {
        if (_weapon == null) return;

        if (itemFrames.TryGetValue(_weapon.InventoryID, out ClickableFrame frame))
        {
            frame.SetFrameData(_weapon);
        }
    }
}
