using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using GameStuff;

public class LevelUPColumn : MonoBehaviour
{
    public TMP_Text Name;
    public LevelType Type;
    public TMP_Text beforeLevel;
    public TMP_Text afterLevel;
    public Button increaseButton;
    public Button decreaseButton;

    public event Action<LevelUPColumn, bool> OnIncreaseButtonClicked;
    public event Action<LevelUPColumn, bool> OnDecreaseButtonClicked;

    private void Awake()
    {
        increaseButton.onClick.AddListener(() => OnIncreaseButtonClicked?.Invoke(this, true));
        decreaseButton.onClick.AddListener(() => OnDecreaseButtonClicked?.Invoke(this, false));
    }
}
