using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using GameStuff;

public class EnhancementDialogWindow : WindowBase
{
    private ToggleGroup tabGroup;
    private Tab[] tabs;
    private List<SubWindow> subWindows;
    private SkillList skillList;
    private EnhanceSubWindow enhanceSubWindow;
    private EnchantSubWindow enchantSubWindow;
    private EnhancementItemList enhancementItemList;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        tabGroup = GetComponentInChildren<ToggleGroup>();
        subWindows = GetComponentsInChildren<SubWindow>().ToList();
        skillList = GetComponentInChildren<SkillList>();
        enhanceSubWindow = GetComponentInChildren<EnhanceSubWindow>();
        enchantSubWindow = GetComponentInChildren<EnchantSubWindow>();
        enhancementItemList = GetComponentInChildren<EnhancementItemList>();

        tabs = tabGroup.GetComponentsInChildren<Tab>();

        int count = tabs.Length;

        for (int i = 0; i < count; i++)
        {
            int index = i;
            tabs[index].OnTabSelected += SwapTab;
        }

        EnhanceSubWindow.OnEnhancementSuccess += OnEnhancementSuccess;
        EnchantSubWindow.OnEnchantSuccess += OnEnchantSuccess;
        
        enhancementItemList.OnWeaponSelected += OnWeaponSelected;
        skillList.OnSkillSelected += OnSkillSelected;

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        tabs[0].OnTabSelected?.Invoke(0);
    }

    private void OnDestroy()
    {
        EnhanceSubWindow.OnEnhancementSuccess -= OnEnhancementSuccess;
        EnchantSubWindow.OnEnchantSuccess -= OnEnchantSuccess;
        
        enhancementItemList.OnWeaponSelected -= OnWeaponSelected;
        skillList.OnSkillSelected -= OnSkillSelected;
    }

    private void OnEnhancementSuccess(WeaponItemInstance _weapon)
    {
        if (enhancementItemList != null)
            enhancementItemList.RefreshWeaponData(_weapon);
    }

    private void OnEnchantSuccess(WeaponItemInstance _weapon)
    {
        if (enhancementItemList != null)
            enhancementItemList.RefreshWeaponData(_weapon);
        
        if (skillList != null)
            skillList.RefreshSkillList();
    }

    private void SwapTab(int _index)
    {
        int fromIndex = subWindows.FindIndex(x => x.IsSelected);

        if (fromIndex == -1 || fromIndex == _index)
        {
            subWindows[_index].Swap(null);
            return;
        }

        subWindows[_index].Swap(subWindows[fromIndex]);
        
        skillList.SetShown(_index == 1);
    }

    private void OnWeaponSelected(WeaponItemInstance _weapon)
    {
        if (enhanceSubWindow != null)
            enhanceSubWindow.SetWeapon(_weapon);
        
        if (enchantSubWindow != null)
            enchantSubWindow.SetWeapon(_weapon);
    }

    private void OnSkillSelected(SkillItemInstance _skill)
    {
        if (enchantSubWindow != null)
            enchantSubWindow.SelectSkill(_skill.ItemID);
    }
}
