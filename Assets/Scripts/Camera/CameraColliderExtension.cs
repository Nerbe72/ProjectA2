using UnityEngine;
using Unity.Cinemachine;

public class CameraColliderExtension : CinemachineExtension
{
    [SerializeField] LayerMask CollisionLayers;
    [SerializeField] float CameraRadius = 0.1f;
    [SerializeField] float MinDistanceFromTarget = 0.5f;

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body) return;
        Vector3 targetPos = state.ReferenceLookAt;
        Vector3 desiredPos = state.RawPosition;
        Vector3 dir = desiredPos - targetPos;
        float dist = dir.magnitude;
        if (dist <= MinDistanceFromTarget) return;
        dir /= dist;
        if (Physics.SphereCast(targetPos, CameraRadius, dir, out var hit, dist, CollisionLayers))
        {
            Vector3 correctedPos = hit.point - dir * CameraRadius;
            state.PositionCorrection += correctedPos - desiredPos;
        }
    }
} 