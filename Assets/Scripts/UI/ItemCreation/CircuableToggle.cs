using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("CustomUI/CircularToggle", 0)]
[RequireComponent(typeof(EventTrigger))]
public class CircuableToggle : Toggle
{
    [SerializeField] private bool isCircuable = true;
    [SerializeField] private Image icon;
    [SerializeField] private List<Sprite> cycleIcons;
    [SerializeField] private int currentCycle = 0;

    public bool IsCircuable { get => isCircuable; }
    public List<Sprite> CycleIcons
    {
        get => cycleIcons;
        set
        {
            if (value == null || value.Count == 0)
                return;
            cycleIcons = value;
            if (currentCycle >= cycleIcons.Count)
                currentCycle = 0;
            icon.sprite = cycleIcons[currentCycle];
        }
    }
    public int CurrentCycle
    {
        get => currentCycle;
        set
        {
            if (value < 0 || value >= cycleIcons.Count)
                return;
            currentCycle = value;
        }
    }
    public Image Icon { get => icon;}
}
