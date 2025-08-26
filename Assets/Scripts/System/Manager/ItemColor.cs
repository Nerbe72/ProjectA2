using System.Collections.Generic;

using GameStuff;

public static class ItemColor
{
    private static Dictionary<Rarity, UnityEngine.Color> item = new Dictionary<Rarity, UnityEngine.Color>
    {
        { Rarity.SSR, new UnityEngine.Color(1f, 0.72f, 0f, 1f) },
        { Rarity.SR, new UnityEngine.Color(0.8f, 0f, 1f, 1f) },
        { Rarity.R, new UnityEngine.Color(0f, 0.8f, 1f, 1f) }
    };

    public static UnityEngine.Color GetColor(Rarity _rarity)
    {
        return item.TryGetValue(_rarity, out var color) ? color : UnityEngine.Color.white;
    }
}
