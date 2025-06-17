using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AuthManager : MonoBehaviour
{
    public int InitializationPriority => 0;
    private string token;
    public static string URL;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        token = "";
        URL = "http://localhost:3000/";
    }

    public void SetToken(string _token)
    {
        token = _token;
        Debug.Log("<color=orange>유저 토큰이 변경되었습니다</color>");
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
        while (string.IsNullOrEmpty(token))
        {
            await Task.Yield();
        }

        UnityWebRequest request = await SendAuthorizedRequest(URL + _type.ToString(), "POST");

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

        UnityWebRequest request = new UnityWebRequest(URL + _type.ToString(), "POST");
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
            Debug.LogError("서버 요청 오류: " + request.error);
        } else
        {
            Debug.Log($"<color=green>{_type.ToString()} 데이터 저장 성공</color>");
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
        UnityWebRequest request = await SendAuthorizedRequest(URL + _fileName, "POST");

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
        UnityWebRequest request = await SendAuthorizedRequest(URL + _fileName, "POST");

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
