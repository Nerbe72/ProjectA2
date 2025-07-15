using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

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
        var names = AssetDatabase.GetAllAssetBundleNames();

        foreach(var name in names)
        {
            var filter = name.Split('.')[0];

            if (!filter.Equals("minigame")) continue;

            var async = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, name));
            while (!async.isDone) await Task.Yield();

            var bundle = async.assetBundle;
            var loaded = bundle.LoadAllAssets<GameObject>();
            gameList.AddRange(loaded);
        }

        Debug.Log($"<color=green>미니게임 {gameList.Count}개 로드됨</color>");
    }

    public GameObject GetRandomMinigame()
    {
        return gameList[Random.Range(0, gameList.Count)];
    }
}
