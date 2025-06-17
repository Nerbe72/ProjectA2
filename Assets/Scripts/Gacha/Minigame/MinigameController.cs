using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.Scripting;

public class MinigameController : MonoBehaviour
{
    public event Action<bool> OnEndMinigame;

    [SerializeField] private MinigameTimer timer;
    [SerializeField] private MinigameSuccessIndicator success;

    private List<Minigame> minigameLoaded = new List<Minigame>();

    private bool isTimeOut = false;
    private bool isGameFinished = true;
    private bool isGameSuccess = false;

    private void Awake()
    {
        timer.OnTimeOut += OnTimerOut;
        success.OnAnimationFinished += CloseMinigame;
    }

    private void OnDisable()
    {
        int count = minigameLoaded.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            Destroy(minigameLoaded[i].gameObject);
        }
        minigameLoaded.Clear();

        GC.Collect();
    }

    private void OnDestroy()
    {
        timer.OnTimeOut -= OnTimerOut;
        success.OnAnimationFinished -= CloseMinigame;
    }

    public async void StartMinigame()
    {
        Debug.Log("미니게임 시작됨");

        Singleton.Get<CameraManager>().SetCamera(CameraType.Minigame);

        //미니게임을 랜덤으로 선택하고 생성함(prefab)

        bool hasLoadedGame = false;
        int count = minigameLoaded.Count;
        for(int i = 0; i < count; i++)
        {
            var notInstantiated = Singleton.Get<MinigameManager>().GetRandomMinigame();
            if (minigameLoaded[i].Type == notInstantiated.GetComponent<Minigame>().Type)
            {
                minigameLoaded[i].SetGame();
                minigameLoaded[i].gameObject.SetActive(true);
                hasLoadedGame = true;
            }
        }

        if (!hasLoadedGame)
        {
            GameObject obj = Instantiate(Singleton.Get<MinigameManager>().GetRandomMinigame());
            obj.transform.parent = this.transform;
            obj.transform.localPosition = Vector3.zero;
            var minigame = obj.GetComponent<Minigame>();
            minigame.OnGameFinished += (result) => { isGameSuccess = result; isGameFinished = false; };
            minigameLoaded.Add(minigame);
        }

        isGameSuccess = false;
        timer.StartTimer(5f);

        while (!isTimeOut)
        {
            if (!isGameFinished)
            {
                EndMinigame();
                return;
            }

            await Task.Yield();
        }

        EndMinigame();
    }

    public void EndMinigame()
    {
        timer.StopTimer();
        success.ShowSuccess(isGameSuccess);
    }

    //결과 애니메이션 표시 이후 호출
    public void CloseMinigame()
    {
        Singleton.Get<CameraManager>().SetCamera(CameraType.Main);
        OnEndMinigame?.Invoke(isGameSuccess);
    }

    private void OnTimerOut()
    {
        isTimeOut = true;
    }
}
