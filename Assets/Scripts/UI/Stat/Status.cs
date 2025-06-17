using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Status : MonoBehaviour
{
    [Header("Locale")]
    [SerializeField] private TMP_Text basicStatus;
    [SerializeField] private TMP_Text weaponFixed;

    [SerializeField] private StatColumn hp;
    [SerializeField] private StatColumn damage;
    [SerializeField] private StatColumn defense;
    [SerializeField] private StatColumn finalDamage;
    [SerializeField] private StatColumn finalDefense;

    private List<StatColumn> statusList = new List<StatColumn>();

    private void Awake()
    {
        statusList.Add(hp);
        statusList.Add(damage);
        statusList.Add(defense);
    }

    private void Start()
    {
        Singleton.Player.OnWeaponChanged += UpdateStatus;
        Singleton.Get<GameManager>().OnLocaleChanged += SetLocale;

        SetLocale();
        UpdateStatus();
    }

    private void SetLocale()
    {
        var table = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;

        basicStatus.text = table.Get(10000010, locale);
        weaponFixed.text = table.Get(10000013, locale);

        hp.Name.text = table.Get(10000001, locale);
        damage.Name.text = table.Get(10000002, locale);
        defense.Name.text = table.Get(10000003, locale);
        finalDamage.Name.text = table.Get(10000014, locale);
        finalDefense.Name.text = table.Get(10000042, locale);
    }

    public void UpdateStatus(WeaponItemInstance _instance = null)
    {
        var player = Singleton.Player;

        if (player == null) return;

        for (int i = 0; i < (int)StatType.Count; i++)
        {
            statusList[i].Value.text = player.CurrentStatus((StatType)i).ToString();
            statusList[i].Value.color = Color.white;
        }
        finalDamage.Value.text = player.GetCalculatedDamage(_instance).ToString();
        finalDamage.Value.color = Color.white;

        finalDefense.Value.text = player.GetCalculatedDefense(_instance).ToString();
        finalDefense.Value.color = Color.white;
    }

    public void UpdateStatus(Levels _tempLevels)
    {
        var player = Singleton.Player;

        if (player == null) return;

        for (int i = 0; i < (int)StatType.Count; i++)
        {
            int tempStatus = player.TempStatus((StatType)i, _tempLevels);
            if (player.CurrentStatus((StatType)i) != tempStatus)
                statusList[i].Value.color = Color.green;
            else
                statusList[i].Value.color = Color.white;

            statusList[i].Value.text = tempStatus.ToString();
        }

        //공격력
        int tempFinalDamage = player.GetCalculatedDamage(_tempLevels);
        if (player.GetCalculatedDamage() != tempFinalDamage)
            finalDamage.Value.color = Color.green;
        else
            finalDamage.Value.color = Color.white;

        finalDamage.Value.text = tempFinalDamage.ToString();

        //방어력
        int tempFinalDefense = player.GetCalculatedDefense(_tempLevels);
        if (player.GetCalculatedDefense() != tempFinalDefense)
            finalDefense.Value.color = Color.green;
        else
            finalDefense.Value.color = Color.white;

        finalDefense.Value.text = tempFinalDefense.ToString();
    }
}
