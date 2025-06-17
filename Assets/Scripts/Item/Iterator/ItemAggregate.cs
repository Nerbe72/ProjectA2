using System.Collections.Generic;

public class ItemAggregate : Aggregate<ItemInstance>
{
    public override Iterator<ItemInstance> CreateIterator(List<ItemInstance> _list)
    {
        return new ItemIterator(_list);
    }
}
