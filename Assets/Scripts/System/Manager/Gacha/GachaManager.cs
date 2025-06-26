using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class GachaManager : MonoBehaviour
{
    public int InitializationPriority => 1;
    public List<BannerData> BannerDatas { get; private set; }
    public List<int> RollHistory { get; private set; }
    public GameObject Self { get; set; }

    [SerializeField] private int srCeil = 10;
    [SerializeField] private int ssrAdventageStart = 50; //확업 시작
    [SerializeField] private int ssrCeil = 70; //ssr천장수치
    [SerializeField] private float ssrChanceIncrease = 1.5f;

    private int totalCount = 0;
    private int currentSSRCount = 0; // 마지막 SSR 등장 이후 누적 뽑기 수
    private int currentSRCount = 0; // 마지막 SR 또는 SSR 등장 이후 누적 뽑기 수
    private bool pickupForce = false;  // 이전 SSR이 픽업이 아니었을 경우 다음 SSR은 무조건 픽업 처리

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitBannerDatas();
    }

    /// <summary>
    /// 가챠 확률 계산
    /// </summary>
    /// <returns>결과(캐릭터id) 반환</returns>
    public void StartGacha(ref GachaResultData _result)
    {
        List<(int ID, RandomWeaponData Data)> results = new List<(int ID, RandomWeaponData Data)>();
        List<GachaResultContent> logs = new List<GachaResultContent>();

        for (int i = 0; i < _result.Count; i++)
        {
            //todo: 시간추가
            var now = DateTime.Now;
            string currentTime =
                $"{TimeZoneInfo.Local.DisplayName.Split(" ")[0]} {now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-{now.Second}:{i}";
            // 각 뽑기마다 pity 카운터 증가
            totalCount++;
            currentSSRCount++;
            currentSRCount++;

            // 강제 천장 조건
            bool forcedSSR = (currentSSRCount >= ssrCeil);
            bool forcedSR = (currentSRCount >= srCeil);

            // SR확정 우선 적용
            if (forcedSR)
            {
                (int ID, RandomWeaponData Data) srResult = GetSRResult(_result.Banner);
                results.Add(srResult);
                currentSRCount = 0;
                logs.Add(new GachaResultContent(srResult.ID, srResult.Data.Damage, srResult.Data.Defense, totalCount, currentSRCount, currentSSRCount, pickupForce, currentTime));
                continue;
            }

            // SSR 확정 적용
            if (forcedSSR)
            {
                (int ID, RandomWeaponData Data) ssrResult = GetSSRResult(ref pickupForce, _result.Banner);
                results.Add(ssrResult);
                currentSSRCount = 0;
                currentSRCount = 0;
                logs.Add(new GachaResultContent(ssrResult.ID, ssrResult.Data.Damage, ssrResult.Data.Defense, totalCount, currentSRCount, currentSSRCount, pickupForce, currentTime));
                continue;
            }

            //미니게임 성공시 SSR 확률 증가
            float SSRChance = _result.Banner.SSR_Percent;
            if (_result.MinigameSuccess)
            {
                SSRChance += 5.0f; // 미니게임 성공 시 + 5%p
            }
            if (currentSSRCount >= ssrAdventageStart)
            {
                SSRChance += (currentSSRCount - ssrAdventageStart - 1) * ssrChanceIncrease;
            }

            // ssr확률 증가 계산
            SSRChance = _result.Banner.SSR_Percent;
            if (currentSSRCount >= ssrAdventageStart)
            {
                SSRChance += (currentSSRCount - ssrAdventageStart - 1) * ssrChanceIncrease;
            }

            // SSR 판정
            float roll = Random.Range(0f, 100f);
            if (roll < SSRChance)
            {
                (int ID, RandomWeaponData Data) ssrResult = GetSSRResult(ref pickupForce, _result.Banner);
                results.Add(ssrResult);
                currentSSRCount = 0;
                currentSRCount = 0;
                logs.Add(new GachaResultContent(ssrResult.ID, ssrResult.Data.Damage, ssrResult.Data.Defense, totalCount, currentSRCount, currentSSRCount, pickupForce, currentTime));
            }
            else
            {
                // SSR 실패 시 SR 판정
                float srRoll = Random.Range(0f, 100f);
                if (srRoll < _result.Banner.SR_Percent)
                {
                    (int ID, RandomWeaponData Data) srResult = GetSRResult(Singleton.Get<TableDataManager>().GetItemIDsByRare(Rare.SR));
                    results.Add(srResult);
                    currentSRCount = 0;
                    logs.Add(new GachaResultContent(srResult.ID, srResult.Data.Damage, srResult.Data.Defense, totalCount, currentSRCount, currentSSRCount, pickupForce, currentTime));
                }
                else
                {
                    // 나머지는 R 등급 처리 (기본값 0)
                    (int ID, RandomWeaponData Data) rResult = GetRResult();
                    results.Add(rResult);
                    logs.Add(new GachaResultContent(rResult.ID, rResult.Data.Damage, rResult.Data.Defense, totalCount, currentSRCount, currentSSRCount, pickupForce, currentTime));
                }
            }
        }

        // 결과 출력 (디버그 로그)
        for (int i = 0; i < results.Count; i++)
        {
            Debug.Log(string.Format("뽑기 결과 {0}: {1}", i + 1, results[i]));
        }
        Debug.Log("SSR 스택: " + currentSSRCount);

        //결과 저장
        Task task = Singleton.Get<AuthManager>().SetDataAsync(Request.writegachalog, new GachaResultWrapper(logs));
        task.ContinueWith(task =>
        {
            Debug.LogWarning("가챠정보 저장 실패, 1회 재시도합니다.");
        }, TaskContinuationOptions.OnlyOnFaulted);

        _result.Items = results;
    }

    /// <summary>
    /// _pickupForce 플래그가 활성화되어 있다면 무조건 픽업 SSR을 선택,
    /// or 50% 확률로 픽업 SSR, 아닐 경우 다음 SSR은 무조건 픽업 SSR이 되도록 플래그 설정
    /// </summary>
    private (int ID, RandomWeaponData Data) GetSSRResult(ref bool _pickupForce, BannerData _banner)
    {
        if (_pickupForce)
        {
            (int ID, RandomWeaponData Data) result = PickFromList(_banner.SSR_PickupList, -3);
            _pickupForce = false;
            return result;
        }
        else
        {
            float pickupRoll = Random.Range(0f, 100f);
            if (pickupRoll < 50f)
            {
                return PickFromList(_banner.SSR_PickupList, -3);
            }
            else
            {
                // 픽업이 아닌 경우 다음 SSR은 무조건 픽업 SSR이 되도록 플래그 설정
                _pickupForce = true;
                var weapons = Singleton.Get<TableDataManager>().GetItemIDsByRare(Rare.SSR);
                return PickFromList(weapons, -3); //CharacterManager.GetCharactersFromRare(Rare.SSR), -1);
            }
        }
    }

    /// <summary>
    /// SR 결과 반환
    /// SR 픽업 리스트가 있다면 무작위 선택, 없으면 기본값(-2) 반환.
    /// </summary>
    private (int ID, RandomWeaponData Data) GetSRResult(BannerData _banner)
    {
        return PickFromList(_banner.SR_PickupList, -2);
    }

    private (int ID, RandomWeaponData Data) GetSRResult(List<int> _list)
    {
        return PickFromList(_list, -2);
    }

    /// <summary>
    /// 픽업 선택
    /// </summary>
    private (int ID, RandomWeaponData Data) PickFromList(IReadOnlyList<int> _list, int _defaultValue)
    {
        if (_list != null && _list.Count > 0)
        {
            int idx = Random.Range(0, _list.Count);

            var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(_list[idx]);

            int damageResult = (int)Random.Range(weapon_selected.Damage_Min, weapon_selected.Damage_Max + 1);
            int defenseResult = (int)Random.Range(weapon_selected.Defense_Min, weapon_selected.Defense_Max + 1);

            RandomWeaponData data = new RandomWeaponData(damageResult, defenseResult);

            return (_list[idx], data);
        }
        return (_defaultValue, new RandomWeaponData());
    }

    /// <summary>
    /// R 결과 반환
    /// </summary>
    private (int ID, RandomWeaponData Data) GetRResult()
    {
        //int idx = Random.Range(0, CharacterManager.GetRareCharacterCount(Rare.R));
        var rList = Singleton.Get<TableDataManager>().GetItemIDsByRare(Rare.R);

        return PickFromList(rList, -1);
    }

    // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public async Task InitBannerDatas()
    {
        while (Singleton.Get<AuthManager>() == null)
        {
            await Task.Yield();
        }

        BannerWrapper wrapper = await Singleton.Get<AuthManager>().GetDataAsync<BannerWrapper>(Request.banners);
        BannerDatas = (wrapper).banners;

        Debug.Log($"<color=green>Banner: {BannerDatas.Count} Loaded</color>");
    }

    public async Task InitCount()
    {
        //db로 부터 결과값을 가져옴
        GachaResultWrapper result = await Singleton.Get<AuthManager>().GetDataAsync<GachaResultWrapper>(Request.readgachalog);

        if (result.GachaResultList == null || result.GachaResultList.Count == 0)
        {
            totalCount = 0;
            currentSRCount = 0;
            currentSSRCount = 0;
            pickupForce = false;
        }
        else
        {
            GachaResultContent latest = result.GachaResultList[result.GachaResultList.Count - 1];
            totalCount = latest.TotalCount;
            currentSRCount = latest.SRCurrentCount;
            currentSSRCount = latest.SSRCurrentCount;
            pickupForce = latest.PickupForce;
        }

        Debug.Log($"가챠 기록 {{\r\n  \"banners\": [\r\n    {{\r\n      \"BannerType\": \"Pickup\",\r\n      \"BannerName\": 100000001,\r\n      \"SSR_PickupList\": [10004],\r\n      \"SR_PickupList\": [10001, 10006],\r\n      \"SSR_Percent\": 1,\r\n      \"SR_Percent\": 19,\r\n      \"SSR_PickupPercent\": 50,\r\n      \"CharacterPosition\": {{ \"x\": 0, \"y\": 0 }},\r\n      \"BannerPath\": \"cecilia_banner\",\r\n      \"BackgroundPath\": \"cecilia_bg\",\r\n      \"SinglePrice\": 1200,\r\n      \"TenPrice\": 12000\r\n    }},\r\n    {{\r\n      \"BannerType\": \"None\",\r\n      \"BannerName\": 100000002,\r\n      \"SSR_PickupList\": [],\r\n      \"SR_PickupList\": [],\r\n      \"SSR_Percent\": 1,\r\n      \"SR_Percent\": 19,\r\n      \"SSR_PickupPercent\": 0,\r\n      \"CharacterPosition\": {{ \"x\": 0, \"y\": 0 }},\r\n      \"BannerPath\": \"regular_banner\",\r\n      \"BackgroundPath\": \"regular_bg\",\r\n      \"SinglePrice\": 1200,\r\n      \"TenPrice\": 10600\r\n    }}\r\n  ]\r\n}}{{\r\n  \"banners\": [\r\n    {{\r\n      \"BannerType\": \"Pickup\",\r\n      \"BannerName\": 100000001,\r\n      \"SSR_PickupList\": [10004],\r\n      \"SR_PickupList\": [10001, 10006],\r\n      \"SSR_Percent\": 1,\r\n      \"SR_Percent\": 19,\r\n      \"SSR_PickupPercent\": 50,\r\n      \"CharacterPosition\": {{ \"x\": 0, \"y\": 0 }},\r\n      \"BannerPath\": \"cecilia_banner\",\r\n      \"BackgroundPath\": \"cecilia_bg\",\r\n      \"SinglePrice\": 1200,\r\n      \"TenPrice\": 12000\r\n    }},\r\n    {{\r\n      \"BannerType\": \"None\",\r\n      \"BannerName\": 100000002,\r\n      \"SSR_PickupList\": [],\r\n      \"SR_PickupList\": [],\r\n      \"SSR_Percent\": 1,\r\n      \"SR_Percent\": 19,\r\n      \"SSR_PickupPercent\": 0,\r\n      \"CharacterPosition\": {{ \"x\": 0, \"y\": 0 }},\r\n      \"BannerPath\": \"regular_banner\",\r\n      \"BackgroundPath\": \"regular_bg\",\r\n      \"SinglePrice\": 1200,\r\n      \"TenPrice\": 10600\r\n    }}\r\n  ]\r\n}}{{\r\n  \"banners\": [\r\n    {{\r\n      \"BannerType\": \"Pickup\",\r\n      \"BannerName\": 100000001,\r\n      \"SSR_PickupList\": [10004],\r\n      \"SR_PickupList\": [10001, 10006],\r\n      \"SSR_Percent\": 1,\r\n      \"SR_Percent\": 19,\r\n      \"SSR_PickupPercent\": 50,\r\n      \"CharacterPosition\": {{ \"x\": 0, \"y\": 0 }},\r\n      \"BannerPath\": \"cecilia_banner\",\r\n      \"BackgroundPath\": \"cecilia_bg\",\r\n      \"SinglePrice\": 1200,\r\n      \"TenPrice\": 12000\r\n    }},\r\n    {{\r\n      \"BannerType\": \"None\",\r\n      \"BannerName\": 100000002,\r\n      \"SSR_PickupList\": [],\r\n      \"SR_PickupList\": [],\r\n      \"SSR_Percent\": 1,\r\n      \"SR_Percent\": 19,\r\n      \"SSR_PickupPercent\": 0,\r\n      \"CharacterPosition\": {{ \"x\": 0, \"y\": 0 }},\r\n      \"BannerPath\": \"regular_banner\",\r\n      \"BackgroundPath\": \"regular_bg\",\r\n      \"SinglePrice\": 1200,\r\n      \"TenPrice\": 10600\r\n    }}\r\n  ]\r\n}}로드된 최종 결과\ntotal:{totalCount}, sr:{currentSRCount}, ssr:{currentSSRCount}, pickupForce:{pickupForce}");
    }
}
