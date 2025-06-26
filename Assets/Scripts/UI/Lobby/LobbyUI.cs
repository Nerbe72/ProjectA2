using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUI : MonoBehaviour
{
    private Button pressKey;

    private VisualElement loginFrame;
    private TextField idField;
    private TextField passwordField;
    private Button loginButton;

    private void Awake()
    {
        InitUI();
    }

    private void InitUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;


        pressKey = root.Q<Button>("PressKey");
        pressKey.clicked += OnClickPressKey;
        pressKey.visible = false;

        loginFrame = root.Q<VisualElement>("LoginFrame");
        idField = root.Q<TextField>("ID");
        passwordField = root.Q<TextField>("Password");

        loginButton = root.Q<Button>("Login");
        loginButton.clicked += OnClickLogin;
    }

    private async void OnClickLogin()
    {
        // test : user1 1234aa!
        var loginTask = LoginManager.LoginAsync(idField.value, passwordField.value);

        loginFrame.visible = false;
        while (!loginTask.IsCompleted)
        {
            if (loginTask.IsFaulted)
            {
                Debug.Log("Login failed");
                loginFrame.visible = true;
                return;
            }
            await Task.Yield();
        }

        if (!loginTask.Result)
        {
            loginFrame.visible = true;
            return;
        }

        PhotonNetwork.ConnectUsingSettings();

        while (!PhotonNetwork.IsConnected)
        {
            Debug.Log("포톤 네트워크 연결중");
            await Task.Yield();
        }

        PhotonNetwork.JoinLobby();

        while (!PhotonNetwork.InLobby)
        {
            await Task.Yield();
        }

        pressKey.visible = true;
    }

    private void OnClickPressKey()
    {
        pressKey.visible = false;
        Singleton.Get<GameManager>().StartGame();
    }
}
