using System.Collections.Generic;

public abstract class Iterator<T>
{
    public abstract List<T> SourceL { get; protected set; }
    public int Index = -1;

    public Iterator(List<T> _source)
    {
        SourceL = _source;
    }

    public abstract bool HasNext();
    public abstract T Next();
    public abstract void Reset();
}
