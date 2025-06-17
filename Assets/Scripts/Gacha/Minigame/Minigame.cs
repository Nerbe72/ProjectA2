using System;
using TMPro;
using UnityEngine;

public abstract class Minigame : MonoBehaviour
{
    public event Action<bool> OnGameFinished;
    public abstract MinigameType Type { get; }

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        SetGame();
    }

    private void Update()
    {
        Control();
    }

    private void OnDestroy()
    {
        OnGameFinished = null;
    }

    /// <summary>
    /// 사용할 변수 초기화
    /// </summary>
    protected abstract void Init();

    /// <summary>
    /// 게임 세팅
    /// </summary>
    public abstract void SetGame();

    /// <summary>
    /// 미니게임 조작 등의 로직
    /// </summary>
    protected abstract void Control();

    protected virtual void GameSuccess()
    {
        OnGameFinished?.Invoke(true);
    }

    protected virtual void GameFail()
    {
        OnGameFinished?.Invoke(false);
    }
}
