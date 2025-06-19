using System.Collections;
using System.ComponentModel;
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

        if (!GameManager.BeforeLoaded)
        {
            yield return new WaitForEndOfFrame();
            UnityEngine.AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;
            float time = 0;
            while (!asyncOperation.isDone)
            {
                time += Time.deltaTime;
                ProgressBar.value = asyncOperation.progress * 0.3f;
                Debug.Log("Loading Before");

                if (asyncOperation.progress >= 0.9f)
                {
                    break;
                }
                yield return new WaitForEndOfFrame();
            }

            asyncOperation.allowSceneActivation = true;
        }

        if (!GameManager.PlayerLoaded)
        {

        }

        if (!GameManager.AfterLoaded)
        {
            yield return new WaitForEndOfFrame();
            UnityEngine.AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;
            float time = 0;
            while (!asyncOperation.isDone)
            {
                time += Time.deltaTime;
                ProgressBar.value = 0.3f + asyncOperation.progress * 0.3f;
                Debug.Log("Loading After");

                if (asyncOperation.progress >= 0.9f)
                {
                    break;
                }
                yield return new WaitForEndOfFrame();
            }

            asyncOperation.allowSceneActivation = true;
        }

        yield break;
    }
}
