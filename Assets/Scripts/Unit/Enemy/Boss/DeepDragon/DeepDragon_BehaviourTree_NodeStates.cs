using UnityEngine;

using GameStuff;

public partial class DeepDragon : Boss
{
    private float distanceToPlayer;

    #region Conditions
    private bool CheckDead()
    {
        return isDead;
    }

    /// <summary>
    /// 애니메이터의 현재 상태가 "Attack" 태그를 가지고 있는지 확인합니다.
    /// 보스의 모든 공격 애니메이션 상태에 "Attack" 태그를 반드시 추가해야 합니다.
    /// </summary>
    private bool IsAttacking()
    {
        // 0번 레이어(Base Layer)의 현재 애니메이션 상태 정보를 가져옵니다.
        return isAttacking;
    }

    private bool IsAiDisabled()
    {
        return !isAIActive;
    }

    private bool IsPlayerInPatternDistance(AttackPatternDistance pattern)
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        return (distanceToPlayer >= pattern.MinDistance && distanceToPlayer <= pattern.MaxDistance);
    }

    private bool IsInPattern1Distance() => IsPlayerInPatternDistance(pattern1Melee);
    private bool IsInPattern2Distance() => IsPlayerInPatternDistance(pattern2MidRange);
    private bool IsInPattern3Distance() => IsPlayerInPatternDistance(pattern3LongRange);
    private bool IsInPattern4Distance() => IsPlayerInPatternDistance(pattern4Charge);
    #endregion

    #region Actions
    private NodeStates DoDead()
    {
        Debug.Log("DoDead");
        if (agent.enabled)
        {
            agent.ResetPath();
            agent.SetDestination(transform.position);
        }
        // 사망 애니메이션은 한 번만 트리거해야 합니다.
        if (!animator.GetBool("Dead"))
        {
            animator.SetBool("Dead", true); // Nightmare 컨트롤러가 bool을 사용할 경우
            animator.SetTrigger("Dead"); // Trigger를 사용할 경우
        }
        return NodeStates.SUCCESS;
    }

    /// <summary>
    /// 공격 중일 때 제자리에서 대기하도록 NavMeshAgent를 정지시킵니다.
    /// </summary>
    private NodeStates StopMovement()
    {
        Debug.Log("StopMovement");
        animator.SetBool(AnimationHash.GetHash(ActionType.Walk), false);
        animator.SetFloat(AnimationHash.GetHash(ActionType.Move), 0f);
        animator.SetFloat(AnimationHash.GetHash(ActionType.Side), 0f);
        agent.SetDestination(transform.position);
        return NodeStates.SUCCESS;
    }

    private NodeStates DoNothing()
    {
        if (agent.enabled)
            agent.SetDestination(transform.position);
        animator.SetBool(AnimationHash.GetHash(ActionType.Walk), false);
        animator.SetFloat(AnimationHash.GetHash(ActionType.Move), 0f);
        animator.SetFloat(AnimationHash.GetHash(ActionType.Side), 0f);
        return NodeStates.SUCCESS;
    }

    private NodeStates DoAttack(int patternIndex)
    {
        Debug.Log($"DoAttack Pattern{patternIndex}");
        headHitbox.Configure(AttackType.Physical, BossData.Damage, this);
        rightArmHitbox.Configure(AttackType.Physical, BossData.Damage, this);
        groundHitbox.Configure(AttackType.Physical, BossData.Damage, this);
        groundShock.Configure(AttackType.Physical, BossData.Damage, this);

        // Disable head collider and mark attacking
        SetAttacking();
        animator.SetBool(AnimationHash.GetHash(ActionType.Walk), false);
        animator.SetFloat(AnimationHash.GetHash(ActionType.Move), 0f);
        animator.SetFloat(AnimationHash.GetHash(ActionType.Side), 0f);
        Debug.Log($"Executing Attack Pattern {patternIndex}");
        transform.LookAt(player.transform.position);
        animator.SetInteger("Pattern", patternIndex);
        animator.SetTrigger("Attack");
        agent.SetDestination(transform.position);  // Hold position during attack
        return NodeStates.SUCCESS;
    }

    private NodeStates DoPattern1() => DoAttack(0);
    private NodeStates DoPattern2() => DoAttack(1);
    private NodeStates DoPattern3() => DoAttack(2);
    private NodeStates DoPattern4() => DoAttack(3);

    private NodeStates DoChaseAndReposition()
    {
        Debug.Log("DoChaseAndReposition");
        if (Time.time > chasePhaseStartTime + ChasePhaseDuration)
            return NodeStates.SUCCESS;

        transform.LookAt(player.transform.position);
        Vector3 velocity = agent.velocity;
        Vector3 localVel = transform.InverseTransformDirection(velocity);
        float moveVal = Mathf.Clamp(localVel.z, -1f, 1f);
        float sideVal = Mathf.Clamp(localVel.x, -1f, 1f);
        animator.SetFloat(AnimationHash.GetHash(ActionType.Move), moveVal);
        animator.SetFloat(AnimationHash.GetHash(ActionType.Side), sideVal);

        int movementIndex = Mathf.FloorToInt((Time.time - chasePhaseStartTime) / MovementChangeInterval);
        if (movementIndex != currentMovementIndex)
        {
            currentMovementIndex = movementIndex;
            int randomMoveType = Random.Range(0, 4);
            Vector3 targetPosition = Vector3.zero;
            Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
            Vector3 right = transform.right;
            switch (randomMoveType)
            {
                case 0: targetPosition = player.transform.position - dirToPlayer * 2f; break;
                case 1: targetPosition = transform.position - dirToPlayer * 5f; break;
                case 2: targetPosition = transform.position - right * 5f; break;
                case 3: targetPosition = transform.position + right * 5f; break;
            }
            agent.SetDestination(targetPosition);
        }
        return NodeStates.RUNNING;
    }

    private NodeStates InitChasePhase()
    {
        Debug.Log("InitChasePhase");
        chasePhaseStartTime = Time.time;
        currentMovementIndex = -1;
        agent.speed = BossData.Speed;
        if (agent.enabled)
            agent.isStopped = false;
        agent.updateRotation = false;
        return NodeStates.SUCCESS;
    }
    #endregion
}