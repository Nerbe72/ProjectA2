using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public async void UpdateDescription(ItemInstance item)
    {
        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        UpdateLocale();

        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(item.ItemID);
        var locale = Singleton.Get<TableDataManager>().Table.Locale;

        // 아이템 정보 업데이트
        itemIcon.sprite = await ResourceLoader.LoadAsync<Sprite>(item_selected.Icon, LoadType.ItemIcon);
        itemName.text = locale.Get(item_selected.Name, GameManager.CurrentLocale);
        itemName.color = RarityColor.GetColor((Rare)item_selected.Rarity);

        if (item_selected.ItemType == (int)ItemType.Weapon)
        {
            var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(item.ItemID);
            var player = Singleton.Player;

            require_str.text = weapon_selected.Require_STR.ToString();
            require_str.color = player.GetCurrentLevel(LevelType.Strength) >= weapon_selected.Require_STR ? Color.white : Color.red;

            require_dex.text = weapon_selected.Require_DEX.ToString();
            require_dex.color = player.GetCurrentLevel(LevelType.Dexterity) >= weapon_selected.Require_DEX ? Color.white : Color.red;

            require_int.text = weapon_selected.Require_INT.ToString();
            require_int.color = player.GetCurrentLevel(LevelType.Intelligent) >= weapon_selected.Require_INT ? Color.white : Color.red;

            fix_str.text = weapon_selected.DamageGrowth_STR.ToString();
            fix_dex.text = weapon_selected.DamageGrowth_DEX.ToString();
            fix_int.text = weapon_selected.DamageGrowth_INT.ToString();

            itemDescription.text = "";

            var abilityTable = Singleton.Get<TableDataManager>().Table.WeaponAbility;

            int count = weapon_selected.Abilities.Length;
            for (int i = 0; i < count; i++)
            {
                if (weapon_selected.Abilities[i] == 0) continue;

                var ability = abilityTable.Get(weapon_selected.Abilities[i]);
                List<object> parameters = new List<object> { ability.Cooldown, ability.ContinuouseTime, ability.Damage, ability.KnockbackForce, ability.Projectile_Amount };

                itemDescription.text += string.Format(locale.Get(ability.Description, GameManager.CurrentLocale), parameters.ToArray());
                itemDescription.text += "\n";
            }
            WeaponField.SetActive(true);
        }
        else
        {
            WeaponField.SetActive(false);
            itemDescription.text = locale.Get(item_selected.Description, GameManager.CurrentLocale);
        }

        gameObject.SetActive(true);
    }
}