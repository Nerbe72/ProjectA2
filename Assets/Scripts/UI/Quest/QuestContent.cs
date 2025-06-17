using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestContent : MonoBehaviour
{
    [SerializeField] private TMP_Text State;
    [SerializeField] private TMP_Text Name;
    [SerializeField] private Toggle Toggle;
    [SerializeField] private Button TargetButton;

    public void SetHeader(string _state)
    {
        State.text = _state;
    }

    public void SetData(string _state, string _name, bool _isAlreadyCompleted, UnityAction _onButtonAction, UnityAction<bool> _onToggleAction)
    {
        State.text = _state;
        Name.text = _name;
        TargetButton.onClick.RemoveAllListeners();
        TargetButton.gameObject.SetActive(!_isAlreadyCompleted);
        TargetButton.onClick.AddListener(_onButtonAction);
        Toggle.onValueChanged.AddListener(_onToggleAction);
    }
}
