using System.Collections.Generic;

public class GachaResultData
{
    public BannerData Banner;
    public int Count;
    public bool MinigameSuccess;
    public List<(int ID, RandomWeaponData Data)> Items;

    public GachaResultData(BannerData banner, int count)
    {
        Banner = banner;
        Count = count;
        MinigameSuccess = false;
        Items = new List<(int ID, RandomWeaponData Data)>();
    }
}

public class RandomWeaponData
{
    public int Damage;
    public int Defense;

    public RandomWeaponData()
    {
        Damage = 1;
        Defense = 1;
    }

    public RandomWeaponData(int damage, int defense)
    {
        Damage = damage;
        Defense = defense;
    }
}