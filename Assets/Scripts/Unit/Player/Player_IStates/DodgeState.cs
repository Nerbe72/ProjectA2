using UnityEngine;

public class DodgeState : IPlayerState
{
    public void OnJumpInput(Player _player) { /* 회피 중 점프 불가 */ }

    private bool preparingNextDodge = false;

    public void Enter(Player _player)
    {
        _player.SetDodgeDirection();
        _player.SetDodgeAnimation();
        preparingNextDodge = false;
    }

    public void Update(Player _player)
    {
        // 애니메이션 재생 중일 때 바로 리턴
        if (_player.IsFlagged(StateFlags.Dodging))
        {
            // 다음 회피를 준비 중인지 확인
            if (_player.IsFlagged(StateFlags.Dodge))
            {
                preparingNextDodge = true;
            }
            return;
        }

        // 애니메이션이 끝났을 때 다음 동작 결정

        // 회피 키가 눌려있거나 다음 회피가 준비된 경우 - 연속 회피
        if (preparingNextDodge || _player.IsFlagged(StateFlags.Dodge))
        {
            preparingNextDodge = false;
            _player.SetDodgeDirection(); // 회피 방향 재설정
            _player.SetDodgeAnimation();
            return;
        }

        // 회피 종료 후 이동 상태로 전환
        if (_player.IsInputMoving())
        {
            _player.TransitionTo(new MoveState());
            return;
        }
        // 이동 입력이 없으면 대기 상태로
        else
        {
            _player.TransitionTo(new IdleState());
            return;
        }
    }

    public void FixedUpdate(Player _player)
    {
        if (_player.IsFlagged(StateFlags.Dodging) || preparingNextDodge)
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
