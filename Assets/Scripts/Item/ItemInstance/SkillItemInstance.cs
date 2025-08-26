using System;
using UnityEngine;

[Serializable]
public class SkillItemInstance : ItemInstance, IStackable
{
    [SerializeField] public int maxStackSize = 99;
    [SerializeField] public int currentStackSize;

    public int MaxStackSize => maxStackSize;

    public int CurrentStack { get => currentStackSize; set => currentStackSize = value; }

    public override bool OnUse(int _useAmount = 1)
    {
        if (currentStackSize < _useAmount)
            return false;

        currentStackSize = Math.Max(currentStackSize - _useAmount, 0);
        return true;
    }

    public void SetMaxStackSize(int _maxStackSize)
    {
        maxStackSize = _maxStackSize;
        currentStackSize = _maxStackSize;
    }

    public void AddMaxStackSize(int _maxStackSize)
    {
        maxStackSize += _maxStackSize;
    }
}
