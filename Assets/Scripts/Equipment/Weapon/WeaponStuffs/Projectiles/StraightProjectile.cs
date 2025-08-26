using UnityEngine;

public class StraightProjectile : Projectile
{
    protected override void Update()
    {
        if (target != null)
        {
            transform.rotation = Quaternion.LookRotation(target.position - transform.position);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        float distance = (startPosition - transform.position).sqrMagnitude;

        if (distance >= (maxRange * maxRange))
        {
            Destroy(gameObject);
            return;
        }

        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, projectileSpeed * Time.deltaTime);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, transform.position + (direction * maxRange), projectileSpeed * Time.deltaTime);
    }
}
