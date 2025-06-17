using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Slider ProgressBar;

    private void Awake()
    {
        ProgressBar.value = 0;
    }

    void Start()
    {
        StartCoroutine(CoLoading());
    }

    private IEnumerator CoLoading()
    {
        var nextScene = SceneLoadManager.NextScene == Map.None ? Map.World_FrontVillage : SceneLoadManager.NextScene;
        while (true)
        {
            yield return new WaitForEndOfFrame();
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync((int)nextScene);
            asyncOperation.allowSceneActivation = false;
            float time = 0f;

            while (true)
            {
                time += Time.deltaTime;
                ProgressBar.value = time;

                if (time >= 0.1f)
                {
                    break;
                }

                yield return new WaitForEndOfFrame();
            }

            while (!asyncOperation.isDone)
            {
                time += Time.deltaTime;
                ProgressBar.value = time + asyncOperation.progress;
                Debug.Log("Loading");

                if (asyncOperation.progress >= 0.9f)
                {
                    break;
                }
                yield return new WaitForEndOfFrame();
            }

            ProgressBar.value = 1f;

            time = 0f;
            while (true)
            {
                time += Time.deltaTime;

                if (time >= 0.1f)
                {
                    break;
                }

                yield return new WaitForEndOfFrame();
            }

            Debug.Log("Loading End");

            asyncOperation.allowSceneActivation = true;
            yield break;
        }
    }
}
