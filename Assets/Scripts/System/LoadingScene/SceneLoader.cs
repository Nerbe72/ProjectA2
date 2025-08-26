using Photon.Pun;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using GameStuff;

[System.Serializable]
public class LoadingTooltip
{
    public Sprite Sprite;
    public int LocaleID;
}

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private float tooltipSkipTime;
    [SerializeField] private float loadingDelay = 6f;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float textSpeedMultiply = 1.6f;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Transform backgroundParent;
    [SerializeField] private Transform tooltipTextParent;
    [SerializeField] private List<LoadingTooltip> loadingTooltips;

    private float multiply = 0.33f;
    private Image[] backgroundImages;
    private TMP_Text[] tooltipTexts;
    private int currentTooltipIndex = -1;
    private int imageIndex = -1;

    private void Awake()
    {
        progressBar.value = 0;

        backgroundImages = backgroundParent.GetComponentsInChildren<Image>();
        tooltipTexts = tooltipTextParent.GetComponentsInChildren<TMP_Text>();

        imageIndex = 1;
        backgroundImages[imageIndex].transform.SetAsLastSibling();

        int randomIndex = Random.Range(0, loadingTooltips.Count);
        currentTooltipIndex = randomIndex;

        var selectedTooltip = loadingTooltips[currentTooltipIndex];

        backgroundImages[imageIndex].sprite = selectedTooltip.Sprite;
        backgroundImages[imageIndex].color = Color.white;
        
        SetTooltipText(imageIndex, selectedTooltip.LocaleID);
        tooltipTexts[imageIndex].color = Color.white;
    }

    void Start()
    {
        StartCoroutine(CoTooltip());
        StartCoroutine(CoLoading());
    }

    private IEnumerator CoTooltip()
    {
        while (true)
        {
            yield return new WaitForSeconds(tooltipSkipTime);
            
            bool IsValidRange1 = currentTooltipIndex > 0;
            bool IsValidRange2 = (currentTooltipIndex + 1) < loadingTooltips.Count;

            if (IsValidRange1 && IsValidRange2)
            {
                if (Random.Range(0, 2) == 0)
                    currentTooltipIndex = Random.Range(0, currentTooltipIndex);
                else
                    currentTooltipIndex = Random.Range((currentTooltipIndex + 1), loadingTooltips.Count);
            } else
            {
                if (IsValidRange1)
                    currentTooltipIndex = Random.Range(0, currentTooltipIndex);
                else if (IsValidRange2)
                    currentTooltipIndex = Random.Range((currentTooltipIndex + 1), loadingTooltips.Count);
                else
                    currentTooltipIndex = Random.Range(0, loadingTooltips.Count);
            }
            
            StartCoroutine(TextFade());
            yield return StartCoroutine(BackgroundFade());
        }
    }

    private IEnumerator BackgroundFade()
    {
        var selectedTooltip = loadingTooltips[currentTooltipIndex];
        
        int backIndex = 1 - imageIndex;
        int frontIndex = imageIndex;
        
        backgroundImages[backIndex].sprite = selectedTooltip.Sprite;
        backgroundImages[backIndex].color = Color.white;
        
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime * fadeSpeed;
            
            backgroundImages[frontIndex].color = Color.Lerp(Color.white, Color.clear, time);

            if (time >= 1f) break;

            yield return null;
        }
        
        backgroundImages[frontIndex].transform.SetAsFirstSibling();
        
        imageIndex = 1 - imageIndex;

        yield break;
    }

    private void SetTooltipText(int _imageIndex, int _localeID)
    {
        var localeTable = Singleton.Get<TableDataManager>().Table.Locale;
        string tooltipText = localeTable.Get(_localeID, GameManager.CurrentLocale);
        tooltipTexts[_imageIndex].text = tooltipText;
    }

    private IEnumerator TextFade()
    {
        var selectedTooltip = loadingTooltips[currentTooltipIndex];
        
        int backIndex = 1 - imageIndex;
        int frontIndex = imageIndex;
        
        SetTooltipText(backIndex, selectedTooltip.LocaleID);
        tooltipTexts[backIndex].color = Color.clear;
        
        Vector2 backStartPos = tooltipTexts[backIndex].rectTransform.anchoredPosition;
        backStartPos.x = 80f;
        tooltipTexts[backIndex].rectTransform.anchoredPosition = backStartPos;
        
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime * (fadeSpeed * textSpeedMultiply);
            
            tooltipTexts[frontIndex].color = Color.Lerp(Color.white, Color.clear, time);
            tooltipTexts[backIndex].color = Color.Lerp(Color.clear, Color.white, time);
            
            Vector2 backCurrentPos = tooltipTexts[backIndex].rectTransform.anchoredPosition;
            backCurrentPos.x = Mathf.Lerp(80f, 0f, time);
            tooltipTexts[backIndex].rectTransform.anchoredPosition = backCurrentPos;

            if (time >= 1f) break;

            yield return null;
        }

        yield break;
    }

    private IEnumerator CoLoading()
    {
        // 로딩 시작 시 모든 사운드 멈춤
        var soundManager = Singleton.Get<SoundManager>();
        if (soundManager != null)
        {
            soundManager.StopAllSounds();
        }

        var gameManager = Singleton.Get<GameManager>();
        var photonManager = Singleton.Get<PhotonManager>();

        yield return new WaitUntil(() => PhotonNetwork.InRoom);

        yield return StartCoroutine(LerpProgress(0f, 0.125f, loadingDelay / 3f));
        yield return StartCoroutine(LerpProgress(0.125f, 0.25f, loadingDelay / 3f));
        yield return StartCoroutine(LerpProgress(0.25f, 0.375f, loadingDelay / 3f));

        if (!gameManager.BeforeLoaded)
        {
            yield return new WaitForEndOfFrame();
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;
            
            yield return StartCoroutine(LerpProgressWithAsync(0.375f, 0.5f, asyncOperation, 1f));
            
            asyncOperation.allowSceneActivation = true;
            yield return new WaitUntil(() => asyncOperation.isDone);
            gameManager.BeforeLoaded = true;

            yield return new WaitUntil(() => Singleton.Inventory != null);
            var inventoryTask = gameManager.LoadInventoryData();
            yield return new WaitUntil(() => inventoryTask.IsCompleted);
        }
        else
        {
            progressBar.value = 0.5f;
        }

        if (!gameManager.PlayerLoaded)
        {
            var loadedPlayerData = gameManager.LoadPlayerData();
            Player player = photonManager.InstantiatePlayer(loadedPlayerData);

            Debug.Log("플레이어 생성중...");

            yield return StartCoroutine(LerpProgressWithWait(0.5f, 0.625f, () => Singleton.Player != null, 1f));

            yield return new WaitUntil(() => Singleton.Player != null);

            Singleton.Player.SetPlayerDataFromLoaded(loadedPlayerData);

            SceneLoadManager.NextScene = (Map)loadedPlayerData.Scene;
            Singleton.Player.transform.position = loadedPlayerData.Position;
            Singleton.Player.transform.rotation = loadedPlayerData.Rotation;
            gameManager.PlayerLoaded = true;
        }
        else
        {
            progressBar.value = 0.625f;
        }

        Debug.Log($"{Singleton.Player.photonView.ViewID}  플레이어 데이터 로드 완료");

        int sceneIndex = (int)SceneLoadManager.NextScene;
        byte newGroup = (byte)SceneLoadManager.NextScene;
        Singleton.Get<PhotonManager>().ChangeInterestGroup(newGroup);

        Singleton.Player.IsLoadingScene = true;

        if (!gameManager.AfterLoaded)
        {
            yield return new WaitForEndOfFrame();
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;
            
            yield return StartCoroutine(LerpProgressWithAsync(0.625f, 0.75f, asyncOperation, 1f));
            
            asyncOperation.allowSceneActivation = true;
            yield return new WaitUntil(() => asyncOperation.isDone);
            gameManager.AfterLoaded = true;
        }
        else
        {
            progressBar.value = 0.75f;
        }

        Debug.Log("후반 데이터 로드 완료");

        AsyncOperation asyncMapOperation = SceneManager.LoadSceneAsync(sceneIndex);
        asyncMapOperation.allowSceneActivation = false;
        
        yield return StartCoroutine(LerpProgressWithAsync(0.75f, 0.875f, asyncMapOperation, 1f));
        
        var mapManager = Singleton.Get<MapManager>();
        if (mapManager != null)
            mapManager.SetCurrentMap(sceneIndex);
        
        // 맵 로드 완료 후 초기 BGM 설정
        var mapSoundManager = Singleton.Get<SoundManager>();
        if (mapSoundManager != null)
        {
            GameStuff.Map mapType = (GameStuff.Map)sceneIndex;
            mapSoundManager.PlayMapBGM(mapType);
        }
        
        if (PhotonNetwork.InRoom)
        {
            yield return null;
            PhotonNetwork.IsMessageQueueRunning = true;
            Debug.Log($"버퍼 처리");
        }

        Singleton.Player.IsLoadingScene = false;
        asyncMapOperation.allowSceneActivation = true;

        yield return StartCoroutine(LerpProgress(0.875f, 1f, 0.5f));

        yield return new WaitUntil(() => asyncMapOperation.isDone);
    }

    private IEnumerator LerpProgress(float _startProgress, float _targetProgress, float _duration)
    {
        float time = 0f;
        float startProgress = _startProgress;
        float targetProgress = _targetProgress;
        float duration = _duration;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            progressBar.value = Mathf.Lerp(startProgress, targetProgress, progress);
            yield return null;
        }

        progressBar.value = targetProgress;
    }

    private IEnumerator LerpProgressWithAsync(float _startProgress, float _targetProgress, AsyncOperation _asyncOperation, float _maxDuration)
    {
        float time = 0f;
        float startProgress = _startProgress;
        float targetProgress = _targetProgress;
        float maxDuration = _maxDuration;

        while (time < maxDuration && _asyncOperation.progress < 0.9f)
        {
            time += Time.deltaTime;
            float progress = time / maxDuration;
            progressBar.value = Mathf.Lerp(startProgress, targetProgress, progress);
            yield return null;
        }

        // 실제 로딩이 완료되면 즉시 목표 진행률로
        progressBar.value = targetProgress;
    }

    private IEnumerator LerpProgressWithWait(float _startProgress, float _targetProgress, System.Func<bool> _waitCondition, float _maxDuration)
    {
        float time = 0f;
        float startProgress = _startProgress;
        float targetProgress = _targetProgress;
        float maxDuration = _maxDuration;

        while (time < maxDuration && !_waitCondition())
        {
            time += Time.deltaTime;
            float progress = time / maxDuration;
            progressBar.value = Mathf.Lerp(startProgress, targetProgress, progress);
            yield return null;
        }

        // 조건이 만족되면 즉시 목표 진행률로
        progressBar.value = targetProgress;
    }
}
