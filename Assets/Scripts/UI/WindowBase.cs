using UnityEngine;

using GameStuff;

/// <summary>
/// 반드시 창의 사용처에 맞춰 WindowType을 설정할것
/// </summary>
public abstract class WindowBase : MonoBehaviour, IWindowStack
{
    public GameObject Self { get; set; }
    public WindowType WindowType { get; set; }

    public event System.Action OnWindowClosed;

    protected virtual void OnEnable()
    {
        Self = gameObject;
    }

    protected virtual void OnDisable()
    {
        OnWindowClosed?.Invoke();
        OnWindowClosed = null;
    }
}
