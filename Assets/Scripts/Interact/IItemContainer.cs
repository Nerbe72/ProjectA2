using UnityEngine;

/// <summary>
/// 오브젝트에 아이템을 담음
/// </summary>
public interface IItemContainer
{
    protected ItemInstance Item { get; set; }
    protected int Amount { get; set; }

    public void SetItemContainer(int _id, int _itemCount = 1);
}
