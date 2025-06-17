using System.Collections;
using UnityEngine;

[System.Serializable]
public struct AttackPatternDistance
{
    public float MinDistance;
    public float MaxDistance;
}

public partial class DeepDragon : Boss
{
    [SerializeField] private bool isAIActive = false;
    private Node root;

    [Header("Attack Hitbox")]
    [SerializeField] private MeleeHitbox rightArmHitbox;
    [SerializeField] private MeleeHitbox headHitbox;
    [SerializeField] private MeleeHitbox groundHitbox;

    [Header("Pattern Range")]
    [SerializeField] private AttackPatternDistance pattern1Melee = new AttackPatternDistance { MinDistance = 0f, MaxDistance = 5f };
    public AttackPatternDistance Pattern1Melee => pattern1Melee;

    [SerializeField] private AttackPatternDistance pattern2MidRange = new AttackPatternDistance { MinDistance = 5.1f, MaxDistance = 15f };
    public AttackPatternDistance Pattern2MidRange => pattern2MidRange;

    [SerializeField] private AttackPatternDistance pattern3LongRange = new AttackPatternDistance { MinDistance = 15.1f, MaxDistance = 30f };
    public AttackPatternDistance Pattern3LongRange => pattern3LongRange;

    [SerializeField] private AttackPatternDistance pattern4Charge = new AttackPatternDistance { MinDistance = 10f, MaxDistance = 25f };
    public AttackPatternDistance Pattern4Charge => pattern4Charge;
    
    [Header("Fallback Attack")]
    [SerializeField] private float jumpAttackRetreatDistance = 10f;
    public float JumpAttackRetreatDistance => jumpAttackRetreatDistance;

    [Tooltip("Retreat Speed")]
    [SerializeField] private float retreatSpeed = 15f;
    public float RetreatSpeed => retreatSpeed;
    
    [Header("Chase Time")]
    [SerializeField] private float chasePhaseDuration = 5f;
    public float ChasePhaseDuration => chasePhaseDuration;

    [SerializeField] private float movementChangeInterval = 1.5f;
    public float MovementChangeInterval => movementChangeInterval;
    
    // Chase & Reposition State
    private float chasePhaseStartTime;
    private int currentMovementIndex;
    
    protected override void Awake()
    {
        base.Awake();
        root = CreateBehaviourTree();
    }
    
    private void Update()
    {
        if (isDead) return;
        root.Evaluate();
    }
    
    public void StartAI()
    {
        isAIActive = true;
    }

    // Animation Event

    public void OnArmAttack() => rightArmHitbox.Activate();
    public void OnHeadAttack() => headHitbox.Activate();
    public void OnGroundAttack() => groundHitbox.Activate();
    public void OnArmAttackEnd() => rightArmHitbox.Deactivate();
    public void OnHeadAttackEnd() => headHitbox.Deactivate();
    public void OnGroundAttackEnd() => groundHitbox.Deactivate();

    public void CreateShock()
    {
        StartCoroutine(ShockCoroutine());
    }

    private IEnumerator ShockCoroutine()
    {
        yield break;
    }
}
