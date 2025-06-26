using UnityEngine;

public class QuestIndicatorUI : MonoBehaviour
{
    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
}
