using System.Collections;
using UnityEngine;

public partial class Player : Character
{
    private Vector3 movementInput;

    private StateFlags isState = StateFlags.None;

    public StateFlags IsState { get { return isState; } }

    public Vector3 MovementInput { get { return movementInput; } }

    private WaitForSeconds attackInputWait;
    private Coroutine attackWaitCoroutine;
    private const float attackInputDuration = 0.03f;

    private void InitInput()
    {
        attackInputWait = new WaitForSeconds(attackInputDuration);
    }

    public void CheckNormalInputs()
    {
        Singleton.Get<CameraManager>().IgnoreCameraController(InputManager.IgnoreInput);

        if (InputManager.IgnoreInput)
        {
            ResetInput();
            return;
        }

        // 기본 이동 입력
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.z = Input.GetAxisRaw("Vertical");

        // 달리기
        if (Input.GetKey(KeyCode.LeftShift))
            SetFlag(StateFlags.Run);
        else
            SetFlag(StateFlags.Run, false);

        // 점프
        // 점프
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpInput();
        }

        // 공격
        if (Input.GetMouseButtonDown(0))
        {
            if (weaponInstance != null)
            {
                if (attackWaitCoroutine != null)
                    StopCoroutine(attackWaitCoroutine);

                attackWaitCoroutine = StartCoroutine(AttackWait());
            }
        }

        // 회피
        if (Input.GetMouseButtonDown(1))
            SetFlag(StateFlags.Dodge);
        else
            SetFlag(StateFlags.Dodge, false);

        // 타겟팅
        if (Input.GetMouseButtonUp(2))
            SetFlag(StateFlags.Targeted);
        else
            SetFlag(StateFlags.Targeted, false);

        if (Input.GetKeyUp(KeyCode.F))
        {
            var target = Singleton.Get<InteractManager>().GetCurrent();
            if (target == null) return;

            target.DoAction();
        }

        if (Input.GetKeyUp(KeyCode.G))
        {
            Singleton.Get<InteractManager>().SwitchInteract();
        }
    }

    private void CheckUIInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            //뽑기가 진행중이면 무시
            if (Singleton.Get<GachaUI>().IsGachaRunning) return;

            WindowStackManager.PopWindow();
        }

        if (InputManager.IgnoreUIInput) return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            var top = WindowStackManager.PeekTopWindow();
            var window = Singleton.Inventory;
            if (window.gameObject.activeSelf && top != null && (top is WindowBase) == window.gameObject)
                WindowStackManager.PopWindow();
            else if (!window.gameObject.activeSelf)
                (window as IWindowStack)?.ShowWindow();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            ShowUI<PlayerStatusUI>();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            //뽑기가 진행중이면 무시
            if (Singleton.Get<GachaUI>().IsGachaRunning) return;

            ShowUI<GachaUI>();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ShowUI<QuestUI>();
        }
    }

    private void ShowUI<T>() where T : MonoBehaviour, IWindowStack
    {
        var top = WindowStackManager.PeekTopWindow();
        var window = Singleton.Get<T>();
        if (window.gameObject.activeSelf && top != null && (top as WindowBase) == window.gameObject)
            WindowStackManager.PopWindow();
        else if (!window.gameObject.activeSelf)
            (window as IWindowStack)?.ShowWindow();
    }

    private IEnumerator AttackWait()
    {
        SetFlag(StateFlags.Attack);

        yield return attackInputWait;

        SetFlag(StateFlags.Attack, false);
    }

    public void ResetInput()
    {
        movementInput = Vector3.zero;
        SetFlag(StateFlags.Run, false);
        SetFlag(StateFlags.Dodge, false);
        SetFlag(StateFlags.Attack, false);
    }

    public void SetFlag(StateFlags _flag, bool _isTure = true)
    {
        if (_isTure) isState |= _flag;
        else isState &= ~_flag;
    }

    public bool IsFlagged(StateFlags _flag)
    {
        return (isState & _flag) != 0;
    }

    public bool IsInputMoving()
    {
        return MovementInput.sqrMagnitude >= 0.01f;
    }
}
