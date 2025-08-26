using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class VCams : MonoBehaviour
{
    [SerializeField] private List<CinemachineCamera> vCams;

    public void SetCam(PlateType _type)
    {
        for (int i = 0; i < vCams.Count; i++)
        {
            vCams[i].gameObject.SetActive(i == (int)_type);
        }
    }
}
