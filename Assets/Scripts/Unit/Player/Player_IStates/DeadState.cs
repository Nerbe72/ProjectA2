using GameStuff;
using UnityEngine;

public class DeadState : IPlayerState
{
    public void Enter(Player _player)
    {
        _player.SetDeadAnimation();
        InputManager.IgnoreInput = true;
        InputManager.IgnoreUIInput = true;
        WindowStackManager.PopAllWindows();
    }

    public void Update(Player _player)
    {
        if (!_player.IsFlagged(StateFlags.Death))
        {
            _player.TransitionTo(new IdleState());
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _player.Respawn();
        }
    }

    public void FixedUpdate(Player _player) { }

    public void Exit(Player _player)
    {
        _player.ReleaseDeadAnimation();
        InputManager.IgnoreInput = false;
        InputManager.IgnoreUIInput = false;
    }

    public void OnJumpInput(Player _player)
    {

    }
}
