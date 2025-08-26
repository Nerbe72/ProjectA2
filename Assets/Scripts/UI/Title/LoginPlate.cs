using Photon.Pun;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LoginPlate : PlateBase
{
    private enum Progress
    {
        LoginContinuing,
        Connecting,
        Failed,
        Succeeded
    }

    [SerializeField] private TMP_InputField idField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_InputField serverUrlField;
    [SerializeField] private Toggle autoLoginToggle;
    [SerializeField] private Button loginButton;

    [SerializeField] private TMP_Text progressingText;
    [SerializeField] private Button logoutButton;

    private string idKey = "USER-ID";
    private string pwKey = "USER-PW";
    private string serverUrlKey = "SERVER-URL";

    public event Action<bool> OnLogined;

    protected override void Awake()
    {
        base.Awake();

        loginButton.onClick.AddListener(StartLogin);
    }

    private string GetIPFromURL(string url)
    {
        if (string.IsNullOrEmpty(url))
            return "localhost";

        try
        {
            Uri uri = new Uri(url);
            return uri.Host;
        }
        catch
        {
            return "localhost";
        }
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey(serverUrlKey))
        {
            string savedIP = PlayerPrefs.GetString(serverUrlKey);
            serverUrlField.text = savedIP;
            AuthManager.URL = $"http://{savedIP}:3000/";
        }
        else
        {
            serverUrlField.text = GetIPFromURL(AuthManager.URL);
        }
        
        AutoLogin();
    }

    private void AutoLogin()
    {
        if (PlayerPrefs.HasKey(idKey) && PlayerPrefs.HasKey(pwKey))
        {
            autoLoginToggle.isOn = true;
            idField.text = PlayerPrefs.GetString(idKey);
            passwordField.text = PlayerPrefs.GetString(pwKey);
            StartLogin();
            Singleton.Get<Alert>().Show("자동 로그인 되었습니다", Color.white);
        }
    }

    private async void StartLogin()
    {
        PlayerPrefs.SetString(serverUrlKey, serverUrlField.text);
        AuthManager.URL = $"http://{serverUrlField.text}:3000/";
        
        var loginTask = LoginManager.LoginAsync(idField.text, passwordField.text);

        SetProgress(Progress.LoginContinuing);

        while (!loginTask.IsCompleted)
        {
            if (loginTask.IsFaulted)
            {
                SetProgress(Progress.Failed);
                return;
            }
            await Task.Yield();
        }

        if (!loginTask.Result)
        {
            SetProgress(Progress.Failed);
            return;
        }

        // 자동 로그인 설정
        if (autoLoginToggle.isOn)
        {
            PlayerPrefs.SetString(idKey, idField.text);
            PlayerPrefs.SetString(pwKey, passwordField.text);
        }
        else
        {
            PlayerPrefs.DeleteKey(idKey);
            PlayerPrefs.DeleteKey(pwKey);
        }

        PhotonNetwork.ConnectUsingSettings();

        while (!PhotonNetwork.IsConnected)
        {
            SetProgress(Progress.Connecting);
            await Task.Yield();
        }

        PhotonNetwork.JoinLobby();

        while (!PhotonNetwork.InLobby)
        {
            await Task.Yield();
        }

        SetProgress(Progress.Succeeded);

        OnLogined?.Invoke(true);
    }

    private void StartLogout()
    {
        OnLogined?.Invoke(false);
    }

    private void SetProgress(Progress _progress)
    {
        switch (_progress)
        {
            case Progress.LoginContinuing:
                Singleton.Get<Alert>().Show("로그인 시도중..", Color.white);
                break;
            case Progress.Connecting:
                Singleton.Get<Alert>().Show("서버 연결중..", Color.white);
                break;
            case Progress.Failed:
            default:
                Singleton.Get<Alert>().Show("로그인 실패", Color.red);
                break;
            case Progress.Succeeded:
                Singleton.Get<Alert>().Show("접속 성공", Color.green);
                buttons[0].Click();
                break;
        }
    }
}
