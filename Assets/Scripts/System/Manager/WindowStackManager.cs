using System.Collections.Generic;
using UnityEngine;

public static class WindowStackManager
{
    private static Stack<IWindowStack> sceneStack = new Stack<IWindowStack>();

    public static void ResetWindowStack()
    {
        while (sceneStack.Count >= 1)
        {
            PopWindow();
        }

        PeekAllWindows();
    }

    public static void AddWindow(IWindowStack _window)
    {
        _window.SetSortingOrder(sceneStack.Count + 10);
        (_window as WindowBase).gameObject.SetActive(true);
        InputManager.IgnoreInput = true;
        sceneStack.Push(_window);
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

        if (sceneStack.Count == 0)
        {
            if (closeTarget.WindowType != WindowType.DialogueWindow)
                InputManager.IgnoreInput = false;
        }

        PeekAllWindows();

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
