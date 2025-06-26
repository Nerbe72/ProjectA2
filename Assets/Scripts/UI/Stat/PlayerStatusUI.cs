public class PlayerStatusUI : WindowBase
{
    private Level level;
    private Status basicStatus;

    public int InitializationPriority => 2;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        level = GetComponentInChildren<Level>();
        basicStatus = GetComponentInChildren<Status>();

        WindowType = WindowType.NormalWindow;

        gameObject.SetActive(false);
    }
}
