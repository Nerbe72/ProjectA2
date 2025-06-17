using UnityEngine;

public class SettingUI : WindowBase
{
    public int InitializationPriority => 7;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        WindowType = WindowType.SettingWindow;

        gameObject.SetActive(false);
    }
}
