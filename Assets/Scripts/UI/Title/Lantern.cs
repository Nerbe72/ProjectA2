using System.Collections;
using UnityEngine;

public class Lantern : MonoBehaviour
{
    private new Light light;

    [SerializeField][Range(0, 90)] private float forwardShakeForce;
    [SerializeField][Range(0, 90)] private float sideShakeForce;
    [SerializeField] private float shakeSpeed;

    private void Awake()
    {
        light = GetComponentInChildren<Light>(true);
    }

    private void Start()
    {
        SetShake();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void SetShake()
    {
        StartCoroutine(ShakeLerp());
    }

    private void SetLightForce()
    {

    }

    private IEnumerator ShakeLerp()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime * shakeSpeed;

            transform.rotation = Quaternion.Euler(Mathf.Sin(time) * forwardShakeForce, 0, Mathf.Sin(time) * sideShakeForce);

            yield return null;
        }

        yield break;
    }
}
