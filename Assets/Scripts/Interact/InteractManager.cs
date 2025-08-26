using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // 씬 전환 시 상호작용 대상 초기화
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Clear();
    }

    public IInteractable GetCurrent()
    {
        if (currentIndex >= CurrentInteract.Count)
        {
            currentIndex = CurrentInteract.Count - 1;
        }

        if (currentIndex < 0 || currentIndex >= CurrentInteract.Count) return null;

        return CurrentInteract[currentIndex];
    }

    public void SwitchInteract()
    {
        if (CurrentInteract == null || CurrentInteract.Count <= 1)
            return;

        currentIndex = (currentIndex + 1) % CurrentInteract.Count;
    }

    public void SetInteract(IInteractable _target)
    {
        if (_target == null) return;
        if (CurrentInteract.Contains(_target)) return;

        CurrentInteract.Add(_target);
        currentIndex = CurrentInteract.Count - 1;
    }

    public void UnSetInteract(IInteractable _target)
    {
        int removedIndex = CurrentInteract.IndexOf(_target);
        if (removedIndex == -1) return;

        CurrentInteract.RemoveAt(removedIndex);

        if (CurrentInteract.Count == 0)
        {
            currentIndex = 0;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, CurrentInteract.Count - 1);
    }

    // 전역 초기화: 씬 로드 또는 강제 초기화 시 호출
    public static void Clear()
    {
        if (CurrentInteract == null) return;
        CurrentInteract.Clear();
        currentIndex = 0;
    }
}
