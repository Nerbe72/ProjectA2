public interface IPlayerState
{
    public void OnJumpInput(Player _player);
    public void Enter(Player _player);

    public void Update(Player _player);

    public void FixedUpdate(Player _player);

    public void Exit(Player _player);
}
