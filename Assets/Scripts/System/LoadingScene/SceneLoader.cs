using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;

    private float multiply = 0.33f;

    private void Awake()
    {
        progressBar.value = 0;
    }

    void Start()
    {
        StartCoroutine(CoLoading());
    }

    private IEnumerator CoLoading()
    {
        float currentProgress = 0f;
        var gameManager = Singleton.Get<GameManager>();
        var photonManager = Singleton.Get<PhotonManager>();

        progressText.text = "룸 대기중";
        yield return new WaitUntil(() => PhotonNetwork.InRoom);

        if (!gameManager.BeforeLoaded)
        {
            yield return new WaitForEndOfFrame();
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;
            while (asyncOperation.progress < 0.9f)
            {
                progressText.text = "초기 오브젝트 생성중";
                progressBar.value = asyncOperation.progress * multiply;
                yield return null;
            }
            asyncOperation.allowSceneActivation = true;

            yield return new WaitUntil(() => asyncOperation.isDone);
            gameManager.BeforeLoaded = true;

            progressText.text = "인벤토리 오브젝트 로드중";
            yield return new WaitUntil(() => Singleton.Inventory != null);
            var inventoryTask = gameManager.LoadInventoryData();
            progressText.text = "인벤토리 데이터 불러오는중";
            yield return new WaitUntil(() => inventoryTask.IsCompleted);
        }

        currentProgress = progressBar.value;

        if (!gameManager.PlayerLoaded)
        {
            var loadedPlayerData = gameManager.LoadPlayerData();
            Player player = photonManager.InstantiatePlayer(loadedPlayerData);

            progressText.text = $"플레이어 생성중";
            Debug.Log("플레이어 데이터 생성중...");

            yield return new WaitUntil(() => Singleton.Player != null);

            Singleton.Player.SetPlayerDataFromLoaded(loadedPlayerData);
            gameManager.PlayerLoaded = true;
        }

        progressText.text = "플레이어 생성 완료";
        Debug.Log($"{Singleton.Player.photonView.ViewID}  플레이어 데이터 로드 완료");

        int sceneIndex = (int)SceneLoadManager.NextScene;
        byte newGroup = (byte)SceneLoadManager.NextScene;
        Singleton.Get<PhotonManager>().ChangeInterestGroup(newGroup); // Subscribe to map interest group for buffered spawn events

        Singleton.Player.IsLoadingScene = true;

        currentProgress = progressBar.value;

        if (!gameManager.AfterLoaded)
        {
            yield return new WaitForEndOfFrame();
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;
            while (asyncOperation.progress < 0.9f)
            {
                progressText.text = "후반 오브젝트 생성중";
                Debug.Log("후반 데이터 로드중...");
                progressBar.value = currentProgress + asyncOperation.progress * multiply;
                yield return null;
            }
            asyncOperation.allowSceneActivation = true;
            yield return new WaitUntil(() => asyncOperation.isDone);
            gameManager.AfterLoaded = true;
        }

        Debug.Log("후반 데이터 로드 완료");

        currentProgress = progressBar.value;

        AsyncOperation asyncMapOperation = SceneManager.LoadSceneAsync(sceneIndex);
        asyncMapOperation.allowSceneActivation = false;
        while (asyncMapOperation.progress < 0.9f)
        {
            progressText.text = "맵 로드중";
            progressBar.value = currentProgress + asyncMapOperation.progress * multiply;
            yield return null;
        }

        Singleton.Player.transform.position = SceneLoadManager.NextPosition;
        Singleton.Player.transform.rotation = SceneLoadManager.NextRotation;
        Singleton.Player.IsLoadingScene = false;

        var mapManager = Singleton.Get<MapManager>();
        if (mapManager != null)
            mapManager.SetCurrentMap(sceneIndex);

        // 씬 로딩 완료 후 메시지 큐 재개
        if (PhotonNetwork.IsConnected)
        {
            yield return null;

            PhotonNetwork.IsMessageQueueRunning = true;
            Debug.Log($"버퍼 처리");
        }

        asyncMapOperation.allowSceneActivation = true;

        yield break;
    }
}
