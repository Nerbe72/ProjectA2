using System;

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
    public void EndAction();
}
