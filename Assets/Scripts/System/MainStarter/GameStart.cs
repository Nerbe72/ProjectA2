using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene(1);
    }
}
