
using NUnit.Framework;
using System.Collections.Generic;

public interface IItemSorter
{
    /// <summary>
    /// 리스트로부터 정렬된 리스트를 반환
    /// </summary>
    /// <param name="_items"></param>
    /// <returns></returns>
    public List<ItemInstance> Sort(List<ItemInstance> _items);

    public void ToggleSortDirection();

    public ISortInfo GetSortInfo();
}
