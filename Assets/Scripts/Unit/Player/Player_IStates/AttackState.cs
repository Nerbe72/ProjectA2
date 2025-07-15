using UnityEngine;

public class AttackState : IPlayerState
{
    public void OnJumpInput(Player _player) { /* 공격 중 점프 불가 */ }

    public void Enter(Player _player)
    {
        _player.SetAttackAnimation();
    }

    public void Update(Player _player)
    {
        if (_player.IsFlagged(StateFlags.Hit))
        {
            _player.TransitionTo(new HitState());
            return;
        }

        if (_player.IsFlagged(StateFlags.Attack))
        {
            _player.SetAttackAnimation();
        }

        // 공격 애니메이션이 끝났는지 확인
        if (_player.IsFlagged(StateFlags.Attacking)) return;

        if (_player.IsInputMoving())
        {
            _player.TransitionTo(new MoveState());
            return;
        }

        // 그 외의 경우 대기 상태로 전환
        _player.TransitionTo(new IdleState());
    }

    public void FixedUpdate(Player _player)
    {
        // 공격 중에도 제한된 이동 가능하게
        Vector3 limitedMovement = _player.HorizontalMove() * 0.3f; // 이동 속도 감소
        _player.SetMove(limitedMovement, _player.VerticalMove());
    }

    public void Exit(Player _player)
    {
        _player.ReleaseAttackAnimation();
        _player.ResetAttacking();
    }
}
