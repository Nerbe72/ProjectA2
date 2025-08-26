using GameStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Filter : MonoBehaviour
{
    private ToggleGroup filterGroup;

    private List<CircuableToggle> filterToggles;

    public event Action<int, int> OnFilterSelected;
    
    public int GetCurrentFilterIndex()
    {
        for (int i = 0; i < filterToggles.Count; i++)
        {
            if (filterToggles[i].isOn)
            {
                return i;
            }
        }
        return filterToggles.Count - 1;
    }
    
    public int GetCurrentFilterSubIndex()
    {
        int currentIndex = GetCurrentFilterIndex();
        return filterToggles[currentIndex].CurrentCycle;
    }

    private void Awake()
    {
        filterGroup = GetComponent<ToggleGroup>();
        filterToggles = filterGroup.GetComponentsInChildren<CircuableToggle>().ToList();

        InitFilter();
    }

    private void InitFilter()
    {
        for (int i = 0; i < filterToggles.Count; i++)
        {
            var index = i;
            filterToggles[index].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    OnFilterSelected?.Invoke(index, filterToggles[index].CurrentCycle);
                }
            });

            if (filterToggles[index].IsCircuable)
            {
                var eventTrigger = filterToggles[index].GetComponent<EventTrigger>();
                var entry = new EventTrigger.Entry();

                entry.eventID = EventTriggerType.PointerDown;
                entry.callback.AddListener((data) =>
                {
                    if (filterToggles[index].isOn)
                    {
                        CycleFilter(index);
                        OnFilterSelected?.Invoke(index, filterToggles[index].CurrentCycle);
                    }
                });

                eventTrigger.triggers.Add(entry);
            }
        }


        // 초기화
        for (int i = 0; i < filterToggles.Count; i++)
        {
            filterToggles[i].group = filterGroup;
            filterToggles[i].isOn = i == filterToggles.Count - 1;
        }
    }

    private void CycleFilter(int _index)
    {
        var target = filterToggles[_index];

        if (target.CycleIcons.Count == 0) return;
        
        target.CurrentCycle = (target.CurrentCycle + 1) % target.CycleIcons.Count;

        //체크박스 색상
        Image weaponFilterImage = target.graphic.GetComponent<Image>();
        if (weaponFilterImage == null) return;

        // 아이콘 변경
        Image weaponFilterIcon = target.Icon;
        Sprite weaponFilterSprite = target.CycleIcons[target.CurrentCycle];

        if (weaponFilterSprite != null)
        {
            weaponFilterIcon.sprite = weaponFilterSprite;
        }
        else
        {
            weaponFilterIcon.color = Color.clear;
        }
    }
}
