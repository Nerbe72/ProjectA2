using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private void Awake()
    {
        if (SingletonManager.spawnManager != null)
        {
            Destroy(gameObject);
            return;
        }

        SingletonManager.spawnManager = this;
        DontDestroyOnLoad(gameObject);


    }


}
