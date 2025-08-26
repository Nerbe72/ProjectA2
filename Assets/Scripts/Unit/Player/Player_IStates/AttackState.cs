using UnityEngine;

using GameStuff;

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

        _player.TransitionTo(new IdleState());
    }

    public void FixedUpdate(Player _player)
    {
        _player.SetMove(Vector3.zero, _player.VerticalMove());
    }

    public void Exit(Player _player)
    {
        _player.ReleaseAttackAnimation();
        _player.ResetAttacking();
    }
}
