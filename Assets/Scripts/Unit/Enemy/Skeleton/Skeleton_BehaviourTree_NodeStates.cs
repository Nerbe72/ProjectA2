using UnityEngine;
using System.Collections;

public partial class Skeleton : Enemy
{
    private Coroutine hitStunCoroutine = null;
    private float lastAttackTime = -Mathf.Infinity;

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
        return (Vector3.Distance(transform.position, Singleton.Player.transform.position) <= EnemyData.AttackDistance);
    }

    private bool CheckSpawnDistance()
    {
        return (EnemyData.DistanceLimit * EnemyData.DistanceLimit) <= (SpawnPoint_Enemy - transform.position).sqrMagnitude;
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
        return isChasing || CheckPlayerInSight() || isHit;
    }

    private bool CheckAttackCooldown()
    {
        return Time.time >= lastAttackTime + EnemyData.AttackCooldown;
    }

    private NodeStates DoDead()
    {
        Debug.Log($"[Skeleton] DoDead: {gameObject.name}");
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
        Debug.Log($"[Skeleton] DoReturnSpawnPoint: {gameObject.name}");
        agent.isStopped = false;
        agent.SetDestination(SpawnPoint_Enemy);
        animator.SetBool(AnimationHash.GetHash(ActionType.Move), true);
        isReturning = true;
        return NodeStates.SUCCESS;
    }

    private NodeStates DoChase()
    {
        Debug.Log($"[Skeleton] DoChase: {gameObject.name}");
        agent.isStopped = false;
        agent.speed = EnemyData.Speed;
        Vector3 dir = (transform.position - player.transform.position).normalized;
        Vector3 targetPosition = player.transform.position + dir * (EnemyData.AttackDistance - 1f);
        agent.SetDestination(targetPosition);
        animator.SetBool(AnimationHash.GetHash(ActionType.Move), true);
        isChasing = true;
        return NodeStates.SUCCESS;
    }

    private NodeStates DoAttack()
    {
        Debug.Log($"[Skeleton] DoAttack: {gameObject.name}");
        isAttacking = true;
        isHit = false;
        agent.isStopped = true;
        animator.SetBool(AnimationHash.GetHash(ActionType.Move), false);
        animator.SetBool(AnimationHash.GetHash(ActionType.Attack), true);
        // 플레이어 방향으로 회전
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        transform.rotation = Quaternion.LookRotation(directionToPlayer);
        // 원거리 곡사 투사체 발사
        if (projectileHandle != null)
        {
            var handler = projectileHandle.GetComponent<ProjectileHandler>();
            if (handler != null && weaponInstance != null)
            {
                handler.FireProjectile(this, weaponInstance.ItemID);
            }
        }
        lastAttackTime = Time.time;
        return NodeStates.SUCCESS;
    }

    private NodeStates DoIdle()
    {
        Debug.Log($"[Skeleton] DoIdle: {gameObject.name}");
        agent.stoppingDistance = 0;
        if (Vector3.Distance(SpawnPoint_Enemy, transform.position) <= 0.1f)
        {
            isChasing = false;
            isReturning = false;
            isAttacking = false;
            agent.isStopped = false;
            animator.SetBool(AnimationHash.GetHash(ActionType.Move), false);
            animator.SetBool(AnimationHash.GetHash(ActionType.Attack), false);
            if (CheckPlayerInSight())
            {
                Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
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
        Debug.Log($"[Skeleton] DoHit: {gameObject.name}");
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