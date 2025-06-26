using System.Collections.Generic;
using UnityEngine;

public class InteractManager : MonoBehaviour
{
    public int InitializationPriority => 6;

    public static List<IInteractable> CurrentInteract { get; private set; }
    public static int currentIndex = 0;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        CurrentInteract = new List<IInteractable>();
    }

    public IInteractable GetCurrent()
    {
        if (currentIndex >= CurrentInteract.Count)
        {
            currentIndex = CurrentInteract.Count - 1;
        }

        return CurrentInteract[currentIndex];
    }

    public void SwitchInteract()
    {
        currentIndex = (currentIndex + 1) % CurrentInteract.Count;
    }

    public void SetInteract(IInteractable _target)
    {
        CurrentInteract.Add(_target);
        currentIndex = CurrentInteract.Count;
    }

    public void UnSetInteract(IInteractable _target)
    {
        if (!CurrentInteract.Contains(_target)) return;

        if (GetCurrent() == _target)
            currentIndex = 0;

        CurrentInteract.Remove(_target);
    }
}
