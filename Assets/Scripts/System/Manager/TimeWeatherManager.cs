using UnityEngine;

public class TimeWeatherManager : MonoBehaviour
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
