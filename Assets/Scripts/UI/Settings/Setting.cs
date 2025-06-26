using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Setting : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text soundTitle;
    [SerializeField] private SettingSlider master;
    [SerializeField] private SettingSlider music;
    [SerializeField] private SettingSlider effect;
    [SerializeField] private SettingDropdown language;

    private void Awake()
    {
        InitSetting();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        SetLocale();
    }

    private void SetLocale()
    {
        var table = Singleton.Get<TableDataManager>().Table.Locale;
        var locale = GameManager.CurrentLocale;

        title.text = table.Get(10000025, locale);
        soundTitle.text = table.Get(10000026, locale);
        master.Name.text = table.Get(10000027, locale);
        music.Name.text = table.Get(10000028, locale);
        effect.Name.text = table.Get(10000029, locale);
        language.Name.text = table.Get(10000030, locale);
    }

    private void InitSetting()
    {
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < (int)Locale.Count; i++)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = ((Locale)i).ToString();
            options.Add(option);
        }

        language.Dropdown.AddOptions(options);
    }

}
