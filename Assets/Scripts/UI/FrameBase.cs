using GameStuff;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class FrameBase : MonoBehaviour
{
    [SerializeField] protected Image itemImage;
    [SerializeField] protected Image itemRarityImage;
    [SerializeField] protected TMP_Text detailText;
    [SerializeField] protected TMP_Text equippedText;
    [SerializeField] protected TMP_Text enhancementText;

    protected ItemInstance instance;

    public virtual async Task SetFrameData(ItemInstance _item)
    {
        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(_item.ItemID);

        itemImage.color = Color.clear;

        if (itemImage != null)
        {
            itemImage.color = Color.white;
            itemImage.sprite = await ResourceLoader.LoadAsync<Sprite>(item_selected.Icon, LoadType.ItemIcon);
        }

        if (itemRarityImage != null)
            itemRarityImage.color = ItemColor.GetColor((Rarity)item_selected.Rarity);

        SetItemDetail("");

        if (enhancementText != null && _item != null)
        {
            bool shouldHide = (ItemType)item_selected.ItemType == ItemType.Material || 
                            (ItemType)item_selected.ItemType == ItemType.Scroll || 
                            (ItemType)item_selected.ItemType == ItemType.Skill || 
                            (ItemType)item_selected.ItemType == ItemType.Potion;
            
            enhancementText.gameObject.SetActive(!shouldHide);
        }

        instance = _item;
    }

    public virtual void SetItemDetail(string _detail)
    {
        if (detailText != null)
        {
            detailText.text = _detail;
            detailText.gameObject.SetActive(true);
        }
    }

    public virtual void SetEnhancementText(string _enhancement)
    {
        if (enhancementText != null)
        {
            enhancementText.text = _enhancement;
            enhancementText.gameObject.SetActive(!string.IsNullOrEmpty(_enhancement));
        }
    }

    public ItemInstance GetItemInstance()
    {
        return instance;
    }
}
