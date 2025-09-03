using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestPathData
{
    public Vector3 startPoint;
    public Vector3 controlPoint;
    public Color anchorColor;
    public QuestPathData(Vector3 startPoint, Vector3 controlPoint, Color anchorColor)
    {
        this.startPoint = startPoint;
        this.controlPoint = controlPoint;
        this.anchorColor = anchorColor;
    }
}

public class QuestPath : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 1f;
    [SerializeField] private List<QuestPathData> paths;

    private ParticleSystem particle;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        particle.Play();
    }

    private void Start()
    {
        StartCoroutine(FollowPathLoop());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void OnDrawGizmosSelected()
    {
        if (paths == null || paths.Count == 0)
            return;
        for (int i = 0; i < paths.Count; i++)
        {
            Gizmos.color = paths[i].anchorColor;

            Gizmos.DrawCube(paths[i].startPoint, Vector3.one * 0.15f);

            if (paths.Count <= i + 1)
                break;

            Gizmos.DrawSphere(paths[i].controlPoint, 0.1f);

            Gizmos.DrawLine(paths[i].startPoint, paths[i + 1].startPoint);
        }
    }

    private IEnumerator FollowPathLoop()
    {
        while (true)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (paths.Count <= i + 1)
                    break;

                Vector3 startPosition = paths[i].startPoint;
                Vector3 controlPoint = paths[i].controlPoint;
                Vector3 endPosition = paths[i + 1].startPoint;

                float time = 0f;
                while (true)
                {

                    float distance = Vector3.Distance(startPosition, endPosition);
                    float moveDelta = projectileSpeed * Time.deltaTime;
                    time += moveDelta / distance;
                    time = Mathf.Clamp01(time);

                    Vector3 position = Mathf.Pow(1 - time, 2) * startPosition +
                                       2 * (1 - time) * time * controlPoint +
                                       Mathf.Pow(time, 2) * endPosition;

                    transform.position = position;
                    //transform.position = Vector3.Lerp(transform.position, position, time);

                    if ((position - endPosition).magnitude <= 0.005f)
                        break;

                    yield return null;
                }
            }

            yield return null;
        }
    }
}
