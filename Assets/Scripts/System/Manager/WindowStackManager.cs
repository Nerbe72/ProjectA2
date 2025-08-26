using System.Collections.Generic;
using UnityEngine;

using GameStuff;

public static class WindowStackManager
{
    private static Stack<IWindowStack> sceneStack = new Stack<IWindowStack>();

    public static void PopAllWindows()
    {
        while (sceneStack.Count >= 1)
        {
            PopWindow();
        }

        PeekAllWindows();
    }

    public static void Init()
    {
        Player.OnUIKeyDown += Test;
    }

    public static void Test(KeyCode _key)
    {
        switch (_key)
        {
            case KeyCode.I:
                {
                    var top = WindowStackManager.PeekTopWindow();
                    var window = Singleton.Inventory;

                    var topWindowBase = top as WindowBase;
                    if (window.gameObject.activeSelf && topWindowBase != null && topWindowBase.gameObject == window.gameObject)
                        WindowStackManager.PopWindow();
                    else if (!window.gameObject.activeSelf)
                        (window as IWindowStack)?.ShowWindow();
                }
                break;
            case KeyCode.O:
                {
                    if (Singleton.Get<GachaUI>().IsGachaRunning) return;
                    ShowUI<GachaUI>();
                }
                break;
            case KeyCode.P:
                ShowUI<QuestUI>();
                break;
            case KeyCode.B:
                ShowUI<ItemCreationDialogueWindow>();
                break;
            case KeyCode.U:
                ShowUI<PlayerStatusUI>();
                break;
            case KeyCode.V:
                ShowUI<EnhancementDialogWindow>();
                break;
        }
    }

    public static void ShowUI<T>() where T : MonoBehaviour, IWindowStack
    {
        var top = WindowStackManager.PeekTopWindow();
        var window = Singleton.Get<T>();
        var topWindowBase = top as WindowBase;
        if (window.gameObject.activeSelf && topWindowBase != null && topWindowBase.gameObject == window.gameObject)
            WindowStackManager.PopWindow();
        else if (!window.gameObject.activeSelf)
            (window as IWindowStack)?.ShowWindow();
    }

    public static void AddWindow(IWindowStack _window)
    {
        _window.SetSortingOrder(sceneStack.Count + 10);
        (_window as WindowBase).gameObject.SetActive(true);
        sceneStack.Push(_window);
        RefreshCursorState();
        PeekAllWindows();
    }

    public static IWindowStack PopWindow()
    {
        int count = sceneStack.Count;

        if (count <= 0)
        {
            return null;
        }

        WindowBase closeTarget = sceneStack.Pop() as WindowBase;
        closeTarget.gameObject.SetActive(false);

        RefreshCursorState();

        PeekAllWindows(); //표시용

        return closeTarget;
    }

    public static IWindowStack PeekAllWindows()
    {
        IWindowStack outWindow = null;

        if (sceneStack.TryPeek(out outWindow))
        {
            DebugStack();
            return outWindow;
        }

        DebugStack();
        return null;
    }

    public static IWindowStack PeekTopWindow()
    {
        IWindowStack outWindow = null;
        sceneStack.TryPeek(out outWindow);
        return outWindow;
    }

    public static int GetWindowCount()
    {
        return sceneStack.Count;
    }

    public static void SetAltHeld(bool _held)
    {
        // Forward to CursorManager
        CursorManager.SetAltHeld(_held);
    }

    private static void RefreshCursorState()
    {
        // Forward to CursorManager
        CursorManager.Refresh();
    }

    private static void DebugStack()
    {
        int i = sceneStack.Count - 1;
        foreach (var scene in sceneStack)
        {
            Debug.Log($"<color=magenta>{i}// {(scene as WindowBase).name} //</color>");
            i--;
        }
        Debug.Log($"<color=orange>////////////////////////////////////////////</color>");
    }
}
