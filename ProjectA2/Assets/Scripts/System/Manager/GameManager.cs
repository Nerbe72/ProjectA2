using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        if (SingletonManager.gameManager != null)
        {
            Destroy(gameObject);
            return;
        }

        SingletonManager.gameManager = this;
        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 60;
    }
}
