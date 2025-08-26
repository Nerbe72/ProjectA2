using GameStuff;
using System.Collections.Generic;
using UnityEngine;

public class QuestPathManager : MonoBehaviour
{
    private GameObject currentPath;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void DrawPath(int _questID)
    {
        ClearPath();

        currentPath = Instantiate(ResourceLoader.Load<GameObject>(_questID + "_QuestPath", LoadType.Path));
        currentPath.gameObject.SetActive(true);
    }

    public void ClearPath()
    {
        if (currentPath != null)
        {
            Destroy(currentPath);
            currentPath = null;
        }
    }
}
