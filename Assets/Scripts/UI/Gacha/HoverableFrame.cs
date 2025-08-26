using GameStuff;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class HoverableFrame : FrameBase, IPointerEnterHandler, IPointerExitHandler
{
    protected int itemID;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var table = Singleton.Get<TableDataManager>().Table;

        if (table == null)
        {
            Debug.LogError("Item table is null in HoverableFrame");
            return;
        }

        var item_selected = table.Item.Get(itemID);

        if (item_selected == null)
            return;

        InfoDisplayType displayType = InfoDisplayType.TableInfoWeapon;

        switch ((ItemType)item_selected.ItemType)
        {
            case ItemType.Weapon:
                {
                    if (instance == null)
                        displayType = InfoDisplayType.TableInfoWeapon;
                    else
                        displayType = InfoDisplayType.ActualWeapon;
                }
                break;
            case ItemType.Skill:
                {
                    displayType = InfoDisplayType.Skill;
                }
                break;
        }

        Singleton.Get<ItemInfoPopUp>().Show(rectTransform, itemID, displayType, instance);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Singleton.Get<ItemInfoPopUp>().Hide();
    }

    public async virtual void SetItemID(int _itemID)
    {
        itemID = _itemID;
        instance = null;

        if (itemID == 0)
        {
            itemImage.color = Color.clear;
            SetEnhancementText("");
            return;
        }

        var tableItem = Singleton.Get<TableDataManager>().Table.Item;

        if (tableItem == null)
        {
            Debug.LogError($"TableDataManager is null in {nameof(HoverableFrame)}");
            return;
        }

        var item_selected = tableItem.Get(_itemID);

        itemImage.color = Color.white;
        itemImage.sprite = await ResourceLoader.LoadAsync<Sprite>(item_selected.Icon, LoadType.ItemIcon);

        SetEnhancementText("");
    }

    public override async Task SetFrameData(ItemInstance _item)
    {
        await base.SetFrameData(_item);

        // 강화수치 표시
        if (_item is WeaponItemInstance weaponInstance)
        {
            var weaponAdapter = Singleton.Inventory.GetWeaponAdapter(weaponInstance);

            if (weaponAdapter != null && weaponAdapter.EnhancedLevel > 0)
                SetEnhancementText($"+{weaponAdapter.EnhancedLevel}");
            else
                SetEnhancementText("");
        }
        else
            SetEnhancementText("");
    }

    public void SetFrameDataFromRandom(TableItem.Info _targetInfo, RandomWeaponData _targetRandom)
    {
        var temp_weapon = new WeaponItemInstance();

        temp_weapon.ItemID = _targetInfo.ID;
        temp_weapon.Damage = _targetRandom.Damage;
        temp_weapon.Defense = _targetRandom.Defense;

        itemID = _targetInfo.ID;
        instance = temp_weapon;

        itemImage.color = Color.white;
        itemImage.sprite = ResourceLoader.Load<Sprite>(_targetInfo.Icon, LoadType.ItemIcon);
        itemRarityImage.color = ItemColor.GetColor((Rarity)_targetInfo.Rarity);
    }
}
