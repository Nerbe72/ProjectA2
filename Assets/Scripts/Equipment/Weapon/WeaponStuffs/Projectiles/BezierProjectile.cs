using UnityEngine;

public class BezierProjectile : Projectile
{
    float time = 0f;
    float duration;
    private Vector3 controlPoint, endPoint;

    protected override void Update()
    {
        float distance = Vector3.Distance(startPosition, endPoint);
        float moveDelta = projectileSpeed * Time.deltaTime;
        time += moveDelta / distance;
        time = Mathf.Clamp01(time);

        Vector3 position = Mathf.Pow(1 - time, 2) * startPosition +
                           2 * (1 - time) * time * controlPoint +
                           Mathf.Pow(time, 2) * endPoint;

        transform.position = position;

        if (time >= 1f)
        {
            Destroy(gameObject);
        }
    }

    public override void SetData(Character _owner, Transform _spawn, Target _target = null, int _abilityID = 0)
    {
        base.SetData(_owner, _spawn, _target, _abilityID);

        endPoint = target != null ? target.position : startPosition + direction * maxRange;
        Vector3 verticalRandom = Vector3.up * Random.Range(-0.5f, curveHeight);
        float randomValue = Random.Range(-1.5f, 1.5f);
        Vector3 horizontalRandom = owner.transform.right * (randomValue < 0f ? randomValue -0.5f : randomValue + 0.5f);
        controlPoint = startPosition + (endPoint - startPosition) * 0.05f + verticalRandom + horizontalRandom;
        time = 0f;
    }
}
