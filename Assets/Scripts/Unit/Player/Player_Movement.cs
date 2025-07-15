using UnityEngine;

public partial class Player : Character
{
    private const float JUMP_ABORT_SPEED = 10f;
    private const float GRAVITY = -9.81f;

    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float RotationSpeed = 15f;
    public float RunMultiply = 1.1f;
    public float AirControlMultiply = 0.5f;

    [Header("Jump")]
    public float JumpForce = 7f;
    public float GravityMultiply = 0.02f;
    public float FallMin = -8f;

    [Header("Ground Check")]
    public float GroundCheckRadiusMultiplier = 0.9f;
    public float GroundCheckYOffset = 0.05f;
    public float GroundCheckDistance = 0.2f;

    [Header("Dodge")]
    public float DodgeSpeed = 5f;

    public float yVelocity = 0f;
    private Vector3 dodgeDirection = Vector3.zero;

    public void CheckGround()
    {
        if (IsLoadingScene) { SetFlag(StateFlags.Grounded, true); return; }
        if (yVelocity > 0.0f) { SetFlag(StateFlags.Grounded, false); return; }

        float sphereRadius = PlayerRadius * GroundCheckRadiusMultiplier;
        Vector3 origin = transform.position + collider.center + Vector3.up * ((-collider.height / 2f) + sphereRadius + GroundCheckYOffset);

        int groundMask = Singleton.Get<LayerManager>().GetLayerMask(LayerType.Ground);

        bool grounded = Physics.SphereCast(origin, sphereRadius, Vector3.down, out _, GroundCheckDistance, groundMask);
        SetFlag(StateFlags.Grounded, grounded);
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
                yVelocity = 0f;
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

        SetFlag(StateFlags.Dodging);
    }

    public Vector3 DodgeMove()
    {
        return dodgeDirection * DodgeSpeed * Time.fixedDeltaTime;
    }
}
