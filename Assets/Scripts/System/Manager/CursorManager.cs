using UnityEngine;
using GameStuff;

public static class CursorManager
{
    private static bool altHeld;
    private static int dialogueRefCount; // 실제 대화 진행 중 참조 카운트

    public static void SetAltHeld(bool held)
    {
        if (altHeld == held) return;
        altHeld = held;
        Refresh();
    }

    // 실제 대화가 열리는 순간 호출
    public static void DialogueOpen()
    {
        dialogueRefCount++;
        Refresh();
    }

    // 실제 대화가 닫히는 순간 호출
    public static void DialogueClose()
    {
        if (dialogueRefCount > 0) dialogueRefCount--;
        Refresh();
    }

    public static void Refresh()
    {
        int count = WindowStackManager.GetWindowCount();
        bool hasWindows = count > 0;

        WindowBase top = WindowStackManager.PeekTopWindow() as WindowBase;
        bool isDialogueWindowTop = top != null && top.WindowType == WindowType.DialogueWindow;

        bool isAltHeld = altHeld;
        bool isDialogueActive = dialogueRefCount > 0;

        // 병렬(OR) 판정: 하나라도 true면 커서 표시
        bool showCursor = hasWindows || isDialogueWindowTop || isAltHeld || isDialogueActive;

        CursorLockMode desiredLock = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        bool desiredVisible = showCursor;
        bool desiredIgnoreInput = showCursor; // 커서가 보이면 플레이어 입력 차단

        if (Cursor.lockState != desiredLock || Cursor.visible != desiredVisible)
        {
            Cursor.lockState = desiredLock;
            Cursor.visible = desiredVisible;
            Debug.Log($"<color=cyan>[CursorManager] Cursor refresh -> lock:{desiredLock}, visible:{desiredVisible}, windows:{count}, dialogueTop:{isDialogueWindowTop}, altHeld:{isAltHeld}, dialogueActive:{isDialogueActive}, dialogueRefs:{dialogueRefCount}, showCursor:{showCursor}</color>");
        }

        if (InputManager.IgnoreInput != desiredIgnoreInput)
        {
            InputManager.IgnoreInput = desiredIgnoreInput;
        }
    }
}
