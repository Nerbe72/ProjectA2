using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using GameStuff;

public class EnchantSubWindow : SubWindow
{
    public static event Action<WeaponItemInstance> OnEnchantSuccess;

    [Header("Weapon Area")]
    [SerializeField] private HoverableFrame weaponFrame;
    [SerializeField] private Slots totalSlot;

    [Header("Skill Area")]
    [SerializeField] private HoverableFrame selectedSkillFrame;
    [SerializeField] private TMP_Text selectedSkillName;
    [SerializeField] private Slots selectedSkillSlots;

    [Header("Enchanted Area")]
    [SerializeField] private ScrollRect skillListScrollView;
    [SerializeField] private GameObject enchantedSkillTemplate;

    [Header("버튼")]
    [SerializeField] private Button enchantButton;

    private WeaponItemInstance selectedWeapon;
    private WeaponEnhancementAdapter weaponAdapter;
    private int selectedSkillID;

    protected override void Awake()
    {
        base.Awake();
        
        if (enchantButton != null)
        {
            enchantButton.onClick.RemoveListener(OnEnchantButtonClicked);
            enchantButton.onClick.AddListener(OnEnchantButtonClicked);
        }
    }

    public void SetWeapon(WeaponItemInstance _weapon)
    {
        selectedWeapon = _weapon;
        selectedSkillID = 0;
        
        if (selectedWeapon == null)
            return;

        weaponAdapter = Singleton.Inventory.GetWeaponAdapter(selectedWeapon);
        
        if (weaponAdapter == null)
        {
            Debug.LogError($"Weapon adapter not found for weapon ID: {selectedWeapon.ItemID}");
            return;
        }

        UpdateUI();
    }

    public void SelectSkill(int _skillID)
    {
        selectedSkillID = _skillID;
        UpdateSelectedSkillInfo();
        UpdateEnchantButton();
    }

    private void UpdateUI()
    {
        UpdateWeaponInfo();
        UpdateEnchantButton();
    }

    private void UpdateWeaponInfo()
    {
        if (selectedWeapon == null) return;

        var table = Singleton.Get<TableDataManager>().Table;
        var weapon_selected = table.Weapon.Get(selectedWeapon.ItemID);
        var item_selected = table.Item.Get(selectedWeapon.ItemID);
        var locale = table.Locale;

        // 무기 프레임 설정
        if (weaponFrame != null)
        {
            weaponFrame.SetItemID(selectedWeapon.ItemID);
        }

        // 총 슬롯 설정 (전체 슬롯과 사용중인 슬롯)
        if (totalSlot != null)
        {
            int totalSlots = weaponAdapter.GetAvailableSkillSlots() + weaponAdapter.GetUsedSkillSlots();
            int usedSlots = weaponAdapter.GetUsedSkillSlots();
            totalSlot.SetSlot(totalSlots, usedSlots);
        }

        UpdateEnchantedSkillList();
        ClearSelectedSkill();
    }

    private void UpdateEnchantedSkillList()
    {
        ClearSkillList();

        if (selectedWeapon == null || weaponAdapter == null) return;

        var enchantedSkills = weaponAdapter.GetEnchantedSkills();
        
        foreach (var skillID in enchantedSkills)
        {
            CreateSkillListItem(skillID);
        }

        if (skillListScrollView?.content != null && enchantedSkillTemplate != null)
        {
            var templateRect = enchantedSkillTemplate.GetComponent<RectTransform>();
            if (templateRect != null)
            {
                float templateHeight = templateRect.rect.height;
                float contentHeight = templateHeight * enchantedSkills.Count;
                skillListScrollView.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            }
        }
    }

    private void ClearSkillList()
    {
        if (skillListScrollView?.content == null) return;
        
        foreach (Transform child in skillListScrollView.content)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateSkillListItem(int _skillID)
    {
        if (skillListScrollView?.content == null || enchantedSkillTemplate == null) return;
        
        var item = Instantiate(enchantedSkillTemplate, skillListScrollView.content);
        var skillItem = item.GetComponent<EnchantedSkillTemplate>();
        
        if (skillItem != null)
        {
            skillItem.SetData(_skillID);
        }
    }

    private void ClearSelectedSkill()
    {
        selectedSkillFrame.SetItemID(0);
        selectedSkillName.text = "";
        selectedSkillSlots.SetSlot(0, 0);
    }

    private void UpdateSelectedSkillInfo()
    {
        if (selectedSkillID == 0)
        {
            ClearSelectedSkill();
            return;
        }

        var abilityManager = Singleton.Get<AbilityManager>();

        selectedSkillFrame.SetItemID(selectedSkillID);
        selectedSkillName.text = abilityManager.GetName(selectedSkillID);

        int requiredSlots = abilityManager.GetSlotCount(selectedSkillID);
        selectedSkillSlots.SetSlot(requiredSlots, requiredSlots);
    }

    private void UpdateEnchantButton()
    {
        if (enchantButton == null) return;

        EnchantResult canEnchant = CanEnchant();
        enchantButton.interactable = (canEnchant == EnchantResult.Success);
        enchantButton.GetComponentInChildren<TMP_Text>().text = "귀속";
    }

    private EnchantResult CanEnchant()
    {
        if (selectedWeapon == null || selectedSkillID == 0)
            return EnchantResult.SkillNotFound;

        if (weaponAdapter != null)
        {
            int availableSlots = weaponAdapter.GetAvailableSkillSlots();
            int requiredSlots = Singleton.Get<AbilityManager>().GetSlotCount(selectedSkillID);
            
            if (availableSlots < requiredSlots)
                return EnchantResult.NoEnoughSlot;
        }

        return EnchantResult.Success;
    }

    private void OnEnchantButtonClicked()
    {
        EnchantResult result = CanEnchant();
        
        if (result != EnchantResult.Success)
        {
            string message = "";
            switch (result)
            {
                case EnchantResult.SkillNotFound:
                    message = "스킬이 선택되지 않았습니다.";
                    break;
                case EnchantResult.NoEnoughSlot:
                    message = "슬롯이 부족합니다.";
                    break;
                default:
                    message = "귀속할 수 없습니다.";
                    break;
            }
            Singleton.Get<Alert>().Show(message, Color.red);
            return;
        }

        ExecuteEnchant();
    }

    private void ExecuteEnchant()
    {
        EnchantResult result = weaponAdapter.TryEnchantSkill(selectedSkillID);
        
        if (result == EnchantResult.Success)
        {
            UpdateUI();
            OnEnchantSuccess?.Invoke(selectedWeapon);
        }
    }
}
