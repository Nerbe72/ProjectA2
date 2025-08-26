using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameStuff;
using System;

public enum DataFieldType
{
    GrowthSTR,
    GrowthDEX,
    GrowthINT,
    FinalDamage,
    FinalDefense,
    TotalEnhancement
}

public class EnhanceSubWindow : SubWindow
{
    public static event Action<WeaponItemInstance> OnEnhancementSuccess;

    [Header("UI References")]
    [SerializeField] private HoverableFrame itemFrame;
    [SerializeField] private List<DataField> dataFields;

    [SerializeField] private TMP_Text price;
    
    [SerializeField] private Button enhanceButton;

    
    private WeaponItemInstance selectedWeapon;
    private WeaponEnhancementAdapter weaponAdapter;
    private TableEnhancement enhancementTable;

    protected override void Awake()
    {
        base.Awake();
        OnEnhancementSuccess += OnWeaponEnhanced;
        
        Singleton.Get<GameManager>().OnLocaleChanged += UpdateLocale;
        UpdateLocale();
    }

    private void OnDestroy()
    {
        OnEnhancementSuccess -= OnWeaponEnhanced;
    }

    private void UpdateLocale()
    {
        var table = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;

        if (dataFields.Count > 0 && dataFields[0] != null)
        {
            UpdateNameField(dataFields[0]);
        }
    }

    private void OnWeaponEnhanced(WeaponItemInstance _weapon)
    {
        if (_weapon != null && selectedWeapon != null && _weapon.InventoryID == selectedWeapon.InventoryID)
        {
            SetWeapon(_weapon);
        }
    }

    public override void Swap(SubWindow _from)
    {
        base.Swap(_from);
        InitializeEnhancement();
    }

    private void InitializeEnhancement()
    {
        enhancementTable = Singleton.Get<TableDataManager>().Table.Enhancement;
        
        if (enhanceButton != null)
        {
            enhanceButton.onClick.RemoveAllListeners();
            enhanceButton.onClick.AddListener(OnEnhanceButtonClicked);
        }
    }

    public void SetWeapon(WeaponItemInstance _weapon)
    {
        selectedWeapon = _weapon;
        
        if (selectedWeapon == null)
        {
            ClearUI();
            return;
        }

        weaponAdapter = Singleton.Inventory.GetWeaponAdapter(selectedWeapon);
        
        if (weaponAdapter == null)
        {
            Debug.LogError($"Weapon adapter not found for weapon ID: {selectedWeapon.ItemID}");
            return;
        }

        UpdateEnhancementInfo();
        UpdateStatFields();
        UpdateItemFrame();
    }

    private void ClearUI()
    {
        foreach (var field in dataFields)
        {
            if (field != null && field.datas != null)
            {
                foreach (var data in field.datas)
                {
                    if (data != null)
                        data.text = "";
                }
            }
        }

        if (price != null)
            price.text = "";

        if (enhanceButton != null)
            enhanceButton.interactable = false;
    }

    private void UpdateEnhancementInfo()
    {
        if (weaponAdapter == null) return;

        int currentLevel = weaponAdapter.EnhancedLevel;
        int nextLevel = currentLevel + 1;

        if (price != null)
        {
            int enhancementCost = GetEnhancementCost(nextLevel);
            price.text = enhancementCost.ToString();
        }

        UpdateEnhanceButton();
    }

    private void UpdateItemFrame()
    {
        if (selectedWeapon == null) return;

        if (itemFrame != null)
        {
            itemFrame.SetItemID(selectedWeapon.ItemID);
            itemFrame.SetItemDetail("");
        }
    }

    private void UpdateStatFields()
    {
        if (weaponAdapter == null || enhancementTable == null) return;

        int currentLevel = weaponAdapter.EnhancedLevel;
        int nextLevel = currentLevel + 1;

        if (dataFields.Count >= 3)
        {
            UpdateNameField(dataFields[0]);
            UpdateBeforeField(dataFields[1], currentLevel);
            UpdateAfterField(dataFields[2], nextLevel);
        }
    }

    private void UpdateNameField(DataField _nameField)
    {
        if (_nameField == null || _nameField.datas == null) return;

        var table = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;

        _nameField.datas[0].text = table.Get(10000052, locale);
        _nameField.datas[1].text = table.Get(10000053, locale);
        _nameField.datas[2].text = table.Get(10000054, locale);
        _nameField.datas[3].text = table.Get(10000055, locale);
        _nameField.datas[4].text = table.Get(10000056, locale);
        _nameField.datas[5].text = table.Get(10000057, locale);
    }

    private void UpdateBeforeField(DataField _beforeField, int _currentLevel)
    {
        if (_beforeField == null || _beforeField.datas == null) return;

        _beforeField.datas[0].text = GetGrowthSTR(_currentLevel).ToString();
        _beforeField.datas[1].text = GetGrowthDEX(_currentLevel).ToString();
        _beforeField.datas[2].text = GetGrowthINT(_currentLevel).ToString();
        _beforeField.datas[3].text = Singleton.Player.GetCalculatedDamageWithGrowth(selectedWeapon, _currentLevel).ToString();
        _beforeField.datas[4].text = Singleton.Player.GetCalculatedDefenseWithGrowth(selectedWeapon, _currentLevel).ToString();
        _beforeField.datas[5].text = _currentLevel.ToString();
    }

