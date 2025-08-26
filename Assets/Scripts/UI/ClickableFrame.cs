using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using GameStuff;

public class ClickableFrame : FrameBase, IPointerClickHandler
{
    protected Toggle self;
    
    protected bool selected;
    
    public event Action<ItemInstance> OnFrameSelected;
    public event Action<ItemInstance, RectTransform> OnRightClick;

    protected virtual void Awake()
    {
        self = GetComponent<Toggle>();
        self.onValueChanged.AddListener(ChangeSelected);
    }

    private void OnDestroy()
    {
        self.onValueChanged.RemoveAllListeners();
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        OnRightClick?.Invoke(instance, GetComponent<RectTransform>());
    }

    protected virtual void ChangeSelected(bool _selected)
    {
        selected = _selected;
        OnFrameSelected?.Invoke(instance);
    }
}
