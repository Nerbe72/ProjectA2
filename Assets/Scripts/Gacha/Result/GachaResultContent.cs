using NUnit.Framework;
using System;
using System.Collections.Generic;

[Serializable]
public class GachaResultContent
{
    public int CharacterId;
    public int Damage; //랜덤 공격 수치
    public int Defense; //랜덤 방어 수치
    public int TotalCount;
    /// <summary>
    /// SR등장 이후 쌓인 스택
    /// </summary>
    public int SRCurrentCount;
    /// <summary>
    /// SSR등장 이후 쌓인 스택
    /// </summary>
    public int SSRCurrentCount;
    /// <summary>
    /// 천장 체크
    /// </summary>
    public bool PickupForce;
    public string Time;

    public GachaResultContent()
    {
        CharacterId = 0;
        Damage = 0;
        Defense = 0;
        TotalCount = 0;
        SRCurrentCount = 0;
        SSRCurrentCount = 0;
        PickupForce = false;
        Time = "";
    }

    public GachaResultContent(int _characterId, int _damage, int _defense, int _totalCount, int _sRCurrentCount, int _sSRCurrentCount, bool _pickupForce, string _time)
    {
        CharacterId = _characterId;
        Damage = _damage;
        Defense = _defense;
        TotalCount = _totalCount;
        SRCurrentCount = _sRCurrentCount;
        SSRCurrentCount = _sSRCurrentCount;
        PickupForce = _pickupForce;
        Time = _time;
    }
}

[Serializable]
public class GachaResultWrapper
{
    public List<GachaResultContent> GachaResultList;

    public GachaResultWrapper()
    {
        GachaResultList = new List<GachaResultContent>();
    }

    public GachaResultWrapper(List<GachaResultContent> _list)
    {
        GachaResultList = _list;
    }
}
