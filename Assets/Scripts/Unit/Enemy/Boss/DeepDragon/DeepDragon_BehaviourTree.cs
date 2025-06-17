using System.Collections.Generic;

public partial class DeepDragon : Boss
{
    private Node CreateBehaviourTree()
    {
        return new Selector(
            new List<Node>
            {
                new Sequence(new List<Node>{ new ConditionNode(CheckDead), new ActionNode(DoDead) }),
                new Sequence(new List<Node>{ new ConditionNode(IsAIDisabled), new ActionNode(DoNothing) }),
                new Sequence(new List<Node>{ new ConditionNode(IsAttacking), new ActionNode(StopMovement) }),
                new Sequence(new List<Node>{ new Inverter(new ConditionNode(IsAttacking)), MainCombatLoop() })
            });
    }

    private Node MainCombatLoop()
    {
        return new Sequence(
            new List<Node>
            {
                ChasePhase(),
                AttackPhase()
            });
    }
    
    private Node AttackPhase()
    {
        return new Selector(
            new List<Node>
            {
                new Sequence(new List<Node>{ new ConditionNode(ShouldPerformFallbackAttack), new ActionNode(DoRetreatForFallback), new ActionNode(DoFallbackJumpAttack) }),
                new Sequence(new List<Node>{ new ConditionNode(IsInPattern4Dist), new ActionNode(DoPattern4) }),
                new Sequence(new List<Node>{ new ConditionNode(IsInPattern3Dist), new ActionNode(DoPattern3) }),
                new Sequence(new List<Node>{ new ConditionNode(IsInPattern2Dist), new ActionNode(DoPattern2) }),
                new Sequence(new List<Node>{ new ConditionNode(IsInPattern1Dist), new ActionNode(DoPattern1) })
            });
    }

    private Node ChasePhase()
    {
        return new Sequence(
            new List<Node>
            {
                new ActionNode(InitChasePhase),
                new ActionNode(DoChaseAndReposition)
            });
    }
} 