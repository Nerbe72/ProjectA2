using System.Collections.Generic;
using UnityEngine;

public partial class Player : Character
{
    [SerializeField] private List<ParticleSystem> attackEffects;

    // 애니메이션 이벤트
    public void PlayAttackEffect(AttackEffect _effect)
    {
        attackEffects[(int)_effect].Play();
    }
}
