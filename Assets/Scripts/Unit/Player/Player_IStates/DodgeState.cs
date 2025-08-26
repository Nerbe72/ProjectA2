using UnityEngine;

using GameStuff;
using SoundStuff;

public class DodgeState : IPlayerState
{
    public void OnJumpInput(Player _player) { /* 회피 중 점프 불가 */ }

    private bool preparingNextDodge = false;

    public void Enter(Player _player)
    {
        _player.SetDodging();
        //_player.SetDodgeDirection();
        _player.SetDodgeAnimation();
        _player.PlayActionSound(PlayerActionType.Dodge);
        preparingNextDodge = false;
    }

    public void Update(Player _player)
    {
        if (_player.IsFlagged(StateFlags.Dodging))
        {
            if (_player.IsFlagged(StateFlags.Dodge) && !preparingNextDodge)
            {
                preparingNextDodge = true;
            }
            return;
        }

        if (preparingNextDodge || _player.IsFlagged(StateFlags.Dodge))
        {
            preparingNextDodge = false;
            //_player.SetDodgeDirection();
            _player.SetDodgeAnimation();
            return;
        }

        if (_player.IsInputMoving())
        {
            _player.TransitionTo(new MoveState());
            return;
        }
        else
        {
            _player.TransitionTo(new IdleState());
            return;
        }
    }

    public void FixedUpdate(Player _player)
    {
        if (_player.IsFlagged(StateFlags.Dodging))
        {
            _player.ResetMoveAnimation();
            _player.SetMove(_player.DodgeMove(), _player.VerticalMove());
        }
        else
        {
            _player.SetMove(Vector3.zero, _player.VerticalMove());
        }
    }

    public void Exit(Player _player)
    {
        _player.ReleaseAttackAnimation();
        _player.ResetDodging();
        preparingNextDodge = false;
    }
}
