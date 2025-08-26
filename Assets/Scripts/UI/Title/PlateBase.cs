using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PlateBase : MonoBehaviour
{
    protected List<ArrowButton> buttons = new List<ArrowButton>();

    public event Action<int> OnClickArrow;

    protected virtual void Awake()
    {
        buttons = GetComponentsInChildren<ArrowButton>().ToList();

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].Clicked += (index) => { OnClickArrow?.Invoke(index); };
        }
    }
}
