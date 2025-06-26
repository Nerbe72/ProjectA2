using UnityEngine;

public partial class Player : Character
{
    #region 기본 애니메이션
    public void UpdateAnimationParameters()
    {
        animator.SetFloat(AnimationHash.GetHash(ActionType.Vertical), yVelocity);
        animator.SetBool(AnimationHash.GetHash(ActionType.Grounded), IsFlagged(StateFlags.Grounded));
    }

    public void SetMoveAnimation()
    {
        float move = movementInput.z * (IsFlagged(StateFlags.Run) ? RunMultiply : 1f);
        float side = movementInput.x * (IsFlagged(StateFlags.Run) ? RunMultiply : 1f);

        animator.SetFloat(AnimationHash.GetHash(ActionType.Move), Mathf.Lerp(animator.GetFloat(AnimationHash.GetHash(ActionType.Move)), move, 0.2f));
        animator.SetFloat(AnimationHash.GetHash(ActionType.Side), Mathf.Lerp(animator.GetFloat(AnimationHash.GetHash(ActionType.Side)), side, 0.2f));
    }

    public void ResetMoveAnimation()
    {
        animator.SetFloat(AnimationHash.GetHash(ActionType.Move), 0);
        animator.SetFloat(AnimationHash.GetHash(ActionType.Side), 0);
    }

    public void SetAttackAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Attack), true);
        SetFlag(StateFlags.Attacking);
    }

    public void ReleaseAttackAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Attack), false);
    }

    public void SetDodgeAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Dodge), true);
        SetFlag(StateFlags.Dodging);
    }

    public void SetJumpAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Jump), true);
    }

    public void ResetJumpAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Jump), false);
    }

    public void SetFallingAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Fall), true);
    }

    public void ResetFallingAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Fall), false);
    }

    public void SetLandingAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Land), true);
    }

    public void ResetLandingAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Land), false);
    }

    public void SetHitAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Hit), true);
    }

    public void ResetHitAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Hit), false);
    }

    public void SetSitAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Sit), true);
    }

    public void ResetSitAnimation()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Sit), false);
    }
    #endregion

    // //////////////////////////////////////////////////////////////////////

    #region 애니메이션 이벤트
    public void ResetAttack()
    {
        SetFlag(StateFlags.Attack, false);
        ReleaseAttackAnimation();
    }

    public void ResetDodge()
    {
        animator.SetBool(AnimationHash.GetHash(ActionType.Dodge), false);
    }

    public void SetAttacking()
    {
        SetFlag(StateFlags.Attacking);
    }

    public void ResetAttacking()
    {
        SetFlag(StateFlags.Attacking, false);
    }

    public void SetDodging()
    {
        SetFlag(StateFlags.Dodging);
    }

    public void ResetDodging()
    {
        SetFlag(StateFlags.Dodging, false);
    }

    public void ResetJumping()
    {
        SetFlag(StateFlags.Jump, false);
        ResetJumpAnimation();
        SetFlag(StateFlags.Jumping, false);
    }

    public void SetHitting()
    {
        ResetHitAnimation();
        SetFlag(StateFlags.Hitting);
    }

    public void ResetHitting()
    {

        SetFlag(StateFlags.Hitting, false);
    }
    #endregion
}
