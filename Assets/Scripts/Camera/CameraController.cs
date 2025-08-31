using System.Threading.Tasks;
using UnityEngine;

using GameStuff;
using CameraType = GameStuff.CameraType;

public class CameraController : MonoBehaviour
{
    private static Player player;
    private static CameraManager cameraManager;

    private Camera mainCamera;
    [SerializeField] private float targetAspect = 16f / 9f;
    private int prevScreenWidth;
    private int prevScreenHeight;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        mainCamera = GetComponent<Camera>();
        prevScreenWidth = Screen.width;
        prevScreenHeight = Screen.height;
        SetAspect();
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
        if (Screen.width != prevScreenWidth || Screen.height != prevScreenHeight)
        {
            prevScreenWidth = Screen.width;
            prevScreenHeight = Screen.height;
            SetAspect();
        }

        if (player == null || cameraManager == null) return;

        Target currentTarget = Singleton.Get<TargetManager>().CurrentTarget;
        bool isSprinting = player.IsFlagged(StateFlags.Run);

        cameraManager.SetCameraCenter(CameraType.Target, player.transform, currentTarget);

        if (currentTarget != null)
        {
            cameraManager.UpdateTargeting(CameraType.Target, player.transform, currentTarget, isSprinting);
        }
    }

    private void SetAspect()
    {
        if (mainCamera == null) return;

        float windowAspect = (float)Screen.width / Screen.height;
        float scale = windowAspect / targetAspect;

        if (scale < 1f)
        {
            mainCamera.rect = new Rect((1f - scale) / 2f, 0f, scale, 1f);
        }
        else
        {
            mainCamera.rect = new Rect(0f, (1f - 1f / scale) / 2f, 1f, 1f / scale);
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
