using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour, IWindowStack
{
    public WindowType WindowType { get; set; }

    private Animator animator;
    private int showHash;

    [Header("Prefab")]
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("UI")]
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject choiceFrame;
    [SerializeField] private Button nextButton;

    private List<Button> choiceButtons = new List<Button>();


    private event Action<int> onChoiceSelected;
    private event Action onNext;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        showHash = Animator.StringToHash("Show");
        WindowType = WindowType.NormalWindow;

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        InputManager.IgnoreInput = true;
        InputManager.IgnoreUIInput = true;
        UIManager.OffBasicUI();
        Singleton.Get<InteractIndicator>().SetShowIndicator(false);

        animator.SetBool(showHash, true);
    }

    private void OnDisable()
    {
        InputManager.IgnoreInput = false;
        InputManager.IgnoreUIInput = false;

        UIManager.OnBasicUI();

        var indicator = Singleton.Get<InteractIndicator>();
        if (indicator != null)
            indicator.SetShowIndicator(true);

        if (animator != null)
            animator.SetBool(showHash, false);
    }

    public void Show(string _speaker, string _dialogue)
    {
        gameObject.SetActive(true);
        choiceFrame.SetActive(false);
        nextButton.gameObject.SetActive(true);

        speakerText.text = _speaker;
        dialogueText.text = _dialogue;
    }

    public void ShowChoice(List<(string text, int nextID)> _choices, Action<int> _onSelect)
    {
        choiceFrame.SetActive(true);
        nextButton.gameObject.SetActive(false);
        onChoiceSelected = _onSelect;

        for (int i = 0; i < _choices.Count; i++)
        {
            GameObject obj = Instantiate(choiceButtonPrefab, choiceFrame.transform);
            var button = obj.GetComponent<Button>();
            var buttonText = obj.GetComponentInChildren<TMP_Text>();
            buttonText.text = _choices[i].text;

            int index = i;
            button.onClick.AddListener(() => OnChoiceClick(index));
            choiceButtons.Add(button);
        }
    }

    private void OnChoiceClick(int _index)
    {
        choiceFrame.SetActive(false);
        onChoiceSelected?.Invoke(_index);
    }

    public void OnNext(Action _onNext)
    {
        onNext = _onNext;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            nextButton.interactable = false;
            nextButton.gameObject.SetActive(false);
            onNext?.Invoke();
        });
        nextButton.interactable = true;
        nextButton.gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);

        int count = choiceButtons.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            choiceButtons[i].onClick.RemoveAllListeners();
            Destroy(choiceButtons[i].gameObject);
        }
        choiceButtons.Clear();

        onChoiceSelected = null;
        onNext = null;
    }
}
