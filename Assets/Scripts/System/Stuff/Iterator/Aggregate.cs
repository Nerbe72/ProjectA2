using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class Aggregate<T>
{
    public abstract Iterator<T> CreateIterator(List<T> _list);
}
