using System;
using UnityEngine;
using UnityEngine.UI;

public enum PlateType
{
    Setting,
    Title,
    Login
}

public class TitleUI : MonoBehaviour
{
    [SerializeField] private SettingPlate settingPlate;
    [SerializeField] private TitlePlate titlePlate;
    [SerializeField] private LoginPlate loginPlate;

    [SerializeField] private VCams titleVCams;

    private void Awake()
    {
        loginPlate.OnLogined += titlePlate.ActivateEnterButton;

        settingPlate.OnClickArrow += ArrowClicked;
        titlePlate.OnClickArrow += ArrowClicked;
        loginPlate.OnClickArrow += ArrowClicked;
    }

    private void OnDestroy()
    {
        loginPlate.OnLogined -= titlePlate.ActivateEnterButton;

        settingPlate.OnClickArrow -= ArrowClicked;
        titlePlate.OnClickArrow -= ArrowClicked;
        loginPlate.OnClickArrow -= ArrowClicked;
    }

    private void ArrowClicked(int _index)
    {
        titleVCams.SetCam((PlateType)_index);
    }
}
