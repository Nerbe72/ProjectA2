using System;

using GameStuff;

/// <summary>
/// 플레이어 입력에 의해 상호작용 가능
/// </summary>
public interface IInteractable
{
    public InteractType InteractType { get; }
    public string ShownString { get; }

    public bool IsNowInteractable { get; }

    public event Action OnInteractStart;
    public event Action OnInteractEnd;

    /// <summary>
    /// 상호작용 행동
    /// </summary>
    public void DoAction();

    /// <summary>
    /// 상호작용 종료시 수동 호출
    /// </summary>
    public void EndAction();
}
