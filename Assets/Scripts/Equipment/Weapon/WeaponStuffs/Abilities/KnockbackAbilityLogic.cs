using UnityEngine;

public class KnockbackAbilityLogic : AbilityLogic
{
    [Header("넉백 파라미터")]
    public float KnockbackForce;
    public float Growth;

    public override void ApplyAbility(Character _owner, IHurtable _target)
    {
        // 넉백 강도에 성장 계수 적용
        float finalKnockbackForce = KnockbackForce * (1f + Growth);
        
        // Enemy 타입인 경우 공격자 정보 전달
        if (_target is Enemy enemy)
        {
            // 공격자 방향 정보를 Enemy에 전달할 수 있도록 개선
            _target.Knockback(finalKnockbackForce);
        }
        else
        {
            _target.Knockback(finalKnockbackForce);
        }
    }
}
