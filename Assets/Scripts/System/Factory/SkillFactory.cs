using UnityEngine;

public class SkillFactory : MonoBehaviour
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
