using System;
using System.Collections.Generic;

[Serializable]
public class InventoryWrapper
{
    public List<WeaponItemInstance> weapons;
    public List<SkillItemInstance> skills;
    public List<PotionItemInstance> potions;
    public List<MaterialItemInstance> materials;
    public List<ScrollItemInstance> scrolls;
    public List<WeaponEnhancementAdapter> weaponAdapters;
    public uint Currency;

    public InventoryWrapper()
    {
        weapons = new List<WeaponItemInstance>();
        skills = new List<SkillItemInstance>();
        potions = new List<PotionItemInstance>();
        materials = new List<MaterialItemInstance>();
        scrolls = new List<ScrollItemInstance>();
        weaponAdapters = new List<WeaponEnhancementAdapter>();
        Currency = 0;
    }
}
