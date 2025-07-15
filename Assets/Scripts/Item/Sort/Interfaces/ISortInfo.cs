
public interface ISortInfo
{
    public string GetSortName();

    /// <summary>
    /// 정렬 타입 반환
    /// </summary>
    /// <returns></returns>
    public SortMainType GetSortMain();

    /// <summary>
    /// 정렬 방향 반환
    /// </summary>
    /// <returns></returns>
    public SortDirectionType GetSortDirection();

    public void SetSortDirection(SortDirectionType _sortDirectionType);
}
