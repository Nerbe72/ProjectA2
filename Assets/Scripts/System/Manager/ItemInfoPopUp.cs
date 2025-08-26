using GameStuff;
using UnityEngine;

public class ItemInfoPopUp : MonoBehaviour
{
    private InfoFrame hoverInfo;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        hoverInfo = GetComponentInChildren<InfoFrame>(true);
    }

    public void Show(RectTransform _transform, int _itemID, InfoDisplayType _displayType = InfoDisplayType.TableInfoWeapon, ItemInstance _instance = null)
    {
        hoverInfo.Show(_transform, _itemID, _displayType, _instance);
    }

    public void Hide()
    {
        hoverInfo.Hide();
    }
}
