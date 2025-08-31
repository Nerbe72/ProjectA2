using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tab : MonoBehaviour
{
    public int localeID;
    public TMP_Text Name;

    private int index;
    private Animator animator;
    private Toggle toggle;

    public Action<int> OnTabSelected;

    private void Awake()
    {
        index = transform.GetSiblingIndex();
        animator = GetComponent<Animator>();
        toggle = GetComponent<Toggle>();
        toggle.group = GetComponentInParent<ToggleGroup>();

        RegisterEventTrigger();

        toggle.onValueChanged.AddListener((isOn) =>
        {
            animator.SetBool("Selected", isOn);
            OnTabSelected?.Invoke(index);
        });
    }

    private void Start()
    {
        if (toggle.isOn)
        {
            animator.SetBool("Selected", toggle.isOn);
            OnTabSelected?.Invoke(index);
        }
        //Singleton.Get<GameManager>().OnLocaleChanged += UpdateLocale;
    }

    private void OnEnable()
    {
        //UpdateLocale();
    }

    private void UpdateLocale()
    {
        var table = Singleton.Get<TableDataManager>().Table;

        if (table == null)
        {
            Debug.LogError("TableDataManager's Table is null. Cannot update locale.");
            return;
        }

        Name.text = table.Locale.Get(localeID, GameManager.CurrentLocale);
    }

    private void RegisterEventTrigger()
    {
        var eventTrigger = GetComponent<EventTrigger>();

        var entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) =>
        {
            if (animator != null)
            {
                animator.SetBool("Hover", true);
            }
        });

        var entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) =>
        {
            if (animator != null)
            {
                animator.SetBool("Hover", false);
            }
        });

        eventTrigger.triggers.Add(entryEnter);
        eventTrigger.triggers.Add(entryExit);
    }
}
