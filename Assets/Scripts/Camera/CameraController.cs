using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static Player player;
    private static CameraManager cameraManager;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (player == null) return;
        cameraManager.SetCameraCenter(CameraType.Target, player.transform);
    }

    private static void Set()
    {
        if (player == null)
            player = Singleton.Player;

        if (cameraManager == null)
            cameraManager = Singleton.Get<CameraManager>();
    }
}
