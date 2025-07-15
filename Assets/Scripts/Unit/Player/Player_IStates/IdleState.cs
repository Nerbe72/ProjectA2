using UnityEngine;

public class IdleState : IPlayerState
{
    public void OnJumpInput(Player _player)
    {
        _player.TransitionTo(new JumpState());
    }

    public void Enter(Player _player)
    {
        _player.yVelocity = 0f;

    }

    public void Update(Player _player)
    {
        if (_player.IsFlagged(StateFlags.Hit))
        {
            _player.TransitionTo(new HitState());
            return;
        }

        if (_player.IsInputMoving())
        {
            _player.TransitionTo(new MoveState());
            return;
        }


        if (_player.IsFlagged(StateFlags.Attack) && _player.IsFlagged(StateFlags.Grounded))
        {
            _player.TransitionTo(new AttackState());
            return;
        }

        if (_player.IsFlagged(StateFlags.Dodge) && _player.IsFlagged(StateFlags.Grounded))
        {
            _player.TransitionTo(new DodgeState());
            return;
        }
    }

    public void FixedUpdate(Player _player)
    {
        _player.SetRotation();
        _player.SetMove(Vector3.zero, _player.VerticalMove());
        _player.ResetMoveAnimation();
    }

    public void Exit(Player _player)
    {
        _player.ResetInput();
    }

}
