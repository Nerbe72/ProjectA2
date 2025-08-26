using GameStuff;

public class SettingUI : WindowBase
{
    private Setting setting;

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

        setting = GetComponentInChildren<Setting>(true);
        setting.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
