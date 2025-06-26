using System.Collections.Generic;

public abstract class Aggregate<T>
{
    public abstract Iterator<T> CreateIterator(List<T> _list);
}
