using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public int InitializationPriority => 1;
    private List<Target> targets = new List<Target>();
    private List<Target> onSightTargets = new List<Target>();
    public Target CurrentTarget;
    public event System.Action<Target> OnTargetChanged;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void OnDrawGizmos()
    {

        for (int i = 0; i < targets.Count; i++)
        {
            if (onSightTargets.Contains(targets[i]))
                Gizmos.color = UnityEngine.Color.green;
            else
                Gizmos.color = UnityEngine.Color.red;
            Gizmos.DrawSphere(targets[i].transform.position + Vector3.up * 0.6f, 0.1f);
        }
    }

    public void AddTarget(Target _target, bool _preInSight)
    {
        if (_target == null) return;
        if (targets.Contains(_target)) return;

        targets.Add(_target);

        if (_preInSight)
            AddTargetVisible(_target);

        Debug.Log($"{_target.transform.parent.name} <color=yellow>in Target</color>");
    }

    public void RemoveTarget(Target _target)
    {
        if (_target == null) return;
        if (!targets.Contains(_target)) return;

        if (CurrentTarget == _target)
            UnSetTarget();

        targets.Remove(_target);

        RemoveTargetVisible(_target);

        Debug.Log($"{_target.transform.parent.name} <color=yellow>out Target</color>");
    }

    public void AddTargetVisible(Target _target)
    {
        if (_target == null) return;
        if (!targets.Contains(_target)) return; //Ÿ�� ������ ������ ���� ����� ��� ����
        if (onSightTargets.Contains(_target)) return;

        onSightTargets.Add(_target);
        Debug.Log($"{_target.transform.parent.name} <color=aqua>in Visible</color>");
    }

    public void RemoveTargetVisible(Target _target)
    {
        if (_target == null) return;
        if (!onSightTargets.Contains(_target)) return;

        onSightTargets.Remove(_target);
        Debug.Log($"{_target.transform.parent.name} <color=aqua>out Visible</color>");
    }

    public void SetTarget(Transform _camera)
    {
        int m_count = onSightTargets.Count;

        if (m_count == 0) return;

        if (m_count == 1)
        {
            CurrentTarget = onSightTargets[0];
            CurrentTarget.SetIndicatorVisibility(true);
            OnTargetChanged?.Invoke(CurrentTarget);
            Debug.Log($"{CurrentTarget.transform.parent.name} is <color=red>Targeted</color>");
            return;
        }

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height);

        Target bestValue = onSightTargets[0];

        for (int i = 0; i < m_count - 1; i++)
        {
            Vector3 target1 = onSightTargets[i].transform.position;
            Vector3 target2 = onSightTargets[i + 1].transform.position;

            if (Vector2.Distance(screenCenter, Camera.main.WorldToScreenPoint(target1))
                > Vector2.Distance(screenCenter, Camera.main.WorldToScreenPoint(target2)))
                bestValue = onSightTargets[i + 1];
        }

        CurrentTarget = bestValue;

        CurrentTarget.SetIndicatorVisibility(true);
        OnTargetChanged?.Invoke(CurrentTarget);
        Debug.Log($"{CurrentTarget.transform.parent.name} is <color=red>Targeted</color>");
    }

    public void ChangeTarget(Target _target)
    {
        if (CurrentTarget == _target) return;

        UnSetTarget();
        CurrentTarget = _target;
        CurrentTarget.SetIndicatorVisibility(true);
        OnTargetChanged?.Invoke(CurrentTarget);
        Debug.Log($"Target Changed to <color=magenta>{CurrentTarget.transform.parent.name}</color>");
    }

    public void UnSetTarget()
    {
        if (CurrentTarget == null) return;

        Debug.Log($"{CurrentTarget.transform.parent.name} is <color=magenta>Un</color><color=red>Targeted</color>");
        CurrentTarget.SetIndicatorVisibility(false);
        OnTargetChanged?.Invoke(null);
        CurrentTarget = null;
    }

    //Ÿ�� ��ȯ (���η� ������ �� �� ��ũ��, ��:��ũ�� �� || ��: ��ũ�� ��)
    public void SearchTarget(bool _searchRight)
    {
        if (CurrentTarget == null) return;

        float dist = 0;
        Target tempTarget = CurrentTarget;

        for (int i = 0; i < onSightTargets.Count; i++)
        {
            if (onSightTargets[i] == CurrentTarget) continue;

            //����Ʈ ��ȯ�� x�Ÿ��� �� �� ��ũ�� ���⿡ ����

            float tempDistance =
                Camera.main.WorldToScreenPoint(CurrentTarget.transform.position).x -
                Camera.main.WorldToScreenPoint(onSightTargets[i].transform.position).x;

            if (_searchRight)
            {
                if (tempDistance > dist)
                {
                    tempTarget = onSightTargets[i];
                    dist = tempDistance;
                }
            }
            else
            {
                if (tempDistance < dist)
                {
                    tempTarget = onSightTargets[i];
                    dist = tempDistance;
                }
            }

        }

        if (tempTarget != CurrentTarget)
        {
            ChangeTarget(tempTarget);
        }
    }
}
