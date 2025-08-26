using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Exit : MonoBehaviour
{
    [SerializeField] private TMP_Text exitText;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (Singleton.exit != null)
        {
            Destroy(gameObject);
            return;
        }

        Singleton.exit = this;
        DontDestroyOnLoad(gameObject);

        exitButton.onClick.AddListener( () => { GameManager.ExitGame(); });
        gameObject.SetActive(false);
    }

    private void Start()
    {
        Singleton.Get<GameManager>().OnLocaleChanged += UpdateLocale;
    }

    private void OnEnable()
    {
        UpdateLocale();
    }

    private void UpdateLocale()
    {
        var table = Singleton.Get<TableDataManager>()?.Table;

        if (table == null)
        {
            Debug.LogError("Exit: table error");
            return;
        }

        var tableLocale = table.Locale;
        var locale = GameManager.CurrentLocale;

        exitText.text = tableLocale.Get(0, locale);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}
