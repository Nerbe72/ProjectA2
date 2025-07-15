using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemFrame : MonoBehaviour, IPointerClickHandler
{
    private Toggle self;
    [SerializeField] private Image itemImage;
    [SerializeField] private Image itemRarityImage;
    [SerializeField] private TMP_Text equippedIndicator;

    private ItemInstance instance;
    private bool selected;

    private void Awake()
    {
        self = GetComponent<Toggle>();
        self.onValueChanged.AddListener(ChangeSelected);
    }

    private void OnDestroy()
    {
        self.onValueChanged.RemoveAllListeners();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (!selected) return;

        Singleton.Inventory.ShowItemMenu(instance, GetComponent<RectTransform>());
    }

    public async void SetFrameData(ItemInstance _item)
    {
        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(_item.ItemID);

        itemImage.sprite = await ResourceLoader.LoadAsync<Sprite>(item_selected.Icon, LoadType.ItemIcon);
        itemRarityImage.color = RarityColor.GetColor((Rare)item_selected.Rarity);
        instance = _item;
        itemImage.gameObject.SetActive(true);
    }

    public void SetEquipped()
    {
        equippedIndicator.gameObject.SetActive(true);
    }

    public void UnEquipped()
    {
        equippedIndicator.gameObject.SetActive(false);
    }

    private void ChangeSelected(bool _selected)
    {
        selected = _selected;
        if (_selected) Singleton.Inventory.SelectItem(instance);
    }
}
