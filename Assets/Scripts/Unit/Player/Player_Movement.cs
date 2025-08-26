using UnityEngine;

using GameStuff;
using SoundStuff;

public partial class Player : Character
{
    private const float JUMP_ABORT_SPEED = 10f;
    private const float GRAVITY = -9.81f;

    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float RotationSpeed = 15f;
    public float RunMultiply = 1.1f;
    public float AirControlMultiply = 0.5f;
    public float FootstepWaitTime = 100f;

    [Header("Jump")]
    public float JumpForce = 7f;
    public float GravityMultiply = 0.02f;
    public float FallMin = -8f;

    [Header("Ground Check")]
    [SerializeField] [Range(0f, 1f)] private float distanceToGround;
    public float GroundCheckRadiusMultiplier = 0.9f;
    public float GroundCheckYOffset = 0.05f;
    public float GroundCheckDistance = 0.2f;

    [Header("Dodge")]
    public float DodgeSpeed = 5f;

    public float yVelocity = 0f;
    private Vector3 dodgeDirection = Vector3.zero;
    private Vector3 groundNormal = Vector3.up;

    private bool leftIK = false;
    private bool rightIK = false;

    private void OnAnimatorIK(int layerIndex)
    {
        if (photonView == null || !photonView.IsMine) return;

        //animator.SetLookAtWeight(1f);
        //if (transform.forward != null)
        //    animator.SetLookAtPosition(m_trsLookPos.position);

        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0.7f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.7f);

        LayerMask layers = (int)FootstepType.All;

        if (Physics.Raycast(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down, out RaycastHit hitL,
            distanceToGround + 1f, layers))
        {
            Vector3 footPos = hitL.point;
            footPos.y += distanceToGround;
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPos);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hitL.normal), hitL.normal));
            
            if (leftIK)
            {
                Singleton.Get<SoundManager>().PlayFootstepSound(hitL.collider.gameObject.layer, true, FootstepWaitTime);
                leftIK = false;
            }
        }
        else
        {
            leftIK = true;
        }

        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0.7f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.7f);

        if (Physics.Raycast(animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down, out RaycastHit hitR,
            distanceToGround + 1f, layers))
        {
            Vector3 footPos = hitR.point;
            footPos.y += distanceToGround;
            animator.SetIKPosition(AvatarIKGoal.RightFoot, footPos);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hitR.normal), hitR.normal));
            
            if (rightIK)
            {
                Singleton.Get<SoundManager>().PlayFootstepSound(hitR.collider.gameObject.layer, false, FootstepWaitTime);
                rightIK = false;
            }
        }
        else
        {
            rightIK = true;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider == null) return;

        float sphereRadius = capsuleCollider.radius * GroundCheckRadiusMultiplier;
        Vector3 origin = transform.position + capsuleCollider.center + Vector3.up * ((-capsuleCollider.height / 2f) + sphereRadius + GroundCheckYOffset);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, sphereRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin + Vector3.down * GroundCheckDistance, sphereRadius);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(origin, origin + Vector3.down * GroundCheckDistance);
    }
