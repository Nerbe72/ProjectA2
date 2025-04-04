using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum CameraType
{
    POV,
    Target,
    Sit,
}

public class CameraManager : MonoBehaviour
{
    [HideInInspector] public Camera main;
    [SerializeField] private List<GameObject> vCams;

    private void Awake()
    {
        if (SingletonManager.cameraManager != null)
        {
            Destroy(gameObject);
            return;
        }

        SingletonManager.cameraManager = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += ChangeCameraToMain;
        SceneManager.sceneLoaded += ResetMousePosition;
        SceneManager.sceneLoaded += ResetCameraPosition;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= ChangeCameraToMain;
    }

    private void ChangeCameraToMain(Scene _scene, LoadSceneMode _mode)
    {
        main = Camera.main;
    }

    public void ResetCameraPosition(Scene _scene, LoadSceneMode _mode)
    {
        try
        {
            for (int i = 0; i < vCams.Count; i++)
            {
                vCams[i].GetComponent<CinemachineCamera>().ForceCameraPosition(SingletonManager.player.transform.position, SingletonManager.player.transform.rotation);
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

    //타겟 상태에 따른 카메라 설정
    public void SetFollow(CameraType _type, Target _target = null)
    {
        if (_type == CameraType.Target && _target == null) return;

        SwitchCamera((int)_type);

        if (_type != CameraType.Target) return;

        vCams[(int)_type].GetComponent<CinemachineCamera>().LookAt = _target.transform;
    }

    private void SwitchCamera(int _followType)
    {
        //변경사항 없음
        if (vCams[_followType].activeSelf)
            return;

        if (_followType == 0)
        {
            CinemachinePOV pov = vCams[0].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>();
            Transform composerT = vCams[1].transform;
            Vector3 composerForward = composerT.forward;

            Transform povT = vCams[0].transform;
            povT.rotation = Quaternion.LookRotation(composerForward);

            pov.m_HorizontalAxis.Value = povT.eulerAngles.y;
            pov.m_VerticalAxis.Value = povT.eulerAngles.x;
        }

        for (int i = 0; i < vCams.Count; i++)
        {
            vCams[i].SetActive(i == _followType);
        }
    }

    public void OffAllCam()
    {
        for (int i = 0; i < vCams.Count; i++)
        {
            vCams[i].SetActive(false);
        }
    }

    public void ResetCam()
    {
        for (int i = 0; i < vCams.Count; i++)
        {
            vCams[i].SetActive(i == 0);
        }
    }
}
