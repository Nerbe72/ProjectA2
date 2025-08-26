using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameStuff;

public class BannerContainer : MonoBehaviour
{
    public BannerData Data;
    
    [SerializeField] private Image bannerImage;
    [SerializeField] private TMP_Text bannerNameText;
    
    public async void SetBannerData(BannerData _data)
    {
        Data = _data;
        
        if (bannerImage != null)
        {
            bannerImage.sprite = await ResourceLoader.LoadAsync<Sprite>(Data.BannerPath, LoadType.GachaBanner);
        }
        
        if (bannerNameText != null)
        {
            var localeTable = Singleton.Get<TableDataManager>().Table.Locale;
            bannerNameText.text = localeTable.Get(Data.BannerName, GameManager.CurrentLocale);
        }
    }
}
