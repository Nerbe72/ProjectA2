using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SideButton : MonoBehaviour
{
    [SerializeField] private KeyCode keyCode;
    [SerializeField] private TMP_Text keyText;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        keyText.text = keyCode.ToString().ToUpper();

        button.onClick.AddListener(() =>
        {
            Player.InvokeKey(keyCode);
        });
    }
}