    private void UpdateAfterField(DataField _afterField, int _nextLevel)
    {
        if (_afterField == null || _afterField.datas == null) return;

        int currentLevel = weaponAdapter.EnhancedLevel;
        int maxLevel = weaponAdapter.MaxEnhancementLevel;

        if (currentLevel >= maxLevel)
        {
            _afterField.datas[0].text = GetGrowthSTR(currentLevel).ToString();
            _afterField.datas[1].text = GetGrowthDEX(currentLevel).ToString();
            _afterField.datas[2].text = GetGrowthINT(currentLevel).ToString();
            _afterField.datas[3].text = Singleton.Player.GetCalculatedDamageWithGrowth(selectedWeapon, currentLevel).ToString();
            _afterField.datas[4].text = Singleton.Player.GetCalculatedDefenseWithGrowth(selectedWeapon, currentLevel).ToString();
            _afterField.datas[5].text = currentLevel.ToString();
        }
        else
        {
            _afterField.datas[0].text = GetGrowthSTR(_nextLevel).ToString();
            _afterField.datas[1].text = GetGrowthDEX(_nextLevel).ToString();
            _afterField.datas[2].text = GetGrowthINT(_nextLevel).ToString();
            _afterField.datas[3].text = Singleton.Player.GetCalculatedDamageWithGrowth(selectedWeapon, _nextLevel).ToString();
            _afterField.datas[4].text = Singleton.Player.GetCalculatedDefenseWithGrowth(selectedWeapon, _nextLevel).ToString();
            _afterField.datas[5].text = _nextLevel.ToString();
        }
    }

    private float GetGrowthSTR(int _enhancementLevel)
    {
        if (selectedWeapon == null) return 0f;

        var weaponData = Singleton.Get<TableDataManager>().Table.Weapon.Get(selectedWeapon.ItemID);
        if (weaponData == null) return 0f;

        var enhancementInfo = enhancementTable.Get(selectedWeapon.ItemID, _enhancementLevel);
        return weaponData.DamageGrowth_STR + (enhancementInfo?.AddGrowthSTR ?? 0f);
    }

    private float GetGrowthDEX(int _enhancementLevel)
    {
        if (selectedWeapon == null) return 0f;

        var weaponData = Singleton.Get<TableDataManager>().Table.Weapon.Get(selectedWeapon.ItemID);
        if (weaponData == null) return 0f;

        var enhancementInfo = enhancementTable.Get(selectedWeapon.ItemID, _enhancementLevel);
        return weaponData.DamageGrowth_DEX + (enhancementInfo?.AddGrowthDEX ?? 0f);
    }

    private float GetGrowthINT(int _enhancementLevel)
    {
        if (selectedWeapon == null) return 0f;

        var weaponData = Singleton.Get<TableDataManager>().Table.Weapon.Get(selectedWeapon.ItemID);
        if (weaponData == null) return 0f;

        var enhancementInfo = enhancementTable.Get(selectedWeapon.ItemID, _enhancementLevel);
        return weaponData.DamageGrowth_INT + (enhancementInfo?.AddGrowthINT ?? 0f);
    }

    private void UpdateEnhanceButton()
    {
        if (enhanceButton == null) return;

        int currentLevel = weaponAdapter.EnhancedLevel;
        int nextLevel = currentLevel + 1;
        int enhancementCost = GetEnhancementCost(nextLevel);
        int playerCurrency = (int)Singleton.Inventory.GetCurrency();
        int maxLevel = weaponAdapter.MaxEnhancementLevel;

        var buttonText = enhanceButton.GetComponentInChildren<TMPro.TMP_Text>();
        if (buttonText != null)
        {
            if (currentLevel >= maxLevel)
            {
                buttonText.color = Color.red;
            }
            else if (playerCurrency >= enhancementCost)
            {
                buttonText.color = Color.white;
            }
            else
            {
                buttonText.color = Color.red;
            }
        }

        enhanceButton.interactable = true;
    }

    private void OnEnhanceButtonClicked()
    {
        if (weaponAdapter == null) return;

        int currentLevel = weaponAdapter.EnhancedLevel;
        int nextLevel = currentLevel + 1;
        int enhancementCost = GetEnhancementCost(nextLevel);
        int playerCurrency = (int)Singleton.Inventory.GetCurrency();
        int maxLevel = weaponAdapter.MaxEnhancementLevel;

        if (currentLevel >= maxLevel)
        {
            Debug.Log("이미 최대 강화입니다!");
            return;
        }

        if (playerCurrency < enhancementCost)
        {
            Debug.Log("재화가 부족합니다!");
            return;
        }

        var result = weaponAdapter.TryEnhance();
        
        switch (result)
        {
            case EnhancementResult.Success:
                Debug.Log("강화 성공!");
                OnEnhancementSuccess?.Invoke(selectedWeapon);
                Singleton.Inventory.SaveInventoryData();
                break;
            case EnhancementResult.Failure:
                Debug.Log("강화에 실패했습니다!");
                break;
            case EnhancementResult.MaxLevel:
                Debug.Log("이미 최대 강화입니다!");
                break;
        }
    }

    private int GetEnhancementCost(int _currentLevel)
    {
        return _currentLevel * 120;
    }
}
