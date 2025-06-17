using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class MinigameManager : MonoBehaviour
{
    //경로로부터 가져옴
    private List<GameObject> gameList = new List<GameObject>();

    public int InitializationPriority => 3;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        LoadMinigames();
    }

    private async void LoadMinigames()
    {
        string name = "minigame.speed_texting";

        //UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(path);
        //await webRequest.SendWebRequest();
        //AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(webRequest);
        //var loaded = bundle.LoadAllAssets<GameObject>();

        var async = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, name));

        while (!async.isDone) await Task.Yield();

        var bundle = async.assetBundle;
        var loaded = bundle.LoadAllAssets<GameObject>();
        gameList = new List<GameObject>(loaded);
        Debug.Log($"미니게임 {gameList.Count}개 로드됨");
    }

    public GameObject GetRandomMinigame()
    {
        return gameList[Random.Range(0, gameList.Count)];
    }
}
