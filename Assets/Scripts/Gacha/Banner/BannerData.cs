using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BannerData
{
    public BannerType BannerType;
    public int BannerName;
    /// <summary>
    /// 모든 픽업은 상시 픽업풀을 포함한다
    /// </summary>
    public List<int> SSR_PickupList;
    public List<int> SR_PickupList;
    public float SSR_Percent;
    public float SR_Percent;
    public float SSR_PickupPercent;

    public Vector2 CharacterPosition;
    public string BannerPath;
    public string BackgroundPath;
    public int SinglePrice;
    public int TenPrice;

    public BannerData()
    {
        BannerType = BannerType.None;
        BannerName = 0;
        SSR_PickupList = new List<int>();
        SR_PickupList = new List<int>();
        SSR_Percent = 1f;
        SR_Percent = 19f;

        CharacterPosition = new Vector2();
        BannerPath = "";
        BackgroundPath = "";
        SinglePrice = 1;
        TenPrice = 10;
    }
}

[Serializable]
public class BannerWrapper
{
    public List<BannerData> banners;

    public BannerWrapper()
    {
        banners = new List<BannerData>();
    }
}
