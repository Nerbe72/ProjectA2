using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using GameStuff;

public class ItemDescription : MonoBehaviour
{
    [Header("Locale")]
    [SerializeField] private TMP_Text requirementStat;
    [SerializeField] private TMP_Text damageFixed;

    [Header("Data")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private GameObject WeaponField;
    [SerializeField] private TMP_Text require_str;
    [SerializeField] private TMP_Text require_dex;
    [SerializeField] private TMP_Text require_int;

    [SerializeField] private TMP_Text fix_str;
    [SerializeField] private TMP_Text fix_dex;
    [SerializeField] private TMP_Text fix_int;

    [SerializeField] private TMP_Text itemDescription;

    private void Awake()
    {
        // 초기에는 비활성화 상태로 시작
        gameObject.SetActive(false);
    }

    private void UpdateLocale()
    {
        var table = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;
        requirementStat.text = table.Get(10000021, locale);
        damageFixed.text = table.Get(10000022, locale);
    }

    public async void UpdateDescription(ItemInstance _instance)
    {
        if (_instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        UpdateLocale();

        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(_instance.ItemID);
        var locale = Singleton.Get<TableDataManager>().Table.Locale;

        // 아이템 정보 업데이트
        itemIcon.sprite = await ResourceLoader.LoadAsync<Sprite>(item_selected.Icon, LoadType.ItemIcon);
        itemName.text = locale.Get(item_selected.Name, GameManager.CurrentLocale);
        itemName.color = ItemColor.GetColor((Rarity)item_selected.Rarity);

        switch ((ItemType)item_selected.ItemType)
        {
            case ItemType.Weapon:
            {
                var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(_instance.ItemID);
                var player = Singleton.Player;

                require_str.text = weapon_selected.Require_STR.ToString();
                require_str.color = player.GetCurrentLevel(LevelType.Strength) >= weapon_selected.Require_STR ? Color.white : Color.red;

                require_dex.text = weapon_selected.Require_DEX.ToString();
                require_dex.color = player.GetCurrentLevel(LevelType.Dexterity) >= weapon_selected.Require_DEX ? Color.white : Color.red;

                require_int.text = weapon_selected.Require_INT.ToString();
                require_int.color = player.GetCurrentLevel(LevelType.Intelligent) >= weapon_selected.Require_INT ? Color.white : Color.red;

                // 강화된 성장수치 적용
                var weaponInstance = _instance as WeaponItemInstance;
                var weaponAdapter = Singleton.Inventory.GetWeaponAdapter(weaponInstance);
                if (weaponAdapter != null)
                {
                    fix_str.text = weaponAdapter.GetGrowth(LevelType.Strength).ToString();
                    fix_dex.text = weaponAdapter.GetGrowth(LevelType.Dexterity).ToString();
                    fix_int.text = weaponAdapter.GetGrowth(LevelType.Intelligent).ToString();
                }
                else
                {
                    fix_str.text = weapon_selected.DamageGrowth_STR.ToString();
                    fix_dex.text = weapon_selected.DamageGrowth_DEX.ToString();
                    fix_int.text = weapon_selected.DamageGrowth_INT.ToString();
                }

                if (weaponAdapter != null)
                {
                    var activeSkills = weaponAdapter.GetActiveSkills();
                    if (activeSkills.Count > 0)
                    {
                        itemDescription.text = "귀속된 스킬:\n";
                        foreach (var skill in activeSkills)
                        {
                            itemDescription.text += Singleton.Get<AbilityManager>().GetDescription(skill.AbilityId);
                            itemDescription.text += "\n";
                        }
                    }
                    else
                    {
                        itemDescription.text = "";
                    }
                }
                else
                {
                    itemDescription.text = "";
                }
                WeaponField.SetActive(true);
                break;
            }
            case ItemType.Skill:
                {
                    SetSkillDescription(_instance);
                    break;
                }
            default:
            {
                WeaponField.SetActive(false);
                itemDescription.text = locale.Get(item_selected.Description, GameManager.CurrentLocale);
                break;
            }
        }

        gameObject.SetActive(true);
    }

    private void SetSkillDescription(ItemInstance _instance)
    {
        WeaponField.SetActive(false);

        var table = Singleton.Get<TableDataManager>().Table;
        var localeTable = table.Locale;
        var item_selected = table.Item.Get(_instance.ItemID);

        if (_instance is SkillItemInstance skillItem)
        {
            itemDescription.text = Singleton.Get<AbilityManager>().GetDescription(skillItem.ItemID);
        }
        else
        {
            itemDescription.text = localeTable.Get(item_selected.Description, GameManager.CurrentLocale);
        }
    }
}