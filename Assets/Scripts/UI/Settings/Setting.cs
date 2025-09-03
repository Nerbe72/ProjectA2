using System.Collections.Generic;
using TMPro;
using UnityEngine;

using GameStuff;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text soundTitle;
    [SerializeField] private SettingSlider master;
    [SerializeField] private SettingSlider music;
    [SerializeField] private SettingSlider effect;
    [SerializeField] private SettingDropdown language;
    [SerializeField] private Button exitButton;

    private string MasterVolumeKey = "MasterVolume";
    private string MusicVolumeKey = "MusicVolume";
    private string EffectVolumeKey = "EffectVolume";

    private void Start()
    {
        Singleton.Get<GameManager>().OnLocaleChanged += SetLocale;
        SetLocale();
        InitSetting();
    }

    //private void Update()
    //{
    //    if (Camera.main == null) return;
    //    transform.position = Camera.main.transform.position;
    //}

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
        // exitButton.GetComponentInChildren<TMP_Text>().text = table.Get(1, locale);
    }

    private void InitSetting()
    {
        if (!PlayerPrefs.HasKey(MasterVolumeKey))
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, 50);
        }
        master.Slider.value = PlayerPrefs.GetFloat(MasterVolumeKey);
        master.Slider.onValueChanged.AddListener((value) => SliderValueChanged(0, value));

        if (!PlayerPrefs.HasKey(MusicVolumeKey))
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, 100);
        }
        music.Slider.value = PlayerPrefs.GetFloat(MusicVolumeKey);
        music.Slider.onValueChanged.AddListener((value) => SliderValueChanged(1, value));

        if (!PlayerPrefs.HasKey(EffectVolumeKey))
        {
            PlayerPrefs.SetFloat(EffectVolumeKey, 100);
        }
        effect.Slider.value = PlayerPrefs.GetFloat(EffectVolumeKey);
        effect.Slider.onValueChanged.AddListener((value) => SliderValueChanged(2, value));

        // 
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        int count = (int)Locale.Count;
        for (int i = 0; i < count; i++)
        {
            options.Add(new TMP_Dropdown.OptionData(((Locale)i).ToString()));
        }

        language.Dropdown.options = options;

        language.Dropdown.onValueChanged.AddListener((value) =>
        {
            Locale selectedLocale = (Locale)value;

            Singleton.Get<GameManager>().ChangeLocale(selectedLocale);
        });

        exitButton.onClick.AddListener(() => { GameManager.ShowExit(); });

        // 초기 볼륨 설정
        ApplyVolumeSettings();
    }

    private void ApplyVolumeSettings()
    {
        float masterVolume = (PlayerPrefs.GetFloat(MasterVolumeKey) / 100f);
        float bgmVolume = (PlayerPrefs.GetFloat(MusicVolumeKey) / 100f);
        float effectVolume = (PlayerPrefs.GetFloat(EffectVolumeKey) / 100f);
        
        var soundManager = Singleton.Get<SoundManager>();
        if (soundManager != null)
        {
            soundManager.SetBGMVolume(masterVolume * bgmVolume);
            soundManager.SetEffectVolume(masterVolume * effectVolume);
        }
    }

    private void SliderValueChanged(int _index, float _value)
    {
        VolumeType type = (VolumeType)_index;

        switch(type)
        {
            case VolumeType.Master:
                PlayerPrefs.SetFloat(MasterVolumeKey, master.Slider.value);
                break;
            case VolumeType.Music:
                PlayerPrefs.SetFloat(MusicVolumeKey, music.Slider.value);
                break;
            case VolumeType.Effect:
                PlayerPrefs.SetFloat(EffectVolumeKey, effect.Slider.value);
                break;
        }

        float masterVolume = (PlayerPrefs.GetFloat(MasterVolumeKey) / 100f);
        float bgmVolume = (PlayerPrefs.GetFloat(MusicVolumeKey) / 100f);
        float effectVolume = (PlayerPrefs.GetFloat(EffectVolumeKey) / 100f);
        Singleton.Get<SoundManager>().SetBGMVolume(masterVolume * bgmVolume);
        Singleton.Get<SoundManager>().SetEffectVolume(masterVolume * effectVolume);
    }
}
