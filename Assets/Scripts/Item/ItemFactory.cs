public static class ItemFactory
{
    public static ItemInstance CreateItemInstance(int _id)
    {
        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(_id);

        switch ((ItemType)item_selected.ItemType)
        {
            case ItemType.Weapon:
                return CreateWeaponInstance(item_selected);
            case ItemType.Potion:
                return CreatePotionInstance(item_selected);
            default:
                throw new System.ArgumentException("Item Type Error");
        }
    }

    private static WeaponItemInstance CreateWeaponInstance(TableItem.Info _info)
    {
        var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(_info.WeaponID);

        float multiply = 1 + (0.1f * _info.Rarity);
        int damage = UnityEngine.Random.Range(weapon_selected.Damage_Min, weapon_selected.Damage_Max);
        int defense = UnityEngine.Random.Range(weapon_selected.Defense_Min, weapon_selected.Defense_Max);

        return new WeaponItemInstance
        {
            ItemID = weapon_selected.ID,
            Damage = damage,
            Defense = defense
        };
    }

    private static PotionItemInstance CreatePotionInstance(TableItem.Info _info)
    {
        return new PotionItemInstance
        {
            ItemID = _info.ID,
            CurrentAmount = 1
        };
    }
}
