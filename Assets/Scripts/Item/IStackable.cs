using UnityEngine;

public interface IStackable
{
    public int MaxStackSize { get; }
    public int CurrentStack { get; set; }

    public void SetMaxStackSize(int _maxStackSize);
    public void AddMaxStackSize(int _maxStackSize);
}
