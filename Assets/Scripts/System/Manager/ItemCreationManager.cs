using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using GameStuff;

public class ItemCreationManager : MonoBehaviour
{
    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public (bool canCraft, Dictionary<int, int> missingMaterials) CanCraft(IReadOnlyList<string> ingredients)
    {
        var missingMaterials = new Dictionary<int, int>();
        
        var inventory = Singleton.Inventory;
        if (inventory == null)
        {
            Debug.LogError("ItemCreationManager: Inventory 인스턴스를 찾을 수 없습니다.");
            return (false, missingMaterials);
        }

        List<ItemInstance> allItems = inventory.GetAllInventoryItems();

        foreach (var ingredientString in ingredients)
        {
            var parts = ingredientString.Split(':');
            if (parts.Length != 2) continue;

            if (!int.TryParse(parts[0], out int requiredItemId)) continue;
            if (!int.TryParse(parts[1], out int requiredAmount)) continue;

            int ownedAmount = allItems
                .Where(item => item != null && item.ItemID == requiredItemId)
                .Sum(item => item is IStackable stackable ? stackable.CurrentStack : 1);

            if (ownedAmount < requiredAmount)
            {
                missingMaterials[requiredItemId] = requiredAmount - ownedAmount;
            }
        }

        return (missingMaterials.Count == 0, missingMaterials);
    }

    public async Task<bool> TryCreateItem(IReadOnlyList<string> _ingredients, uint _price, int _creationItemId)
    {
        var inventory = Singleton.Inventory;
        if (inventory == null)
        {
            Debug.LogError($"Inventory is null {nameof(ItemCreationManager)}");
            return false;
        }

        var (canCraft, _) = CanCraft(_ingredients);
        if (!canCraft || !inventory.IsCurrencyEnough(_price))
        {
            Singleton.Get<Alert>().Show("재화가 부족합니다", Color.red);
            Debug.LogWarning("제작 불가");
            return false;
        }

        foreach (var ingredientString in _ingredients)
        {
            var parts = ingredientString.Split(':');
            if (parts.Length != 2) continue;
            if (!int.TryParse(parts[0], out int itemId)) continue;
            if (!int.TryParse(parts[1], out int amount)) continue;
            inventory.RemoveItemByID(itemId, amount);
        }

        inventory.MinusCurrency(_price);

        // 아이템 생성 및 추가 (랜덤 스탯)
        bool success = await Singleton.Get<ItemFactory>().CreateAndAddToInventoryWithRandomStatsAsync(_creationItemId);
        if (!success)
        {
            Debug.LogError($"아이템 생성 및 인벤토리 추가에 실패 ItemID: {_creationItemId}");
            return false;
        }

        return true;
    }
}
