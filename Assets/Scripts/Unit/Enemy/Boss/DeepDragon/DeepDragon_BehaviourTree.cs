using System.Collections.Generic;
using UnityEngine;

public partial class DeepDragon : Boss
{
    private bool chaseInitialized = false;
    private bool movementInitialized = false;
    private float movementPhaseStartTime;
    private float movementDuration = 2f;
    private MovementType chosenMovementType;
    [Header("Movement Settings")]
    [SerializeField] private float movementStopDistance = 5f;
    public float MovementStopDistance => movementStopDistance;

    private enum MovementType { Idle, Forward, Backward, Left, Right }

    private Node CreateBehaviourTree()
    {
        return new Selector(
            new List<Node>
            {
                new Sequence(new List<Node>{ new ConditionNode(CheckDead), new ActionNode(DoDead) }),
                new Sequence(new List<Node>{ new ConditionNode(IsAiDisabled), new ActionNode(DoNothing) }),
                new Sequence(new List<Node>{ new ConditionNode(IsAttacking), new ActionNode(StopMovement) }),
                new Sequence(new List<Node>{ new Inverter(new ConditionNode(IsAttacking)), PhaseAction() })
            });
    }

    private Node PhaseAction()
    {
        return new ActionNode(() => PhaseUpdate());
    }

    private NodeStates PhaseUpdate()
    {
        if (movementInitialized)
        {
            var moveState = MovementPhase().Evaluate();
            if (moveState == NodeStates.RUNNING)
                return NodeStates.RUNNING;
            movementInitialized = false;
            AttackPhase().Evaluate();
            return NodeStates.RUNNING;
        }
        if (isAttacking)
        {
            return NodeStates.RUNNING;
        }
        movementInitialized = true;
        movementPhaseStartTime = Time.time;
        agent.stoppingDistance = movementStopDistance;
        // 가중 확률로 이동 방향 선택: 50% 전진, 12.5% 대기/후진/좌/우
        float r = Random.value;
        if (r < 0.5f)
            chosenMovementType = MovementType.Forward;
        else if (r < 0.625f)
            chosenMovementType = MovementType.Idle;
        else if (r < 0.75f)
            chosenMovementType = MovementType.Backward;
        else if (r < 0.875f)
            chosenMovementType = MovementType.Left;
        else
            chosenMovementType = MovementType.Right;
        return NodeStates.RUNNING;
    }

    private Node AttackPhase()
    {
        return new Sequence(
            new List<Node>
            {
                new ActionNode(() => { Debug.Log("AttackPhase Start"); return NodeStates.SUCCESS; }),
                new ConditionNode(() =>
                {
                    float dist = Vector3.Distance(transform.position, player.transform.position);
                    bool inRange = (dist >= pattern1Melee.MinDistance && dist <= pattern1Melee.MaxDistance)
                                || (dist >= pattern2MidRange.MinDistance && dist <= pattern2MidRange.MaxDistance)
                                || (dist >= pattern3LongRange.MinDistance && dist <= pattern3LongRange.MaxDistance)
                                || (dist >= pattern4Charge.MinDistance && dist <= pattern4Charge.MaxDistance);
                    return inRange && Random.value < 0.8f;
                }),
                new Selector(new List<Node>
                {
                    new Sequence(new List<Node>{ new ConditionNode(IsInPattern1Distance), new ActionNode(DoPattern1) }),
                    new Sequence(new List<Node>{ new ConditionNode(IsInPattern2Distance), new ActionNode(DoPattern2) }),
                    new Sequence(new List<Node>{ new ConditionNode(IsInPattern3Distance), new ActionNode(DoPattern3) }),
                    new Sequence(new List<Node>{ new ConditionNode(IsInPattern4Distance), new ActionNode(DoPattern4) })
                })
            });
    }

    private Node ChasePhase()
    {
        return new Sequence(
            new List<Node>
            {
                new Selector(
                    new List<Node>
                    {
                        new Sequence(
                            new List<Node>
                            {
                                new ConditionNode(() => !chaseInitialized),
                                new ActionNode(InitChasePhase),
                                new ActionNode(() => { chaseInitialized = true; return NodeStates.SUCCESS; })
                            }),
                        new ActionNode(() => NodeStates.SUCCESS)
                    }),
                new ActionNode(DoChaseAndReposition)
            });
    }

    private Node MovementPhase()
    {
        return new ActionNode(() =>
        {
            // 플레이어와의 수평 거리가 movementStopDistance 이하이면 이동 종료
            Vector2 bossXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 playerXZ = new Vector2(player.transform.position.x, player.transform.position.z);
            if (Vector2.Distance(bossXZ, playerXZ) <= movementStopDistance)
            {
                if (agent.enabled) agent.SetDestination(transform.position);
                animator.SetBool(AnimationHash.GetHash(ActionType.Walk), false);
                animator.SetFloat(AnimationHash.GetHash(ActionType.Move), 0f);
                animator.SetFloat(AnimationHash.GetHash(ActionType.Side), 0f);
                return NodeStates.SUCCESS;
            }
            Debug.Log($"MovementPhase Start: initialized={movementInitialized}, chosen={chosenMovementType}");
            if (!movementInitialized)
            {
                movementInitialized = true;
                movementPhaseStartTime = Time.time;
                agent.stoppingDistance = movementStopDistance;
                float dist = Vector3.Distance(transform.position, player.transform.position);
                bool inRange = (dist >= pattern1Melee.MinDistance && dist <= pattern1Melee.MaxDistance)
                            || (dist >= pattern2MidRange.MinDistance && dist <= pattern2MidRange.MaxDistance)
                            || (dist >= pattern3LongRange.MinDistance && dist <= pattern3LongRange.MaxDistance)
                            || (dist >= pattern4Charge.MinDistance && dist <= pattern4Charge.MaxDistance);
                //chosenMovementType = (MovementType)Random.Range(0, 5); // Idle 포함 0~4
            }
            if (Time.time < movementPhaseStartTime + movementDuration)
            {
                // Y축 회전만 유지하며 플레이어 바라보기
                transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
                // 이동 방향 설정
                Vector3 worldDir;
                switch (chosenMovementType)
                {
                    case MovementType.Idle:
                        worldDir = Vector3.zero;
                        break;
                    case MovementType.Forward:
                        worldDir = transform.forward;
                        break;
                    case MovementType.Backward:
                        worldDir = -transform.forward;
                        break;
                    case MovementType.Left:
                        worldDir = -transform.right;
                        break;
                    case MovementType.Right:
                        worldDir = transform.right;
                        break;
                    default:
                        worldDir = Vector3.zero;
                        break;
                }
                // 애니메이션 파라미터 설정
                float speedFactor = (chosenMovementType == MovementType.Backward) ? 0.5f : 1f;
                Vector3 localDir = transform.InverseTransformDirection(worldDir);
                animator.SetBool(AnimationHash.GetHash(ActionType.Walk), true);
                animator.SetFloat(AnimationHash.GetHash(ActionType.Move), localDir.z * speedFactor);
                animator.SetFloat(AnimationHash.GetHash(ActionType.Side), localDir.x);
                // 네비게이션 이동
                if (worldDir != Vector3.zero)
                {
                    agent.isStopped = false;
                    if (chosenMovementType == MovementType.Forward)
                        agent.SetDestination(player.transform.position);
                    else
                        agent.SetDestination(transform.position + worldDir * JumpAttackRetreatDistance);
                }
                else if (agent.enabled)
                {
                    agent.SetDestination(transform.position);
                }
                return NodeStates.RUNNING;
            }
            return NodeStates.SUCCESS;
        });
    }
}