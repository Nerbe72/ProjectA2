using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using GameStuff;

public class Creation : MonoBehaviour
{
    [SerializeField] private TMP_Text targetNameText;
    [SerializeField] private HoverableFrame targetItem;
    [SerializeField] private GameObject ingredientFramePrefab;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button creationButton;

    [SerializeField] private TMP_Text ingredientText;
    [SerializeField] private TMP_Text priceTitleText;
    [SerializeField] private TMP_Text createText;

    private Inventory inventory;
    private List<IngredientFrame> ingredientFrames;

    private ScrollRect ingredientScroll;

    private int currentScrollId;
    private IReadOnlyList<string> currentIngredients;
    private uint currentPrice;
    private int currentCreationItemId;

    private void Awake()
    {
        ingredientScroll = GetComponentInChildren<ScrollRect>();
        ingredientFrames = new List<IngredientFrame>();
    }

    private void Start()
    {
        inventory = Singleton.Inventory;
        Singleton.Get<GameManager>().OnLocaleChanged += SetLocale;

        SetLocale();
        creationButton.onClick.AddListener(OnClickCreate);
    }

    private void SetLocale()
    {
        var table = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;

        targetNameText.text = "";
        ingredientText.text = table.Get(10000044, locale);
        priceTitleText.text = table.Get(10000012, locale);
        createText.text = table.Get(10000045, locale);
    }

    public async void SetData(int _scrollID)
    {
        var table = Singleton.Get<TableDataManager>().Table;

        if (table == null)
        {
            Debug.LogError($"Scroll table is null in {nameof(Creation)}");
            return;
        }

        var localTable = Singleton.Get<TableDataManager>().Table.Locale;
        var scroll_selected = table.Scroll.Get(_scrollID);
        var creationTarget_selected = table.Item.Get(scroll_selected.CreationItemID);

        currentScrollId = _scrollID;
        currentIngredients = scroll_selected.IngredientIDs.AsReadOnly();
        currentPrice = (uint)scroll_selected.Price;
        currentCreationItemId = scroll_selected.CreationItemID;

        targetNameText.text = localTable.Get(creationTarget_selected.Name, GameManager.CurrentLocale);

        targetItem.SetItemID(creationTarget_selected.ID);

        ClearIngredientFrames();

        bool canCraft = true;
        IReadOnlyList<string> ingredients = scroll_selected.IngredientIDs.AsReadOnly();
        
        for (int i = 0; i < ingredients.Count; i++)
        {
            string ingredientData = ingredients[i];
            if (string.IsNullOrEmpty(ingredientData)) continue;

            string[] parts = ingredientData.Split(':');
            if (parts.Length != 2) continue;

            if (!int.TryParse(parts[0], out int itemId) || !int.TryParse(parts[1], out int requiredCount))
                continue;

            IngredientFrame frame = GetOrCreateIngredientFrame(i);
            
            var itemData = table.Item.Get(itemId);
            ItemInstance displayItem = CreateDisplayItem(itemId, itemData);
            
            await frame.SetFrameData(displayItem);
            
            int ownedCount = GetOwnedItemCount(itemId);
            
            string detailText = $"{ownedCount}/{requiredCount}";
            frame.SetItemDetail(detailText);
            
            if (ownedCount < requiredCount)
            {
                canCraft = false;
                SetFrameImageColor(frame, Color.red);
            }
            else
            {
                SetFrameImageColor(frame, Color.white);
            }
        }
        
        if (createText != null)
        {
            createText.color = canCraft ? Color.black : Color.red;
        }
        
        priceText.text = scroll_selected.Price.ToString();
    }

    private List<ItemInstance> GetInventoryItems()
    {
        var itemsField = typeof(Inventory).GetField("items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (itemsField != null)
        {
            var items = itemsField.GetValue(inventory) as List<ItemInstance>;
            return items ?? new List<ItemInstance>();
        }
        
        return new List<ItemInstance>();
    }

    private void ClearIngredientFrames()
    {
        foreach (var frame in ingredientFrames)
        {
            if (frame != null && frame.gameObject != null)
            {
                frame.gameObject.SetActive(false);
            }
        }
    }

    private IngredientFrame GetOrCreateIngredientFrame(int _index)
    {
        if (_index < ingredientFrames.Count && ingredientFrames[_index] != null)
        {
            ingredientFrames[_index].gameObject.SetActive(true);
            return ingredientFrames[_index];
        }

        GameObject frameObj = Instantiate(ingredientFramePrefab, ingredientScroll.content);
        IngredientFrame frame = frameObj.GetComponent<IngredientFrame>();
        
        if (frame == null)
        {
            frame = frameObj.AddComponent<IngredientFrame>();
        }

        if (_index >= ingredientFrames.Count)
        {
            while (ingredientFrames.Count <= _index)
            {
                ingredientFrames.Add(null);
            }
        }
        
        ingredientFrames[_index] = frame;
        frameObj.SetActive(true);
        
        return frame;
    }

    private ItemInstance CreateDisplayItem(int _itemId, TableItem.Info _itemData)
    {
        switch ((ItemType)_itemData.ItemType)
        {
            case ItemType.Weapon:
                return new WeaponItemInstance { ItemID = _itemId };
            case ItemType.Potion:
                return new PotionItemInstance { ItemID = _itemId };
            case ItemType.Material:
                return new MaterialItemInstance { ItemID = _itemId };
            case ItemType.Skill:
                return new SkillItemInstance { ItemID = _itemId };
            default:
                return new MaterialItemInstance { ItemID = _itemId };
        }
    }

    private int GetOwnedItemCount(int _itemId)
    {
        if (inventory == null) return 0;

        var itemData = Singleton.Get<TableDataManager>().Table.Item.Get(_itemId);
        ItemType itemType = (ItemType)itemData.ItemType;

        var inventoryItems = GetInventoryItems();
        
        switch (itemType)
        {
            case ItemType.Weapon:
                return inventoryItems.OfType<WeaponItemInstance>().Count(w => w.ItemID == _itemId);
            case ItemType.Potion:
                return inventoryItems.OfType<PotionItemInstance>().Where(p => p.ItemID == _itemId).Sum(p => p.CurrentStack);
            case ItemType.Material:
                return inventoryItems.OfType<MaterialItemInstance>().Where(m => m.ItemID == _itemId).Sum(m => m.CurrentStack);
            case ItemType.Skill:
                return inventoryItems.OfType<SkillItemInstance>().Count(s => s.ItemID == _itemId);
            default:
                return 0;
        }
    }

    private void SetFrameImageColor(IngredientFrame _frame, Color _color)
    {
        if (_frame != null)
        {
            _frame.SetImageColor(_color);
        }
    }

    private async void OnClickCreate()
    {
        bool created = await Singleton.Get<ItemCreationManager>().TryCreateItem(currentIngredients, currentPrice, currentCreationItemId);
        if (created)
            Singleton.Inventory.SaveInventoryData();
    }
}
