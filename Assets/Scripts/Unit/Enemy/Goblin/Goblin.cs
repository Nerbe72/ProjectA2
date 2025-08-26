using GameStuff;

public partial class Goblin : Enemy
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Respawn()
    {
        base.Respawn();
        isChasing = false;
        isReturning = false;
        isAttacking = false;
        if (agent != null)
        {
            agent.isStopped = false;
        }
        if (animator != null)
        {
            animator.SetBool(AnimationHash.GetHash(ActionType.Attack), false);
            animator.SetBool(AnimationHash.GetHash(ActionType.Move), false);
        }
    }
}
