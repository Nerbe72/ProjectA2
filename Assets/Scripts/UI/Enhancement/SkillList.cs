using GameStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillList : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private GameObject hoverClickableFramePrefab;
    [SerializeField] private Transform itemGroup;
    [SerializeField] private ScrollRect itemScroll;

    private ItemAggregate itemAggregate;
    private ItemIterator itemIterator;
    private List<SkillItemInstance> skills;
    private Dictionary<Guid, HoverClickableFrame> itemFrames;

    private Inventory inventory;

    public event Action<SkillItemInstance> OnSkillSelected;

    private Color originalRarityColor = Color.white;
    private Color selectedFrameColor = Color.green;

    private void Awake()
    {
        skills = new List<SkillItemInstance>();
        itemFrames = new Dictionary<Guid, HoverClickableFrame>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        inventory = Singleton.Inventory;

        skills = inventory.GetItemsByType(ItemType.Skill).Cast<SkillItemInstance>().ToList();

        itemAggregate = new ItemAggregate();
        itemIterator = itemAggregate.CreateIterator(skills.Cast<ItemInstance>().ToList()) as ItemIterator;

        inventory.OnInventoryItemAdded += (item) =>
        {
            if (item is SkillItemInstance skill)
            {
                skills.Add(skill);
                RefreshSkillList();
            }
        };

        skills.Sort((a, b) => {
            var tableDataManager = Singleton.Get<TableDataManager>();
            var itemA = tableDataManager.Table.Item.Get(a.ItemID);
            var itemB = tableDataManager.Table.Item.Get(b.ItemID);

            return itemB.Rarity.CompareTo(itemA.Rarity);
        });

        InitFrameList();
    }

    public async void AddItemFrame(SkillItemInstance _skill)
    {
        GameObject obj = Instantiate(hoverClickableFramePrefab, itemGroup);
        HoverClickableFrame frame = obj.GetComponent<HoverClickableFrame>();
        await frame.SetFrameData(_skill);
        
        Toggle toggle = frame.GetComponent<Toggle>();
        toggle.group = itemGroup.GetComponent<ToggleGroup>();
        
        toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(toggle, isOn));
        
        itemFrames.Add(_skill.InventoryID, frame);

        frame.OnFrameSelected += (item) =>
        {
            OnSkillSelected?.Invoke(item as SkillItemInstance);
        };
    }

    public void InitFrameList()
    {
        if (hoverClickableFramePrefab == null) Debug.LogError("[SkillList] hoverClickableFramePrefab is null. Resources 경로를 확인하세요.");
        if (itemGroup == null) Debug.LogError("[SkillList] itemGroup 참조 누락되었습니다.");

        var children = itemGroup.GetComponentsInChildren<HoverClickableFrame>();

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
            if (item is SkillItemInstance skill)
            {
                AddItemFrame(skill);
            }
        }

        itemScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (Mathf.Ceil(skills.Count * 0.25f) * 130 - 20));
    }

    private void OnToggleValueChanged(Toggle _toggle, bool _isOn)
    {
        if (_isOn)
        {
            ResetAllFrameColors();
            
            SetFrameColor(_toggle, selectedFrameColor);
            
            var frame = _toggle.GetComponent<HoverClickableFrame>();
            if (frame != null)
            {
                var skillInstance = frame.GetItemInstance() as SkillItemInstance;
                if (skillInstance != null)
                {
                    OnSkillSelected?.Invoke(skillInstance);
                }
            }
        }
    }

    private void SetFrameColor(Toggle _toggle, Color _color)
    {
        if (_toggle == null) return;
        
        var frame = _toggle.GetComponent<HoverClickableFrame>();
        if (frame != null)
        {
            var itemInstance = frame.GetItemInstance();
            if (itemInstance != null)
            {
                var itemData = Singleton.Get<TableDataManager>().Table.Item.Get(itemInstance.ItemID);
                if (itemData != null)
                {
                    var frameBase = _toggle.GetComponent<FrameBase>();
                    if (frameBase != null)
                    {
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

    private void ResetAllFrameColors()
    {
        var frames = itemFrames.Values.ToList();
        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];
            if (frame == null) continue;
            
            var itemInstance = frame.GetItemInstance();
            if (itemInstance == null) continue;
            
            var itemData = Singleton.Get<TableDataManager>().Table.Item.Get(itemInstance.ItemID);
            if (itemData == null) continue;
            
            var frameBase = frame.GetComponent<FrameBase>();
            if (frameBase == null) continue;
            
            var rarityImageField = typeof(FrameBase).GetField("itemRarityImage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rarityImageField == null) continue;
            
            var rarityImage = rarityImageField.GetValue(frameBase) as Image;
            if (rarityImage == null) continue;
            
            rarityImage.color = ItemColor.GetColor((Rarity)itemData.Rarity);
        }
    }

    public void RefreshSkillList()
    {
        skills = inventory.GetItemsByType(ItemType.Skill).Cast<SkillItemInstance>().ToList();
        
        itemAggregate = new ItemAggregate();
        itemIterator = itemAggregate.CreateIterator(skills.Cast<ItemInstance>().ToList()) as ItemIterator;
        
        InitFrameList();
    }

    public void SetShown(bool _isShown)
    {
        if (animator != null)
        {
            animator.SetBool("Show", _isShown);
        }
    }
}
