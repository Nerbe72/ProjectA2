using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using GameStuff;

public class LevelUP : MonoBehaviour
{
    [SerializeField] private TMP_Text title;

    [SerializeField] private StatColumn totalLevel;
    [SerializeField] private StatColumn remainedCoin;
    [SerializeField] private StatColumn requiredNextCoin;
    [SerializeField] private StatColumn requiredTotalCoin;

    [SerializeField] private LevelUPColumn health;
    [SerializeField] private LevelUPColumn strength;
    [SerializeField] private LevelUPColumn dexterity;
    [SerializeField] private LevelUPColumn intelligent;
    [SerializeField] private Button applyButton;

    [SerializeField] private float RemainedLerpTimeMultiply;

    private Levels tempLevels = new Levels();
    private uint tempPrice = 0;
    private List<LevelUPColumn> ColumnLevelUPList = new List<LevelUPColumn>();

    public event Action<Levels> OnTempLevelChanged;
    public event Action OnApplyClicked;

    private void Awake()
    {
        ColumnLevelUPList.Add(health);
        ColumnLevelUPList.Add(strength);
        ColumnLevelUPList.Add(dexterity);
        ColumnLevelUPList.Add(intelligent);

        health.OnIncreaseButtonClicked += OnTempChanged;
        strength.OnIncreaseButtonClicked += OnTempChanged;
        dexterity.OnIncreaseButtonClicked += OnTempChanged;
        intelligent.OnIncreaseButtonClicked += OnTempChanged;

        health.OnDecreaseButtonClicked += OnTempChanged;
        strength.OnDecreaseButtonClicked += OnTempChanged;
        dexterity.OnDecreaseButtonClicked += OnTempChanged;
        intelligent.OnDecreaseButtonClicked += OnTempChanged;

        applyButton.onClick.AddListener(ApplyTempToCurrent);
    }

    private void Start()
    {
        Singleton.Get<GameManager>().OnLocaleChanged += SetLocale;
        Singleton.Player.OnLevelChanged += UpdateLevel;
        Singleton.Inventory.OnCurrencyChanged += UpdateRemainedCoin;

        SetLocale();
        UpdateLevel();
    }

    private void OnDestroy()
    {
        health.OnIncreaseButtonClicked -= OnTempChanged;
        strength.OnIncreaseButtonClicked -= OnTempChanged;
        dexterity.OnIncreaseButtonClicked -= OnTempChanged;
        intelligent.OnIncreaseButtonClicked -= OnTempChanged;

        health.OnDecreaseButtonClicked -= OnTempChanged;
        strength.OnDecreaseButtonClicked -= OnTempChanged;
        dexterity.OnDecreaseButtonClicked -= OnTempChanged;
        intelligent.OnDecreaseButtonClicked -= OnTempChanged;

        applyButton.onClick.RemoveAllListeners();
    }

    private void SetLocale()
    {
        var table = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;

        title.text = table.Get(10000023, locale);

        totalLevel.Name.text = table.Get(10000000, locale);
        remainedCoin.Name.text = table.Get(10000011, locale);
        requiredNextCoin.Name.text = table.Get(10000012, locale);
        requiredTotalCoin.Name.text = table.Get(10000024, locale);
        health.Name.text = table.Get(10000005, locale);
        strength.Name.text = table.Get(10000006, locale);
        dexterity.Name.text = table.Get(10000007, locale);
        intelligent.Name.text = table.Get(10000008, locale);
    }

    /// <summary>
    /// 레벨 초기화
    /// </summary>
    public void UpdateLevel()
    {
        var player = Singleton.Player;

        if (player == null) return;

        for (int i = 1; i < (int)LevelType.Count; i++)
        {
            int level = player.GetCurrentLevel((LevelType)i);
            tempLevels.Data[(LevelType)i] = player.GetCurrentLevel((LevelType)i);
            ColumnLevelUPList[i - 1].beforeLevel.text = level.ToString();
            ColumnLevelUPList[i - 1].afterLevel.text = level.ToString();
            ColumnLevelUPList[i - 1].afterLevel.color = Color.white;
        }

        totalLevel.Value.text = player.CurrentLevels.GetTotal().ToString();
        UpdateRemainedCoin(Singleton.Inventory.GetCurrency());
        UpdateRequiredCoin(player.CurrentLevels.GetTotal());
    }

