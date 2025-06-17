using UnityEngine;

public interface IWindowStack
{
    public WindowType WindowType { get; set; }
    public void ShowWindow()
    {
        WindowStackManager.AddWindow(this);
    }

    public void SetSortingOrder(int order)
    {
        if ((this as WindowBase).GetComponent<Canvas>() == null) return;

        (this as WindowBase).GetComponent<Canvas>().sortingOrder = order;
    }
}