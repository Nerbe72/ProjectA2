using UnityEngine;

public class HitState : IPlayerState
{
    //private float hitStunTime = 0.3f;
    //private float currentHitStunTime = 0f;

    public void Enter(Player _player)
    {
        _player.SetHitAnimation();
        _player.SetFlag(StateFlags.Hitting);
    }

    public void Update(Player _player)
    {
        if (_player.IsFlagged(StateFlags.Hitting)) return;

        _player.TransitionTo(new IdleState());
    }

    public void FixedUpdate(Player _player)
    {
        _player.SetMove(Vector3.zero, _player.VerticalMove());
    }

    public void Exit(Player _player)
    {
        _player.SetFlag(StateFlags.Hit, false);
    }
}
