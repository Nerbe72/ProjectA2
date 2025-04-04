using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    private List<Target> targets = new List<Target>();
    private List<Target> onSightTargets = new List<Target>();

    public Target CurrentTarget;

    private void Awake()
    {
        if (SingletonManager.targetManager != null)
        {
            Destroy(gameObject);
            return;
        }

        SingletonManager.targetManager = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDrawGizmos()
    {
        
        for(int i = 0; i < targets.Count; i++)
        {
            if (onSightTargets.Contains(targets[i]))
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;
            Gizmos.DrawSphere(targets[i].transform.position, 0.5f);
        }
    }

    #region 타깃에 의한 호출
    public void AddTarget(Target _target, bool _preInSight)
    {
        if (_target == null) return;
        if (targets.Contains(_target)) return;

        targets.Add(_target);

        if (_preInSight)
            AddTargetVisible(_target);

        //Debug.Log($"{_target.transform.parent.name} <color=yellow>in Target</color>");
    }

    public void RemoveTarget(Target _target)
    {
        if (_target == null) return;
        if (!targets.Contains(_target)) return;

        if (CurrentTarget == _target)
            UnSetTarget();

        targets.Remove(_target);

        RemoveTargetVisible(_target);

        //Debug.Log($"{_target.transform.parent.name} <color=yellow>out Target</color>");
    }

    public void AddTargetVisible(Target _target)
    {
        if (_target == null) return;
        if (!targets.Contains(_target)) return; //타깃 범위에 들어오지 않은 대상인 경우 무시
        if (onSightTargets.Contains(_target)) return;

        onSightTargets.Add(_target);
        //Debug.Log($"{_target.transform.parent.name} <color=aqua>in Visible</color>");
    }

    public void RemoveTargetVisible(Target _target)
    {
        if (_target == null) return;
        if (!onSightTargets.Contains(_target)) return;

        onSightTargets.Remove(_target);
        //Debug.Log($"{_target.transform.parent.name} <color=aqua>out Visible</color>");
    }
    #endregion

    /// <param name="_camera">이 카메라를 중심으로 연산됨</param>
    public void SetTarget(Transform _camera)
    {
        int m_count = onSightTargets.Count;

        if (m_count == 0) return;

        if (m_count == 1)
        {
            CurrentTarget = onSightTargets[0];
            CurrentTarget.SetIndicatorVisibility(true);
            //Debug.Log($"{CurrentTarget.transform.parent.name} is <color=red>Targeted</color>");
            return;
        }

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height);

        Target bestValue = onSightTargets[0];

        //화면 중앙에 가장 가까운 대상을 선택
        for (int i = 0; i < m_count - 1; i++)
        {
            Vector3 target1 = onSightTargets[i].transform.position;
            Vector3 target2 = onSightTargets[i + 1].transform.position;

            if (Vector2.Distance(screenCenter, Camera.main.WorldToScreenPoint(target1) )
                > Vector2.Distance(screenCenter, Camera.main.WorldToScreenPoint(target2) ) )
                bestValue = onSightTargets[i + 1];
        }

        CurrentTarget = bestValue;

        CurrentTarget.SetIndicatorVisibility(true);
        //Debug.Log($"{CurrentTarget.transform.parent.name} is <color=red>Targeted</color>");
    }

    public void ChangeTarget(Target _target)
    {
        if (CurrentTarget == _target) return;

        UnSetTarget();
        CurrentTarget = _target;
        CurrentTarget.SetIndicatorVisibility(true);
        //Debug.Log($"Target Changed to <color=magenta>{CurrentTarget.transform.parent.name}</color>");
    }

    public void UnSetTarget()
    {
        if (CurrentTarget == null) return;

        //Debug.Log($"{CurrentTarget.transform.parent.name} is <color=magenta>Un</color><color=red>Targeted</color>");
        CurrentTarget.SetIndicatorVisibility(false);
        CurrentTarget = null;
    }

    //타깃 전환 (가로로 가까운순 좌 우 스크롤, 좌:스크롤 상 || 우: 스크롤 하)
    public void SearchTarget(bool _searchRight)
    {
        if (CurrentTarget == null) return;

        float dist = 0;
        Target tempTarget = CurrentTarget;

        for (int i = 0; i < onSightTargets.Count; i++)
        {
            if (onSightTargets[i] == CurrentTarget) continue;

            //뷰포트 전환후 x거리를 뺀 후 스크롤 방향에 따라

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
