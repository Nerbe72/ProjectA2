using UnityEngine;

public class CurveProjectile : Projectile
{
    float time = 0f;
    float duration;
    Vector3 endPoint;
    Vector3 previousPosition;

    public override void SetData(Character _owner, Transform _spawn, Target _target = null, int _abilityID = 0)
    {
        base.SetData(_owner, _spawn, _target, _abilityID);
        // target이 유효하고 사거리 내에 있으면 목표로 설정, 아니면 forward 방향 최대 거리로 설정
        if (target != null && Vector3.Distance(startPosition, target.position) <= maxRange)
            endPoint = target.position;
        else
            endPoint = startPosition + direction * maxRange;

        // 사용자가 요청한 공식: adjustedSpeed = projectileSpeed * (실제 거리 / 최대 사거리)
        // 이 공식은 가까울수록 투사체 속도를 느리게 만들어, 모든 거리에서 비행 시간을 일정하게 맞춥니다.
        float actualDistance = Vector3.Distance(startPosition, endPoint);

        // 데이터가 유효하지 않거나(maxRange, projectileSpeed가 0) 거리가 거의 없을 경우에 대한 예외 처리
        if (maxRange > 0.01f && projectileSpeed > 0.01f && actualDistance > 0.01f)
        {
            float adjustedSpeed = projectileSpeed * (actualDistance / maxRange);
            duration = actualDistance / adjustedSpeed;
        }
        else
        {
            // 예외 상황에서는 기본 속도를 사용합니다.
            duration = projectileSpeed > 0.01f ? actualDistance / projectileSpeed : 1.0f;
        }

        time = 0f;
        previousPosition = startPosition;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    protected override void Update()
    {
        previousPosition = transform.position;
        time += Time.deltaTime;
        float t = time / duration;
        Vector3 flatPosition = Vector3.Lerp(startPosition, endPoint, t);
        float heightOffset = Mathf.Sin(t * Mathf.PI) * curveHeight;
        Vector3 currentPosition = flatPosition + Vector3.up * heightOffset;
        transform.position = currentPosition;
        Vector3 moveDirection = currentPosition - previousPosition;
        if (moveDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(moveDirection);
        
        if (t >= 1f) Destroy(gameObject);
    }
}
