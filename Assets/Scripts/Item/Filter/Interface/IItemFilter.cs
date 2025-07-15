using System.Collections.Generic;

public interface IItemFilter
{
    /// <summary>
    /// 리스트로부터 필터링된 리스트를 반환
    /// </summary>
    /// <param name="_item"></param>
    /// <returns></returns>
    public List<ItemInstance> Filter(List<ItemInstance> _item);

    public IFilterInfo GetFilterInfo();
}
