using Photon.Pun;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class LoginUI : MonoBehaviour
{
    private Button pressKey;

    private VisualElement loginFrame;
    private TextField idField;
    private TextField passwordField;
    private Button loginButton;
    private Toggle autoLoginToggle;

    private string idKey = "USER-ID";
    private string pwKey = "USER-PW";

    private void Awake()
    {
        InitUI();
    }

    private void Start()
    {
        AutoLogin();
    }

    private void InitUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        pressKey = root.Q<Button>("PressKey");
        pressKey.clicked += OnClickPressKey;
        pressKey.visible = false;

        loginFrame = root.Q<VisualElement>("Login");
        idField = root.Q<TextField>("ID");
        passwordField = root.Q<TextField>("Password");

        loginButton = root.Q<Button>("Login");
        loginButton.clicked += OnClickLogin;

        autoLoginToggle = root.Q<Toggle>("AutoLogin");
    }

    private void AutoLogin()
    {
        if (PlayerPrefs.HasKey(idKey) && PlayerPrefs.HasKey(pwKey))
        {
            autoLoginToggle.value = true;
            idField.value = PlayerPrefs.GetString(idKey);
            passwordField.value = PlayerPrefs.GetString(pwKey);
            OnClickLogin();
            Singleton.Get<Alert>().Show("자동 로그인 되었습니다", Color.white);
        }
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

        // 자동 로그인 설정
        if (autoLoginToggle.value)
        {
            PlayerPrefs.SetString(idKey, idField.value);
            PlayerPrefs.SetString(pwKey, passwordField.value);
        }
        else
        {
            PlayerPrefs.DeleteKey(idKey);
            PlayerPrefs.DeleteKey(pwKey);
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
