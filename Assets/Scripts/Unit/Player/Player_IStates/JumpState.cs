using UnityEngine;

using GameStuff;
using SoundStuff;

public class JumpState : IPlayerState
{
    public void OnJumpInput(Player _player)
    {
        if (_player.IsFlagged(StateFlags.Grounded))
        {
            _player.TransitionTo(new JumpState());
        }
    }

    public void Enter(Player _player)
    {
        _player.yVelocity = _player.JumpForce;
        _player.SetJumpAnimation();
    }

    public void Update(Player _player)
    {
        if (_player.IsFlagged(StateFlags.Grounded) && _player.yVelocity <= 0f)
        {
            _player.TransitionTo(_player.IsInputMoving() ? new MoveState() : new IdleState());
        }
    }

    public void FixedUpdate(Player _player)
    {
        _player.SetRotation();
        _player.SetMove(_player.HorizontalMove(), _player.VerticalMove());
        _player.SetMoveAnimation();
    }

    public void Exit(Player _player)
    {
        _player.ResetJumpAnimation();
    }
}