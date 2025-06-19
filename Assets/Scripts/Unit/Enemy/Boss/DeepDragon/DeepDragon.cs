using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;



public partial class DeepDragon : Boss
{
    [System.Serializable]
    private struct AttackPatternDistance
    {
        public float MinDistance;
        public float MaxDistance;
    }

    private Node root;

    [Header("Attack Hitbox")]
    [SerializeField] private MeleeHitbox rightArmHitbox;
    [SerializeField] private MeleeHitbox headHitbox;
    [SerializeField] private MeleeHitbox groundHitbox;
    [SerializeField] private GroundShockwave groundShock;

    [Header("Hit Collider")]
    [SerializeField] private Collider bodyCollider;
    [SerializeField] private Collider headCollider;

    [Header("Animator")]
    [SerializeField] private PlayableDirector cutscene;

    [Header("Particle")]
    [SerializeField] private ParticleSystem clawParticle;

    [Header("Wall")]
    [SerializeField] private GameObject blockingWall; 

    [Header("Pattern Range")]
    [SerializeField] private AttackPatternDistance pattern1Melee = new AttackPatternDistance { MinDistance = 0f, MaxDistance = 5f };

    [SerializeField] private AttackPatternDistance pattern2MidRange = new AttackPatternDistance { MinDistance = 5.1f, MaxDistance = 15f };

    [SerializeField] private AttackPatternDistance pattern3LongRange = new AttackPatternDistance { MinDistance = 15.1f, MaxDistance = 30f };

    [SerializeField] private AttackPatternDistance pattern4Charge = new AttackPatternDistance { MinDistance = 10f, MaxDistance = 25f };
    
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

    public override void SetFaced()
    {
        base.SetFaced();

        blockingWall.SetActive(true);
        PlayCutscene();
    }

    public void StartAI()
    {
        isAIActive = true;
        cutscene.Stop();
    }

    public void BlockInput()
    {
        UIManager.OffBasicUI();
        InputManager.IgnoreInput = true;
        InputManager.IgnoreUIInput = true;
    }

    public void ReleaseInput()
    {
        UIManager.OnBasicUI();
        InputManager.IgnoreInput = false;
        InputManager.IgnoreUIInput = false;
    }

    public override void Dead()
    {
        base.Dead();

        blockingWall.SetActive(false);
    }

    public void PlayCutscene()
    {
        cutscene.Play();
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
        groundShock.Play();
    }

    public void SetAttacking()
    {
        if (headCollider != null) headCollider.enabled = false;
        isAttacking = true;
    }

    public void ReleaseAttacking()
    {
        isAttacking = false;
        if (headCollider != null) headCollider.enabled = true;
    }

    public void SetBoolAnimationParameter(ActionType _type)
    {
        animator.SetBool(AnimationHash.GetHash(_type), true);
    }

    public void ResetBoolAnimationParameter(ActionType _type)
    {
        animator.SetBool(AnimationHash.GetHash(_type), false);
    }

    public void Shake()
    {
        Singleton.Get<CameraManager>().ShakeCamera(0.3f);
    }

    public void PlayClawSlash()
    {
        clawParticle.Play();
    }
}
