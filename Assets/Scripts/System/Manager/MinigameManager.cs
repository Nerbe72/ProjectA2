using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
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
        var async = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "minigame"));
        while (!async.isDone) await Task.Yield();

        var bundle = async.assetBundle;
        var loaded = bundle.LoadAllAssets<GameObject>();
        gameList.AddRange(loaded);

        Debug.Log($"<color=green>미니게임 {gameList.Count}개 로드됨</color>");
    }

    public GameObject GetRandomMinigame()
    {
        return gameList[Random.Range(0, gameList.Count)];
    }
}
