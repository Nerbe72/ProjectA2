using GameStuff;

public class MoveState : IPlayerState
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
        _player.yVelocity = 0f;

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
            _player.TransitionTo(new AttackState());
            return;
        }

        if (_player.IsFlagged(StateFlags.Dodge) && _player.IsFlagged(StateFlags.Grounded))
        {
            _player.TransitionTo(new DodgeState());
            return;
        }

        if (!_player.IsInputMoving() && _player.IsFlagged(StateFlags.Grounded))
        {
            _player.TransitionTo(new IdleState());
            return;
        }


    }

    public void FixedUpdate(Player _player)
    {
        _player.SetMoveAnimation();
        _player.SetRotation();
        _player.SetMove(_player.HorizontalMove(), _player.VerticalMove());
        //지평 좌표 동기화
    }

    public void Exit(Player _player)
    {

    }
}
