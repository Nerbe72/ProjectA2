using GameStuff;
using System.Threading.Tasks;
using UnityEngine;

public class IngredientFrame : FrameBase
{
    public void SetImageColor(Color _color)
    {
        if (itemImage != null)
        {
            itemImage.color = _color;
        }
    }
}
