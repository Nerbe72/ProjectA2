using System.Threading.Tasks;
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

    private async void Start()
    {
        while(cameraManager == null)
        {
            cameraManager = Singleton.Get<CameraManager>();
            await Task.Delay(100);
        }

        while (player == null)
        {
            player = Singleton.Player;
            await Task.Delay(100);
        }
    }

    private void Update()
    {
        if (player == null || cameraManager == null) return;

        Target currentTarget = Singleton.Get<TargetManager>().CurrentTarget;
        bool isSprinting = player.IsFlagged(StateFlags.Run);

        cameraManager.SetCameraCenter(CameraType.Target, player.transform, currentTarget);

        if (currentTarget != null)
        {
            cameraManager.UpdateTargeting(CameraType.Target, player.transform, currentTarget, isSprinting);
        }
    }

    private static void Set()
    {
        if (player == null)
            player = Singleton.Player;

        if (cameraManager == null)
            cameraManager = Singleton.Get<CameraManager>();
    }
}
