using GameStuff;
using UnityEngine;

public class CurveProjectile : Projectile
{
    float time = 0f;
    float duration;
    Vector3 endPoint;
    Vector3 previousPosition;

    public override void SetData(Character _owner, Transform _spawn, Target _target = null, AttackType _type = AttackType.Fixed, int _abilityID = 0)
    {
        base.SetData(_owner, _spawn, _target, _type, _abilityID);
        if (target != null && Vector3.Distance(startPosition, target.position) <= maxRange)
            endPoint = target.position;
        else
            endPoint = startPosition + direction * maxRange;

        float actualDistance = Vector3.Distance(startPosition, endPoint);

        if (maxRange > 0.01f && projectileSpeed > 0.01f && actualDistance > 0.01f)
        {
            float adjustedSpeed = projectileSpeed * (actualDistance / maxRange);
            duration = actualDistance / adjustedSpeed;
        }
        else
        {
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
