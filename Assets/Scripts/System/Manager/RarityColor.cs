using System.Collections.Generic;

public static class RarityColor
{
    public static Dictionary<Rare, UnityEngine.Color> Color = new Dictionary<Rare, UnityEngine.Color>
    {
        { Rare.SSR, new UnityEngine.Color(1f, 0.72f, 0f, 1f) },
        { Rare.SR, new UnityEngine.Color(0.8f, 0f, 1f, 1f) },
        { Rare.R, new UnityEngine.Color(0f, 0.8f, 1f, 1f) }
    };

    public static UnityEngine.Color GetColor(Rare rarity)
    {
        return Color.TryGetValue(rarity, out var color) ? color : UnityEngine.Color.white;
    }
}