#endif

    public void CheckGround()
    {
        if (IsLoadingScene) { SetFlag(StateFlags.Grounded, true); return; }
        if (yVelocity > 0.0f) { SetFlag(StateFlags.Grounded, false); return; }

        float sphereRadius = PlayerRadius * GroundCheckRadiusMultiplier;
        Vector3 origin = transform.position + collider.center + Vector3.up * ((-collider.height / 2f) + sphereRadius + GroundCheckYOffset);

        LayerMask layers = (int)FootstepType.All;

        if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, GroundCheckDistance, layers))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            bool isOnSlope = slopeAngle > 15f;
            
            if (slopeAngle > 60f)
            {
                SetFlag(StateFlags.Grounded, false);
                SetFlag(StateFlags.Slope, false);
                groundNormal = Vector3.up;
                return;
            }
            
            SetFlag(StateFlags.Grounded, true);
            SetFlag(StateFlags.Slope, isOnSlope);
            
            // 경사면 이동 계산용
            groundNormal = hit.normal;
        }
        else
        {
            SetFlag(StateFlags.Grounded, false);
            SetFlag(StateFlags.Slope, false);
            groundNormal = Vector3.up;
        }
    }

    public Vector3 VerticalMove()
    {
        if (!IsFlagged(StateFlags.Grounded))
        {
            if (yVelocity > 0.0f)
            {
                yVelocity -= JUMP_ABORT_SPEED * Time.deltaTime;
            }

            yVelocity = Mathf.Clamp(yVelocity + (GRAVITY * GravityMultiply), FallMin, JumpForce);

            if (yVelocity < 0)
                SetFlag(StateFlags.Falling);
            else
                SetFlag(StateFlags.Falling, false);
        }
        else
        {
            SetFlag(StateFlags.Falling, false);
            
            if (yVelocity < 0.0f)
            {
                // 떨림 방지
                yVelocity = Mathf.Lerp(yVelocity, 0f, Time.fixedDeltaTime * 10f);
                
                if (Mathf.Abs(yVelocity) < 0.01f)
                {
                    yVelocity = 0f;
                }
            }
        }

        return new Vector3(0f, yVelocity * Time.fixedDeltaTime, 0f);
    }

    public void ResetVertical()
    {
        yVelocity = 0f;
        SetFlag(StateFlags.Falling, false);
    }

    public void SetRotation()
    {
        if (movementInput == Vector3.zero) return;

        if (targetManager?.CurrentTarget != null && !IsFlagged(StateFlags.Run))
        {
            Vector3 targetedDirection = (targetManager.CurrentTarget.transform.position - transform.position).normalized;
            targetedDirection.y = 0f;

            Quaternion targetedRotation = Quaternion.LookRotation(targetedDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetedRotation, RotationSpeed * Time.deltaTime * 2f);

            return;
        }

        var cameraManager = Singleton.Get<CameraManager>();
        Vector3 cameraForward = cameraManager.GetCameraForward();
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraManager.GetCameraRight();
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 targetDirection = (cameraForward * movementInput.z + cameraRight * movementInput.x).normalized;

        if (targetDirection.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
    }

    public Vector3 HorizontalMove()
    {
        if (InputManager.IgnoreInput) return Vector3.zero;

        var cameraManager = Singleton.Get<CameraManager>();
        Vector3 cameraForward = cameraManager.GetCameraForward();
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraManager.GetCameraRight();
        cameraRight.y = 0f;
        cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * movementInput.z + cameraRight * movementInput.x).normalized;

        // 경사면에서 이동 방향을 지면 법선에 투영하여 보정
        if (IsFlagged(StateFlags.Grounded) && IsFlagged(StateFlags.Slope))
        {
            moveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
        }

        float currentSpeed = MoveSpeed * (IsFlagged(StateFlags.Run) ? RunMultiply : 1f);
        if (!IsFlagged(StateFlags.Grounded))
            currentSpeed *= AirControlMultiply;

        if (currentSpeed >= MoveSpeed * RunMultiply * 0.7f)
        {
            Singleton.Get<PostProcessingManager>().SetRunningEffect();
        }
        else
        {
            Singleton.Get<PostProcessingManager>().ResetRunningEffect();
        }

        return moveDirection * currentSpeed * Time.fixedDeltaTime;
    }

    public void SetMove(Vector3 _horizontal, Vector3 _vertical)
    {
        if (IsMovementLocked)
        {
            rigidbody.linearVelocity = Vector3.zero;
            return;
        }

        // 이동 플랫폼 속도 보상 (Y값 제외)
        Vector3 platformVelocity = Vector3.zero;
        if (IsFlagged(StateFlags.Grounded))
        {
            float sphereRadius = PlayerRadius * GroundCheckRadiusMultiplier;
            Vector3 origin = transform.position + collider.center + Vector3.up * ((-collider.height / 2f) + sphereRadius + GroundCheckYOffset);
            
            if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit, GroundCheckDistance, Singleton.Get<LayerManager>().GetLayerMask(LayerType.Ground)))
            {
                // Rigidbody가 있는 움직이는 오브젝트인 경우 속도 보상
                Rigidbody platformRb = hit.collider.GetComponent<Rigidbody>();
                if (platformRb != null && !platformRb.isKinematic)
                {
                    // Y값을 0으로 만들어 수직 밀어내기 방지
                    Vector3 horizontalVelocity = platformRb.linearVelocity;
                    horizontalVelocity.y = 0f;
                    platformVelocity = horizontalVelocity * Time.fixedDeltaTime;
                }
            }
        }

        Vector3 finalPosition = rigidbody.position + _horizontal + _vertical + platformVelocity;
    
        finalPosition = ClampPositionToGround(finalPosition);
        
        rigidbody.MovePosition(finalPosition);
        rigidbody.linearVelocity = Vector3.zero;
    }

    public void SetDodgeDirection()
    {
        if (IsInputMoving())
        {
            var cameraManager = Singleton.Get<CameraManager>();
            Vector3 cameraForward = cameraManager.GetCameraForward();
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraManager.GetCameraRight();
            cameraRight.y = 0f;
            cameraRight.Normalize();

            dodgeDirection = (cameraForward * movementInput.z + cameraRight * movementInput.x).normalized;
        }
        else
        {
            dodgeDirection = transform.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(dodgeDirection);
        transform.rotation = targetRotation;
    }

    public Vector3 DodgeMove()
    {
        return dodgeDirection * DodgeSpeed * Time.fixedDeltaTime;
    }

    // 지면 높이 Clamp 헬퍼 함수
    private Vector3 ClampPositionToGround(Vector3 _targetPosition)
    {
        float smallRadius = PlayerRadius * 0.05f;
        Vector3 origin = transform.position + collider.center + Vector3.up * ((-collider.height / 2f) + smallRadius + GroundCheckYOffset);
        
        if (Physics.SphereCast(origin, smallRadius, Vector3.down, out RaycastHit hit, GroundCheckDistance * 2f, (int)FootstepType.All))
        {
            float minY = hit.point.y + GroundCheckYOffset;
            if (_targetPosition.y < minY)
            {
                _targetPosition.y = minY;
            }
        }
        
        return _targetPosition;
    }
}
