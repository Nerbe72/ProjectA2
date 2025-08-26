using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillFrame : HoverableFrame
{
    [Header("Skill")]
    [SerializeField] private Image cooldownImage;
    [SerializeField] private TMP_Text cooldownText;

    private Coroutine updateRoutine;
    private AbilityManager manager;

    public override void SetItemID(int _itemID)
    {
        base.SetItemID(_itemID);
        RefreshImmediate();
    }

    private void OnEnable()
    {
        manager = Singleton.Get<AbilityManager>();
        if (manager != null)
        {
            manager.OnAbilityUsed += HandleAbilityUsed;
            manager.OnAbilityReady += HandleAbilityReady;
        }
        RefreshImmediate();
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.OnAbilityUsed -= HandleAbilityUsed;
            manager.OnAbilityReady -= HandleAbilityReady;
        }
        StopUpdateRoutine();
    }

    private void HandleAbilityUsed(int _abilityId, float _duration)
    {
        if (_abilityId != itemID) return;

        StopUpdateRoutine();

        if (_duration <= 0f)
        {
            SetReadyVisual();
            return;
        }

        updateRoutine = StartCoroutine(CoUpdateCooldown());
    }

    private void HandleAbilityReady(int _abilityId)
    {
        if (_abilityId != itemID) return;
        SetReadyVisual();
        StopUpdateRoutine();
    }

    private IEnumerator CoUpdateCooldown()
    {
        if (manager == null) yield break;

        while (manager.GetCooldownRemaining(itemID) > 0f)
        {
            float remaining = manager.GetCooldownRemaining(itemID);
            float normalizedElapsed = manager.GetCooldownNormalized(itemID);
            float normalizedRemaining = 1f - normalizedElapsed;
            SetCooldownVisual(normalizedRemaining, remaining);
            yield return new WaitForSecondsRealtime(0.1f);
        }

        SetReadyVisual();
        updateRoutine = null;
    }

    private void RefreshImmediate()
    {
        if (manager == null)
        {
            SetReadyVisual();
            return;
        }

        float remaining = manager.GetCooldownRemaining(itemID);
        if (remaining <= 0f)
        {
            SetReadyVisual();
        }
        else
        {
            float normalizedElapsed = manager.GetCooldownNormalized(itemID);
            float normalizedRemaining = 1f - normalizedElapsed;
            SetCooldownVisual(normalizedRemaining, remaining);
            if (updateRoutine == null) updateRoutine = StartCoroutine(CoUpdateCooldown());
        }
    }

    private void SetReadyVisual()
    {
        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;
        SetText("");
    }

    private void SetCooldownVisual(float _normalizedRemaining, float _secondsRemaining)
    {
        if (cooldownImage != null)
            cooldownImage.fillAmount = Mathf.Clamp01(_normalizedRemaining);
        SetText(_secondsRemaining.ToString("0.0"));
    }

    private void SetText(string _text)
    {
        if (cooldownText != null) cooldownText.text = _text;
    }

    private void StopUpdateRoutine()
    {
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }
    }
}
