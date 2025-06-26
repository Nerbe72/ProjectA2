using UnityEngine;

public class JumpState : IPlayerState
{
    private bool isLanding = false;
    private bool jumpStarted = false;

    public void Enter(Player _player)
    {
        jumpStarted = false;
        isLanding = false;

        // 이미 공중에 있는 경우 점프 처리 건너뛰기
        if (!_player.IsFlagged(StateFlags.Grounded))
        {
            jumpStarted = true;
            return;
        }

        // 점프 애니메이션 설정 및 점프 상태 유지
        _player.SetJumpAnimation();
        _player.SetFlag(StateFlags.Jump, true);
    }

    public void Update(Player _player)
    {
        if (_player.IsFlagged(StateFlags.Hit))
        {
            _player.TransitionTo(new HitState());
            return;
        }

        // 공중에서 회피 가능
        if (_player.IsFlagged(StateFlags.Dodge))
        {
            _player.TransitionTo(new DodgeState());
            return;
        }

        // 공중에서 공격 가능
        if (_player.IsFlagged(StateFlags.Attack))
        {
            _player.TransitionTo(new AttackState());
            return;
        }

        // 점프 초기 상승 단계 처리
        if (!jumpStarted)
        {
            // 점프 후 일정 높이에 도달하거나 상승이 끝나면 Jump 플래그 해제
            if (!_player.IsFlagged(StateFlags.Grounded))
            {
                jumpStarted = true;
                _player.SetFlag(StateFlags.Jump, false); // 초기 점프 힘 적용 후 플래그 해제
            }
        }

        // 착지 처리
        if (jumpStarted && _player.IsFlagged(StateFlags.Grounded) && !isLanding)
        {
            isLanding = true;
            _player.SetLandingAnimation();
            // 착지 애니메이션 후 일정 시간 뒤 다음 상태로 전환
            _player.StartCoroutine(LandingDelay(_player));
        }
    }

    private System.Collections.IEnumerator LandingDelay(Player _player)
    {
        // 착지 애니메이션 재생 시간 기다림
        yield return new WaitForSeconds(0.3f);

        _player.ResetLandingAnimation();

        // 착지 후 이동 또는 대기 상태로 전환
        if (_player.IsInputMoving())
        {
            _player.TransitionTo(new MoveState());
        }
        else
        {
            _player.TransitionTo(new IdleState());
        }
    }

    public void FixedUpdate(Player _player)
    {
        // 점프/낙하 중에도 제한된 이동 가능
        if (!isLanding)
        {
            _player.SetMoveAnimation();
            _player.SetRotation();
            // 낙하 중에는 이동 제한을 줄임
            float moveFactor = _player.IsFlagged(StateFlags.Falling) ? 0.7f : 0.8f;
            _player.SetMove(_player.HorizontalMove() * moveFactor, _player.VerticalMove());
        }
        else
        {
            // 착지 중에는 이동 불가
            _player.ResetMoveAnimation();
            _player.SetMove(Vector3.zero, _player.VerticalMove());
        }
    }

    public void Exit(Player _player)
    {
        _player.ResetJumpAnimation();
        _player.ResetLandingAnimation();
        _player.SetFlag(StateFlags.Jumping, false);
    }
}