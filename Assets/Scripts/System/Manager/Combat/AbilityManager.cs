using System.Collections.Generic;
using System;
using UnityEngine;

using GameStuff;

public class AbilityManager : MonoBehaviour
{
    private Dictionary<int, AbilityLogic> skills;
    private Dictionary<int, float> nextReadyByAbilityId;
    private Dictionary<int, Coroutine> cooldownWatchers;

    public event Action<int, float> OnAbilityUsed;
    public event Action<int> OnAbilityReady;

    public int InitializationPriority => 4;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        skills = new Dictionary<int, AbilityLogic>();
        nextReadyByAbilityId = new Dictionary<int, float>();
        cooldownWatchers = new Dictionary<int, Coroutine>();
    }

    public string GetDescription(int _skillID)
    {
        var table = Singleton.Get<TableDataManager>().Table;
        var tableLocale = table.Locale;
        var skill_selected = table.Skill.Get(_skillID);

        string typeText = "";

        switch ((PowerType)skill_selected.PowerType)
        {
            case PowerType.Fixed:
            default:
                typeText = tableLocale.Get(10000046, GameManager.CurrentLocale);
                break;
            case PowerType.Percentage:
                typeText = tableLocale.Get(10000047, GameManager.CurrentLocale);
                break;
        }

        List<object> skillParameters = new List<object> { skill_selected.Cooldown, skill_selected.ContinuouseTime, skill_selected.Power, typeText, skill_selected.Projectile_Amount };
        return string.Format(tableLocale.Get(skill_selected.Description, GameManager.CurrentLocale), skillParameters.ToArray());
    }

    public string GetName(int _skillID)
    {
        var table = Singleton.Get<TableDataManager>().Table;
        var tableLocale = table.Locale;
        var skill_selected = table.Skill.Get(_skillID);

        return tableLocale.Get(skill_selected.Name, GameManager.CurrentLocale);
    }

    public int GetSlotCount(int _skillID)
    {
        var table = Singleton.Get<TableDataManager>().Table;
        var skill_selected = table.Skill.Get(_skillID);

        return skill_selected.RequiredSlotCount;
    }

    public Sprite GetIcon(int _skillID)
    {
        var table = Singleton.Get<TableDataManager>().Table;
        var item_selected = table.Item.Get(_skillID);

        return ResourceLoader.Load<Sprite>(item_selected.Icon, LoadType.ItemIcon);
    }

    public bool TryUseAbility(Character owner, int abilityId, IHurtable target)
    {
        float now = Time.unscaledTime;
        if (nextReadyByAbilityId != null && nextReadyByAbilityId.TryGetValue(abilityId, out var next) && now < next)
            return false;

        var ability = GetAbility(abilityId);
        if (ability == null)
            return false;

        ability.ApplyAbility(owner, target);

        float duration = 0f;
        var table = Singleton.Get<TableDataManager>()?.Table;
        if (table != null)
        {
            var info = table.Skill.Get(abilityId);
            if (info != null) duration = Mathf.Max(0f, info.Cooldown);
        }
        if (nextReadyByAbilityId == null) nextReadyByAbilityId = new Dictionary<int, float>();
        nextReadyByAbilityId[abilityId] = now + duration;

        // 이벤트 통지 및 쿨다운 감시 코루틴 시작
        OnAbilityUsed?.Invoke(abilityId, duration);
        if (duration > 0f)
        {
            if (cooldownWatchers == null) cooldownWatchers = new Dictionary<int, Coroutine>();
            if (cooldownWatchers.TryGetValue(abilityId, out var running) && running != null)
            {
                StopCoroutine(running);
            }
            var co = StartCoroutine(CoWatchCooldown(abilityId));
            cooldownWatchers[abilityId] = co;
        }
        else
        {
            OnAbilityReady?.Invoke(abilityId);
        }
        return true;
    }

    public float GetCooldownRemaining(int abilityId)
    {
        float now = Time.unscaledTime;
        if (nextReadyByAbilityId != null && nextReadyByAbilityId.TryGetValue(abilityId, out var next))
            return Mathf.Max(0f, next - now);
        return 0f;
    }

    public float GetCooldownNormalized(int abilityId)
    {
        var table = Singleton.Get<TableDataManager>()?.Table;
        float duration = 0f;
        if (table != null)
        {
            var info = table.Skill.Get(abilityId);
            if (info != null) duration = Mathf.Max(0f, info.Cooldown);
        }
        if (duration <= 0f) return 1f;

        float remaining = GetCooldownRemaining(abilityId);
        float elapsed = Mathf.Clamp(duration - remaining, 0f, duration);
        return elapsed / duration;
    }

    private System.Collections.IEnumerator CoWatchCooldown(int _abilityId)
    {
        // 0.1초 간격으로 남은 쿨타임을 확인하여 준비 이벤트 발행
        while (GetCooldownRemaining(_abilityId) > 0f)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }
        OnAbilityReady?.Invoke(_abilityId);
        if (cooldownWatchers != null)
            cooldownWatchers.Remove(_abilityId);
    }

    public AbilityLogic GetAbility(int _id)
    {
        if (skills.ContainsKey(_id)) return skills[_id];

        var table = Singleton.Get<TableDataManager>().Table;

        if (table == null)
        {
            Debug.LogError("AbilityManager: TableDataManager is not initialized.");
            return null;
        }

        var skill_selected = table.Skill.Get(_id);

        //ü
        if (skill_selected.ProjectileID != 0)
        {
            
        }

        switch ((SkillType)skill_selected.SkillType)
        {
            case SkillType.Projectile:
                {
                    var projectile = Singleton.Get<TableDataManager>().Table.Projectile.Get(skill_selected.ProjectileID);

                    ProjectileAbilityLogic skillData = new ProjectileAbilityLogic();
                    skillData.AbilityId = skill_selected.ID;
                    skillData.Amount = skill_selected.Projectile_Amount;

                    skills.Add(_id, skillData);
                    return skillData;
                }
            case SkillType.Continuous:
                {
                    ContinuouseAbilityLogic skillData = new ContinuouseAbilityLogic();
                    skillData.AbilityId = skill_selected.ID;
                    skillData.AttackType = (AttackType)skill_selected.AttackType;

                    skillData.Damage = skill_selected.Power;
                    skillData.Duration = skill_selected.ContinuouseTime;

                    skills.Add(_id, skillData);
                    return skillData;
                }
            case SkillType.Knockback:
                {
                    KnockbackAbilityLogic skillData = new KnockbackAbilityLogic();
                    skillData.AbilityId = skill_selected.ID;
                    skillData.KnockbackForce = skill_selected.Power;

                    skills.Add(_id, skillData);
                    return skillData;
                }
            case SkillType.ExtraHit:
                {
                    // ߰Ÿ ..
                    return null;
                }
        }

        return null;
    }
}
