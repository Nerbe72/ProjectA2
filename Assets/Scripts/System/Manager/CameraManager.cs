using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Data;

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

        if (vCams[(int)_followType].GetComponent<CinemachineOrbitalFollow>() != null)
        {
            StopAllCoroutines();
            StartCoroutine(AutoCentering(vCams[(int)_followType].GetComponent<CinemachineOrbitalFollow>()));
        }

        for (int i = 0; i < vCams.Count; i++)
        {
            if (vCams[i] == null) continue;

            if(_follow != null)
            {
                vCams[i].transform.position = _follow.transform.position;
                vCams[i].transform.rotation = _follow.transform.rotation;
            }

            vCams[i].gameObject.SetActive(i == (int)_followType);
        }
    }

    public void SetCameraCenter(CameraType _type, Transform _player)
    {
        vCams[(int)_type].GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Center = _player.rotation.eulerAngles.y;
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

    private IEnumerator AutoCentering(CinemachineOrbitalFollow _orbitalCamera)
    {
        _orbitalCamera.HorizontalAxis.Recentering.Enabled = true;
        _orbitalCamera.VerticalAxis.Recentering.Enabled = true;

        yield return new WaitForSeconds(1f);

        _orbitalCamera.HorizontalAxis.Recentering.Enabled = true;
        _orbitalCamera.VerticalAxis.Recentering.Enabled = true;

        yield break;
    }
}
