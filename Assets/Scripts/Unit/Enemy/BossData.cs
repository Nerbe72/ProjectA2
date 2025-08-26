using UnityEngine;
using System.Collections.Generic;

using GameStuff;
using SoundStuff;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/BossStatData", order = 1)]
public class BossData : StatData
{
    public AttackType AttackType;
    public int Speed;
    public int RewardCurrency;
    
    [Header("Sound Settings")]
    public AudioClip BossBGM;
    public List<AudioClip> AttackSounds;
    
    public AudioClip GetAttackSound(BossAttackPattern _pattern)
    {
        return AttackSounds[(int)_pattern];
    }
}
