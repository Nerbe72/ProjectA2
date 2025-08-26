using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

using GameStuff;
using CameraType = GameStuff.CameraType;

public class CameraManager : MonoBehaviour
{
    public int InitializationPriority => 1;
    public Camera main { get; private set; }
    public CameraType CurrentCamType { get; private set; }
    [SerializeField] private List<CinemachineCamera> vCams;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            if (Singleton.Get<CameraManager>() == this) return;
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += ChangeCameraToMain;
        SceneManager.sceneLoaded += ResetMousePosition;
        //SceneManager.sceneLoaded += ResetCameraPosition;

        main = Camera.main;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= ChangeCameraToMain;
        SceneManager.sceneLoaded -= ResetMousePosition;
        //SceneManager.sceneLoaded -= ResetCameraPosition;
    }

    private void ChangeCameraToMain(Scene _scene, LoadSceneMode _mode)
    {
        main = Camera.main;
    }

    public void IgnoreCameraController(bool _ignore)
    {
        vCams[(int)CameraType.Main].GetComponent<CinemachineInputAxisController>().enabled = !_ignore;
    }

    public void SetCameraTo(CameraType _type, Transform _transform)
    {
        if (vCams[(int)_type] == null) return;

        vCams[(int)_type].Follow = _transform;
    }

    public void ResetCameraPosition(Scene _scene, LoadSceneMode _mode)
    {
        try
        {
            for (int i = 0; i < vCams.Count; i++)
            {
                vCams[i].GetComponent<CinemachineCamera>().ForceCameraPosition(Singleton.Player.transform.position, Singleton.Player.transform.rotation);
            }
        }
        catch { }
    }

    public void ResetMousePosition(Scene _scene, LoadSceneMode _mode)
    {
        Input.mousePosition.Set(0, 0, 0);
    }

    public Vector3 GetEulerY()
    {
        return new Vector3(0, main.transform.eulerAngles.y, 0);
    }

    public Vector3 GetCameraForward()
    {
        return main.transform.forward;
    }

    public Vector3 GetCameraRight()
    {
        return main.transform.right;
    }

    public void SetCamera(CameraType _type, Target _target = null)
    {
        if (_type == CameraType.Target && _target == null) return;

        SwitchCamera(_type);

        if (_type != CameraType.Target) return;
        vCams[(int)_type].GetComponent<CinemachineCamera>().LookAt = _target.transform;
    }

    public void SetCamera(string _type, Transform _follow = null, Transform _lookAt = null)
    {
        CameraType selected = CameraType.Main;
        switch (_type)
        {
            case "WideLook":
                selected = CameraType.Talk;
                break;
            case "ZoomEnemy":
                selected = CameraType.Talk;
                break;
        }

        SwitchCamera(selected, _follow);
    }

    private void SwitchCamera(CameraType _followType, Transform _follow = null)
    {
        if (vCams[(int)_followType].gameObject.activeSelf)
            return;

        switch (_followType)
        {
            case CameraType.Talk:
                if (_follow != null)
                {
                    var talkCamera = vCams[(int)_followType];
                    talkCamera.transform.position = _follow.position;
                    talkCamera.transform.rotation = _follow.rotation;
                    talkCamera.GetComponent<CinemachineCamera>().Follow = null;
                }
                break;
            case CameraType.Minigame:
                break;
            default:
                // Main, Target, Sit, Dead 등 일반 카메라들
                if (_follow == null)
                    vCams[(int)_followType].GetComponent<CinemachineCamera>().Follow = Singleton.Player.transform;
                else
                    vCams[(int)_followType].GetComponent<CinemachineCamera>().Follow = _follow;

                if (vCams[(int)_followType].GetComponent<CinemachineOrbitalFollow>() != null)
                {
                    StopAllCoroutines();
                    StartCoroutine(AutoCentering(vCams[(int)_followType].GetComponent<CinemachineOrbitalFollow>()));
                }
                break;
        }

        // 모든 카메라 비활성화 후 해당 카메라만 활성화
        for (int i = 0; i < vCams.Count; i++)
        {
            if (vCams[i] == null) continue;
            vCams[i].gameObject.SetActive(i == (int)_followType);
        }
    }

    public void SetCameraCenter(CameraType _type, Transform _player, Target _target = null)
    {
        var orbitalFollow = vCams[(int)_type].GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow == null) return;

        if (_target != null)
        {
            // 타겟이 있을 경우: 플레이어-타겟 라인으로 카메라 중심 설정
            Vector3 directionToTarget = (_target.transform.position - _player.position);
            directionToTarget.y = 0; // 수평 방향만 고려
            float desiredAngle = Quaternion.LookRotation(directionToTarget).eulerAngles.y;
            orbitalFollow.HorizontalAxis.Center = desiredAngle;
            orbitalFollow.HorizontalAxis.Value = desiredAngle;
        }
        else
        {
            orbitalFollow.HorizontalAxis.Center = _player.rotation.eulerAngles.y;
        }
    }

    public void UpdateTargeting(CameraType _type, Transform _player, Target _target, bool _isSprinting)
    {
        var orbitalFollow = vCams[(int)_type].GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow == null || _target == null) return;

        if (_isSprinting)
        {
            Vector3 directionToTarget = (_target.transform.position - _player.position);
            directionToTarget.y = 0;
            float desiredAngle = Quaternion.LookRotation(directionToTarget).eulerAngles.y;

            orbitalFollow.HorizontalAxis.Value = Mathf.LerpAngle(orbitalFollow.HorizontalAxis.Value, desiredAngle, Time.deltaTime * 5f);
        }
    }

    public void OffAllCam()
    {
        for (int i = 0; i < vCams.Count; i++)
        {
            vCams[i].gameObject.SetActive(false);
        }
    }

    public void ResetCam()
    {
        for (int i = 0; i < vCams.Count; i++)
        {
            vCams[i].gameObject.SetActive(i == 0);
        }
    }

    public void ShakeCamera(float _shakeTime = 0.2f)
    {
        for (int i = 0; i < vCams.Count; i++)
        {
            var shakable = vCams[i].GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (shakable != null)
            {
                StartCoroutine(ShakeLerp(shakable, _shakeTime));
            }
        }
    }

    private IEnumerator AutoCentering(CinemachineOrbitalFollow _orbitalCamera)
    {
        _orbitalCamera.HorizontalAxis.Recentering.Enabled = true;
        _orbitalCamera.VerticalAxis.Recentering.Enabled = true;

        yield return new WaitForSeconds(1f);

        _orbitalCamera.HorizontalAxis.Recentering.Enabled = false;
        _orbitalCamera.VerticalAxis.Recentering.Enabled = false;

        yield break;
    }

    private IEnumerator ShakeLerp(CinemachineBasicMultiChannelPerlin _shakable, float _shakeTime)
    {
        _shakable.enabled = true;
        yield return new WaitForSeconds(_shakeTime);
        _shakable.enabled = false;
        yield break;
    }
}
