using UnityEngine;

public partial class Player : Character
{
    [Header("이동")]
    public float MoveSpeed = 2f;
    public float RotationSpeed = 15f;
    public float RunMultiply = 1.1f;
    public float AirControlMultiply = 0.5f;  // 공중에서의 이동 속도 계수

    [Header("점프")]
    public float JumpForce = 10f;
    public float GravityMultiply = 0.02f;
    public float FallMin = -10f;
    private const float JUMP_ABORT_SPEED = 10f;

    [Header("회피")]
    public float DodgeSpeed = 5f;

    private const float GRAVITY = -9.81f;
    private float yVelocity = 0f;
    private Vector3 dodgeDirection = Vector3.zero;

    public void CheckGround()
    {
        Vector3 origin = transform.position + (Vector3.up * 0.05f);
        Debug.DrawRay(origin, Vector3.down, Color.red, 0.1f);
        if (Physics.Raycast(origin, Vector3.down, 0.1f, Singleton.Get<LayerManager>().GetLayerMask(LayerType.Ground)))
            SetFlag(StateFlags.Grounded);
        else
            SetFlag(StateFlags.Grounded, false);
    }

    public Vector3 VerticalMove()
    {
        if (!IsFlagged(StateFlags.Grounded))
        {
            if (!IsFlagged(StateFlags.Jump) && yVelocity > 0.0f)
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
            
            if (IsFlagged(StateFlags.Jump))
            {
                yVelocity = JumpForce;
            }
            else
                yVelocity = 0f;
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

        // 지면에 있을 때는 기본 속도, 공중에 있을 때는 AirControlMultiply를 적용
        float currentSpeed = MoveSpeed * (IsFlagged(StateFlags.Run) ? RunMultiply : 1f);
        if (!IsFlagged(StateFlags.Grounded))
            currentSpeed *= AirControlMultiply;

        return moveDirection * currentSpeed * Time.fixedDeltaTime;
    }

    public void SetMove(Vector3 _horizontal, Vector3 _vertical)
    {
        if (IsMovementLocked)
        {
            rigidbody.linearVelocity = Vector3.zero;
            return;
        }
        rigidbody.MovePosition(rigidbody.position + _horizontal + _vertical);
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
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * RotationSpeed * Time.deltaTime);
        
        // 회피 애니메이션은 DodgeState에서 설정
        SetFlag(StateFlags.Dodging);
    }

    public Vector3 DodgeMove()
    {
        return dodgeDirection * DodgeSpeed * Time.fixedDeltaTime;
    }
}
