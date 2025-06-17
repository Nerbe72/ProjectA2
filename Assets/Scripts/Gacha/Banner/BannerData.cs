using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BannerData
{
    public BannerType bannerType;
    public string bannerName;
    /// <summary>
    /// 모든 픽업은 상시 픽업풀을 포함한다
    /// </summary>
    public List<int> SSR_PickupList;
    public List<int> SR_PickupList;
    public float SSR_Percent;
    public float SR_Percent;
    public float SSR_PickupPercent;

    public Vector2 characterPosition;
    public string bannerPath;
    public string backgroundPath;
    public int singlePrice;
    public int tenPrice;

    public BannerData()
    {
        bannerType = BannerType.None;
        bannerName = "";
        SSR_PickupList = new List<int>();
        SR_PickupList = new List<int>();
        SSR_Percent = 1f;
        SR_Percent = 19f;

        characterPosition = new Vector2();
        bannerPath = "";
        backgroundPath = "";
        singlePrice = 1;
        tenPrice = 10;
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
