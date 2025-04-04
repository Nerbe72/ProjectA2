using System;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;
    private static string token;

    public static string url = "http://localhost:3000/";
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
            return;
        }

        token = "";
    }

    public void SetToken(string _token)
    {
        token = _token;
        Debug.LogWarning("<color=orange>��ū�� ����Ǿ����ϴ�</color>");
    }

    public async Task<UnityWebRequest> SendAuthorizedRequest(string _url, string _getpost)
    {
        UnityWebRequest request = new UnityWebRequest(_url, _getpost);

        // ��ū�� ���� ��� Authorization ����� �߰�
        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        request.downloadHandler = new DownloadHandlerBuffer();

        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        return request;
    }

    /// <summary>
    /// ���� ������ �ε�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_type"></param>
    /// <returns></returns>
    public async Task<T> GetDataAsync<T>(Request _type)
    {
        UnityWebRequest request = await SendAuthorizedRequest(url + _type.ToString(), "GET");

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("���� ���� ����: " + request.error);
            return default;
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("���� ������ ��û ����: " + request.error);
            return default;
        }

        string responseText = request.downloadHandler.text;
        T data = JsonUtility.FromJson<T>(responseText);
        return data;
    }

    public async Task SetDataAsync<T>(Request _type, T _data)
    {
        string jsonData = JsonUtility.ToJson(_data);

        UnityWebRequest request = new UnityWebRequest(url + _type.ToString(), "POST");
        byte[] raw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(raw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("������ ���� ����: " + request.error);
        } else
        {
            Debug.Log($"<color=green>{_type.ToString()} �Ϸ�</color>");
        }
    }

    /// <summary>
    /// ���� ������ ���� �ε�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_fileName"></param>
    /// <returns></returns>
    public async Task<T> GetUserDataAsync<T>(string _fileName) where T : new ()
    {
        UnityWebRequest request = await SendAuthorizedRequest(url + _fileName, "POST");

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("���� ���� ����: " + request.error);
            return default;
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("���� ������ ��û ����: " + request.error);

            if (request.responseCode == 404)
            {
                Debug.LogWarning("���� �����͸� ã�� �� ���� ���� �ۼ��մϴ�.");
                T newData = new T();

                await SetUserDataAsync<T>(_fileName, newData);

                return newData;
            }

            return default;
        }

        string responseText = request.downloadHandler.text;
        T data = JsonUtility.FromJson<T>(responseText);
        return data;
    }

    public async Task<T> SetUserDataAsync<T>(string _fileName, T _data)
    {
        UnityWebRequest request = await SendAuthorizedRequest(url + _fileName, "POST");

        string jsondata = JsonUtility.ToJson(_data);
        byte[] raw = System.Text.Encoding.UTF8.GetBytes(jsondata);
        request.uploadHandler = new UploadHandlerRaw(raw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        return default;
    }

    //public async Task<UnityWebRequest> SendAuthedWriteRequest(string _url, string jsonData = null)
    //{
    //    UnityWebRequest request = new UnityWebRequest(_url, "POST");

    //    if (!string.IsNullOrEmpty(jsonData))
    //    {
    //        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
    //        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //        request.SetRequestHeader("Content-Type", "application/json");
    //    }

    //    return request;
    //}

    //public async Task<T> SetDataAsync<T>(Request _type, string _jsonData = null)
    //{
    //    UnityWebRequest request = await SendAuthedWriteRequest("http://localhost:3000/" + _type.ToString());

    //    if (request.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.LogError("API ��û ����: " + request.error);
    //        return default;
    //    }

    //    string responseText = request.downloadHandler.text;
    //    T data = JsonUtility.FromJson<T>(responseText);
    //    return data;
    //}
}
