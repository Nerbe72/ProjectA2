using System.Collections;
using UnityEngine;

public partial class Goblin : Enemy
{
    private Coroutine hitStunCoroutine = null;

    private bool CheckDead()
    {
        return isDead;
    }

    private bool CheckHit()
    {
        return isHit;
    }

    private bool CheckIsPlayerInAttackDistance()
    {
        return (DistanceFromPlayer() <= EnemyData.AttackDistance);
    }

    private bool CheckSpawnDistance()
    {
        return CheckSpawnDistanceOut(EnemyData.DistanceLimit);
    }

    private bool CheckAtSpawnPoint()
    {
        return Vector3.Distance(transform.position, SpawnPoint_Enemy) <= 0.1f;
    }

    private bool CheckReturning()
    {
        return isReturning;
    }

    private bool CheckChasing()
    {
        return isChasing;
    }

    private bool CheckAttacking()
    {
        return isAttacking;
    }

    private bool CheckChaseOrHit()
    {
        return CheckPlayerInSight() || isHit;
    }

    private NodeStates DoDead()
    {
        if (isDying)
        {
            animator.SetBool(AnimationHash.GetHash(ActionType.Dead), false);
            return NodeStates.SUCCESS;
        }

        animator.SetBool(AnimationHash.GetHash(ActionType.Dead), true);
        return NodeStates.SUCCESS;
    }

    private NodeStates DoReturnSpawnPoint()
    {
        agent.isStopped = false;
        agent.SetDestination(SpawnPoint_Enemy);
        animator.SetBool(AnimationHash.GetHash(ActionType.Move), true);
        isReturning = true;
        return NodeStates.SUCCESS;
    }

    private NodeStates DoChase()
    {
        agent.isStopped = false;
        agent.speed = EnemyData.Speed;

        if (currentTarget == null) return NodeStates.FAILURE;

        Vector3 dir = (transform.position - currentTarget.transform.position).normalized;
        Vector3 targetPosition = currentTarget.transform.position + dir * (EnemyData.AttackDistance - 0.05f);
        agent.SetDestination(targetPosition);
        animator.SetBool(AnimationHash.GetHash(ActionType.Move), true);
        isChasing = true;

        return NodeStates.SUCCESS;
    }

    private NodeStates DoAttack()
    {
        isAttacking = true;
        isHit = false;
        agent.isStopped = true;
        animator.SetBool(AnimationHash.GetHash(ActionType.Move), false);
        animator.SetBool(AnimationHash.GetHash(ActionType.Attack), true);

        return NodeStates.SUCCESS;
    }

    private NodeStates DoIdle()
    {
        agent.stoppingDistance = 0;
        if (Vector3.Distance(SpawnPoint_Enemy, transform.position) <= 0.1f)
        {
            // 모든 상태 초기화
            isChasing = false;
            isReturning = false;
            isAttacking = false;

            // 애니메이션 초기화
            animator.SetBool(AnimationHash.GetHash(ActionType.Move), false);
            animator.SetBool(AnimationHash.GetHash(ActionType.Attack), false);

            // 회전 처리
            if (CheckPlayerInSight() && currentTarget != null)
            {
                Vector3 directionToPlayer = (currentTarget.transform.position - transform.position).normalized;
                directionToPlayer.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 20);
            }
            else if (transform.rotation != SpawnRotation_Enemy)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, SpawnRotation_Enemy, 20);
            }
        }

        return NodeStates.SUCCESS;
    }

    private NodeStates DoHit()
    {
        if (hitStunCoroutine == null)
        {
            hitStunCoroutine = StartCoroutine(HitStunCoroutine());
        }
        return NodeStates.SUCCESS;
    }

    private IEnumerator HitStunCoroutine()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(0.5f);
        animator.SetBool(AnimationHash.GetHash(ActionType.Hit), false);
        agent.isStopped = false;
        isHit = false;
        hitStunCoroutine = null;
    }
}
