using UnityEngine;

public class FilterInfo : IFilterInfo
{
    private string displayName;
    private ItemFilterType filterType;
    [SerializeField] private Sprite iconSprite;

    public FilterInfo(string _displayName, ItemFilterType _filterType, Sprite _iconSprite = null)
    {
        displayName = _displayName;
        filterType = _filterType;
        iconSprite = _iconSprite;
    }

    public string GetFilterName()
    {
        return displayName;
    }

    public ItemFilterType GetFilterType()
    {
        return filterType;
    }

    public Sprite GetIcon()
    {
        return iconSprite;
    }
}
