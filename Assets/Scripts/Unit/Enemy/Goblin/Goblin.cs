public partial class Goblin : Enemy
{
    private Node rootNode;

    protected override void Awake()
    {
        base.Awake();
        rootNode = CreateBehaviourTree();
    }

    private void FixedUpdate()
    {
        rootNode.Evaluate();
    }

    public override (AttackType type, int damage) CalculateAttack()
    {
        if (weaponInstance != null)
        {
            var weapon_selected = Singleton.Get<TableDataManager>().Table.Weapon.Get(weaponInstance.ItemID);
            int damage = weaponInstance.Damage;
            return ((AttackType)weapon_selected.AttackType, damage);
        }
        if (stats is EnemyData enemyData)
        {
            return (enemyData.AttackType, stats.Damage);
        }
        return (AttackType.Physical, stats.Damage);
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
