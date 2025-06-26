using System.Collections.Generic;

public class ItemIterator : Iterator<ItemInstance>
{
    public ItemIterator(List<ItemInstance> _source) : base(_source) { Reset(); }

    public override List<ItemInstance> SourceL { get; protected set; }

    public override bool HasNext()
    {
        int tempIndex = Index + 1;
        if (tempIndex > SourceL.Count - 1)
            return false;

        return true;
    }

    public bool IsType(ItemType _type)
    {
        var item_selected = Singleton.Get<TableDataManager>().Table.Item.Get(SourceL[Index].ItemID);

        if ((ItemType)item_selected.ItemType == _type) return true;

        return false;
    }

    public override ItemInstance Next()
    {
        if (!HasNext()) return null;

        ++Index;

        return SourceL[Index];
    }

    public override void Reset()
    {
        Index = -1;
    }
}
