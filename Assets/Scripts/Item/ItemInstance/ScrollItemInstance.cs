using System;
using UnityEngine;

[Serializable]
public class ScrollItemInstance : ItemInstance, IStackable
{
    public int maxStack;
    public int currentStack;

    public int MaxStackSize => maxStack;

    public int CurrentStack { get => currentStack; set => currentStack = value; }

    public override bool OnUse(int _useAmount = 1)
    {
        if (currentStack < 0)
            return false;

        currentStack = Math.Max(currentStack - _useAmount, 0);
        return true;
    }

    public void SetMaxStackSize(int _maxStackSize)
    {
        maxStack = _maxStackSize;
        currentStack = _maxStackSize;
    }

    public void AddMaxStackSize(int _maxStackSize)
    {
        maxStack += _maxStackSize;
    }
}
