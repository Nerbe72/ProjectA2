using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private Button gachaButton;
    [SerializeField] private Button inventoryButton;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        //inventoryButton.onClick.AddListener(ClickInventory);
        //gachaButton.onClick.AddListener(ClickGacha);
    }

    private void ClickGacha()
    {
        Singleton.Get<GachaManager>().Self.SetActive(true);
    }

    private void ClickInventory()
    {
        Singleton.Inventory.Self.SetActive(true);
    }
}
