using UnityEngine;

public static class UIManager
{
    public static void OffBasicUI()
    {
        if (Singleton.Get<StatusUI>() == null || Singleton.Get<MenuUI>() == null) return;

        Singleton.Get<StatusUI>().gameObject.SetActive(false);
        Singleton.Get<MenuUI>().gameObject.SetActive(false);
    }

    public static void OnBasicUI()
    {
        //게임 종료시 호출 이전에 오브젝트가 먼저 파괴된 경우 예외
        if (Singleton.Get<StatusUI>() == null || Singleton.Get<MenuUI>() == null) return;

        Singleton.Get<StatusUI>().gameObject.SetActive(true);
        Singleton.Get<MenuUI>().gameObject.SetActive(true);
    }
}
