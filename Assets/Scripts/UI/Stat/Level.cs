using UnityEngine;
using TMPro;
using NUnit.Framework;
using System.Transactions;
using System.Collections.Generic;

public class Level : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private StatColumn totalLevel;
    [SerializeField] private StatColumn remainedCoin;
    [SerializeField] private StatColumn requiredCoin;
    [SerializeField] private StatColumn health;
    [SerializeField] private StatColumn strength;
    [SerializeField] private StatColumn dexterity;
    [SerializeField] private StatColumn intelligent;

    private List<StatColumn> levelColumnList = new List<StatColumn>();

    private void Awake()
    {
        levelColumnList.Add(health);
        levelColumnList.Add(strength);
        levelColumnList.Add(dexterity);
        levelColumnList.Add(intelligent);
    }

    private void Start()
    {
        Singleton.Inventory.OnCurrencyChanged += UpdateCoin;
        Singleton.Get<GameManager>().OnLocaleChanged += SetLocale;

        SetLocale();
        UpdateLevel();
    }

    private void OnDestroy()
    {
        Singleton.Inventory.OnCurrencyChanged -= UpdateCoin;
    }

    private void SetLocale()
    {
        var table = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;

        title.text = table.Get(10000009, locale);

        totalLevel.Name.text = table.Get(10000000, locale);

        if (remainedCoin != null)
            remainedCoin.Name.text = table.Get(10000011, locale);

        if (requiredCoin != null)
            requiredCoin.Name.text = table.Get(10000012, locale);

        health.Name.text = table.Get(10000005, locale);
        strength.Name.text = table.Get(10000006, locale);
        dexterity.Name.text = table.Get(10000007, locale);
        intelligent.Name.text = table.Get(10000008, locale);
    }

    public void UpdateLevel()
    {
        var player = Singleton.Player;

        if (player == null) return;

        for (int i = 1; i < (int)LevelType.Count; i++)
        {
            int current = player.GetCurrentLevel((LevelType)i);
            levelColumnList[i - 1].Value.text = current.ToString();
            levelColumnList[i - 1].Value.color = Color.white;
        }

        int total = player.GetCurrentLevel(LevelType.Total);
        totalLevel.Value.text = total.ToString();
        totalLevel.Value.color = Color.white;

        if (remainedCoin != null)
            UpdateCoin(Singleton.Inventory.GetCurrency());

        if (requiredCoin != null)
            UpdateRequiredCoin(total);
    }

    public void TempUpdateLevel(Levels _tempLevels)
    {
        var player = Singleton.Player;

        if (player == null) return;

        for (int i = 1; i < (int)LevelType.Count; i++)
        {
            int current = player.GetCurrentLevel((LevelType)i);
            int temp = _tempLevels.Data[(LevelType)i];
            levelColumnList[i - 1].Value.text = temp.ToString();

            if (current != temp)
                levelColumnList[i - 1].Value.color = Color.green;
            else
                levelColumnList[i - 1].Value.color = Color.white;
        }

        int currentTotal = player.GetCurrentLevel(LevelType.Total);
        int tempTotal = _tempLevels.GetTotal();

        totalLevel.Value.text = tempTotal.ToString();

        if (tempTotal != currentTotal)
            totalLevel.Value.color = Color.green;
        else
            totalLevel.Value.color = Color.white;
    }

    public void UpdateCoin(uint currentCoin)
    {
        if (remainedCoin != null)
            remainedCoin.Value.text = currentCoin.ToString();
    }

    public void UpdateRequiredCoin(int _currentLevel, int _difference = 0)
    {
        int totalRequired = 0;
        int sumLevel = _currentLevel + _difference;

        int i = _currentLevel;

        do
        {
            totalRequired += Singleton.Get<TableDataManager>().Table.RequireCurrency.Get(i).RequireCurrency;
            i++;
        } while (i < sumLevel);

        requiredCoin.Value.text = totalRequired.ToString();
    }
}
