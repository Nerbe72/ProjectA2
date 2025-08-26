using GameStuff;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class HoverClickableFrame : ClickableFrame, IPointerEnterHandler, IPointerExitHandler
{
    private int itemID;
    private RectTransform rectTransform;

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var table = Singleton.Get<TableDataManager>().Table;

        if (table == null)
        {
            Debug.LogError("Item table is null in HoverClickableFrame");
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

    public async void SetItemID(int _itemID)
    {
        itemID = _itemID;
        instance = null;

        var tableItem = Singleton.Get<TableDataManager>().Table.Item;

        if (tableItem == null)
        {
            Debug.LogError($"TableDataManager is null in {nameof(HoverClickableFrame)}");
            return;
        }

        var item_selected = tableItem.Get(_itemID);

        itemImage.sprite = await ResourceLoader.LoadAsync<Sprite>(item_selected.Icon, LoadType.ItemIcon);
    }

    public override async Task SetFrameData(ItemInstance _item)
    {
        await base.SetFrameData(_item);

        if (_item is IStackable stackable)
        {
            SetItemDetail($"{stackable.CurrentStack}/{stackable.MaxStackSize}");
        }
        else
        {
            SetItemDetail("");
        }

        if (_item is WeaponItemInstance weaponInstance)
        {
            var weaponAdapter = Singleton.Inventory.GetWeaponAdapter(weaponInstance);
            if (weaponAdapter != null && weaponAdapter.EnhancedLevel > 0)
            {
                SetEnhancementText($"+{weaponAdapter.EnhancedLevel}");
            }
            else
            {
                SetEnhancementText("");
            }
        }
        else
        {
            SetEnhancementText("");
        }
    }
}
