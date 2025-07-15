using ExitGames.Client.Photon.StructWrapping;
using System.Collections.Generic;
using System.Linq;

public class WeaponFilter : IItemFilter
{
    private FilterInfo filterInfo;
    private WeaponFilterType weaponFilterSubType;

    public WeaponFilter(WeaponFilterType _subType = WeaponFilterType.All)
    {
        var tableManager = Singleton.Get<TableDataManager>();
        var locale = GameManager.CurrentLocale;
        filterInfo = new FilterInfo(tableManager.Table.Locale.Get((int)ItemFilterType.Weapon + 110, locale), ItemFilterType.Weapon);
    }

    public List<ItemInstance> Filter(List<ItemInstance> _item)
    {
        var tableManager = Singleton.Get<TableDataManager>();

        return _item.Where(item =>
        {
            var itemData = tableManager.Table.Item.Get(item.ItemID);
            if ((ItemType)itemData.ItemType != ItemType.Weapon)
                return false;

            if (weaponFilterSubType == WeaponFilterType.All)
                return true;

            var weaponData = tableManager.Table.Weapon.Get(item.ItemID);

            //return IsWeaponMatched((WeaponType)weaponData.WeaponType);
            return false;
        }).ToList();
    }

    //private bool IsWeaponMatched(WeaponType _weaponType)
    //{

    //}

    public IFilterInfo GetFilterInfo()
    {
        return null;
    }
}