    /// <summary>
    /// temp 변화 적용
    /// </summary>
    /// <param name="_column"></param>
    /// <param name="_increase"></param>
    private void OnTempChanged(LevelUPColumn _column, bool _increase)
    {
        int currentTotal = Singleton.Player.CurrentLevels.GetTotal();
        int tempTotal = tempLevels.GetTotal();

        int currentAfter = tempLevels.Data[_column.Type];
        int applying = 0;
        if (_increase)
        {
            if (tempTotal + 1 > 300) return;

            uint currentCoin = Singleton.Inventory.GetCurrency();
            uint requiredCoin = GetRequiredCoin(currentTotal, tempTotal + 1 - currentTotal);

            if (requiredCoin > currentCoin) return;

            if (currentCoin >= requiredCoin)
            {
                applying = 1;
            }
        }
        else
        {
            if (currentAfter > 0 && int.Parse(_column.beforeLevel.text) < currentAfter)
            {
                applying = -1;
            }
        }

        _column.afterLevel.text = (currentAfter + applying).ToString();

        if (_column.beforeLevel.text != _column.afterLevel.text)
        {
            _column.afterLevel.color = Color.green;
        }
        else
        {
            _column.afterLevel.color = Color.white;
        }

        tempPrice = GetRequiredCoin(currentTotal, tempTotal + applying - currentTotal);
        UpdateRequiredCoin(currentTotal, tempTotal + applying - currentTotal);
        tempLevels.Data[_column.Type] += applying;

        OnTempLevelChanged?.Invoke(tempLevels);
    }

    public void UpdateRemainedCoin(uint _currentCoin)
    {
        if (!gameObject.activeInHierarchy)
        {
            remainedCoin.Value.text = _currentCoin.ToString();
            return;
        }
        StopAllCoroutines();
        StartCoroutine(RemainedCoinLerp(_currentCoin));
    }

    public void UpdateRequiredCoin(int _currentLevel, int _difference = 0)
    {
        uint nextRequired = GetNextRequiredCoin(_currentLevel + _difference);
        uint totalRequired = GetRequiredCoin(_currentLevel, _difference);
        uint remain = Singleton.Inventory.GetCurrency() - totalRequired;

        if (remain < nextRequired)
            requiredNextCoin.Value.color = Color.red;
        else
            requiredNextCoin.Value.color = Color.white;

        requiredNextCoin.Value.text = nextRequired.ToString();
        requiredTotalCoin.Value.text = totalRequired.ToString();
    }

    private uint GetNextRequiredCoin(int _tempLevel)
    {
        return (uint)Singleton.Get<TableDataManager>().Table.RequireCurrency.Get(_tempLevel).RequireCurrency;
    }

    private uint GetRequiredCoin(int _currentLevel, int _difference = 0)
    {
        uint totalRequired = 0;

        int sumLevel = _currentLevel + _difference;

        for (int i = _currentLevel; i < sumLevel; i++)
        {
            totalRequired += (uint)Singleton.Get<TableDataManager>().Table.RequireCurrency.Get(i).RequireCurrency;
        }

        return totalRequired;
    }

    private void ApplyTempToCurrent()
    {
        Singleton.Player.LevelUp(tempLevels);
        Singleton.Inventory.MinusCurrency(tempPrice);
        OnApplyClicked?.Invoke();
    }

    private IEnumerator RemainedCoinLerp(uint _after)
    {
        uint before = uint.Parse(remainedCoin.Value.text);
        uint after = _after;

        float time = 0f;
        while (true)
        {
            time += Time.deltaTime * RemainedLerpTimeMultiply;

            remainedCoin.Value.text = ((int)Mathf.Lerp(before, after, time)).ToString();

            if (time >= 1f) break;
            yield return null;
        }

        remainedCoin.Value.text = after.ToString();
        yield break;
    }
}
