using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class PotionItemInstance : ItemInstance
{
    public int MaxAmount;
    public int CurrentAmount;

    public void RegeneratePotion()
    {
        CurrentAmount = MaxAmount;
    }

    public override void OnUse()
    {
        CurrentAmount = Math.Clamp(CurrentAmount - 1, 0, MaxAmount);
    }
}
