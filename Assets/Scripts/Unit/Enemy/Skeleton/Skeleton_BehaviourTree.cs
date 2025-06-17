using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public partial class Skeleton : Enemy
{
    private bool isChasing = false;
    private bool isReturning = false;

    private Node CreateBehaviourTree()
    {
        return new Selector(
            new List<Node>
            {
                DieSequence(),
                ReturnSpawnPointSequence(),
                ChaseAndAttackSequence(),
                HitSequence(),
                ReturnToIdleSequence(),
                IdleSequence()
            });
    }

    private Node DieSequence()
    {
        return new Sequence(
            new List<Node>
            {
                new ConditionNode(CheckDead),
                new ActionNode(DoDead)
            });
    }

    private Node ReturnToIdleSequence()
    {
        return new Sequence(
            new List<Node>
            {
                new ConditionNode(CheckAtSpawnPoint),
                new ActionNode(DoIdle)
            });
    }

    private Node ReturnSpawnPointSequence()
    {
        return new Sequence(
            new List<Node>
            {
                new ConditionNode(CheckSpawnDistance),
                new Inverter(new ConditionNode(CheckHit)),
                new ActionNode(DoReturnSpawnPoint)
            });
    }

    private Node ChaseAndAttackSequence()
    {
        return new Sequence(
            new List<Node>
            {
                new Inverter(new ConditionNode(CheckReturning)),
                new ConditionNode(CheckChaseOrHit),
                ChaseOrAttackSelector(),
            });
    }

    private Node ChaseOrAttackSelector()
    {
        return new Selector(
            new List<Node>
            {
                AttackSequence(),
                ChaseSequence()
            });
    }

    private Node ChaseSequence()
    {
        return new Sequence(
            new List<Node>
            {
                new Inverter(new Sequence(
                    new List<Node>
                    {
                        new ConditionNode(CheckPlayerInSight),
                        new ConditionNode(CheckIsPlayerInAttackDistance)
                    })),
                new ActionNode(DoChase)
            });
    }

    private Node AttackSequence()
    {
        return new Sequence(
            new List<Node>
            {
                new ConditionNode(CheckPlayerInSight),
                new ConditionNode(CheckIsPlayerInAttackDistance),
                new ConditionNode(CheckAttackCooldown),
                new ActionNode(DoAttack)
            });
    }

    private Node IdleSequence()
    {
        return new Sequence(
            new List<Node>
            {
                //new ConditionNode(CheckAtSpawnPoint),
                new Inverter(new ConditionNode(CheckChasing)),
                new ActionNode(DoIdle)
            });
    }

    private Node HitSequence()
    {
        return new Sequence(
            new List<Node>
            {
                new ConditionNode(CheckHit),
                new ActionNode(DoHit)
            }
        );
    }
} 