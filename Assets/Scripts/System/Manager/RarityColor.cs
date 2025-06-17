using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RarityColor
{
    public static Dictionary<Rare, UnityEngine.Color> Color = new Dictionary<Rare, UnityEngine.Color>
    {
        { Rare.SSR, new UnityEngine.Color(1f, 0.72f, 0f, 1f) },
        { Rare.SR, new UnityEngine.Color(0.8f, 0f, 1f, 1f) },
        { Rare.R, new UnityEngine.Color(0f, 0.8f, 1f, 1f) }
    };
}
