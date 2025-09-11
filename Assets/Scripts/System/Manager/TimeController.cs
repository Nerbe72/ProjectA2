using System.Collections;
using UnityEngine;
using GameStuff;

public class TimeController : MonoBehaviour
{
    [SerializeField] private Transform sun;
    [SerializeField] private float pollInterval = 1f;

    private const float FullTurn = 360f;

    private float targetAngle;
    private Coroutine pollRoutine;
    private AuthManager auth;
    private Quaternion baseLocalRotation;
    private float currentUnwrapped;
    private float targetUnwrapped;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        auth = FindAnyObjectByType<AuthManager>();

        baseLocalRotation = sun != null ? sun.localRotation : Quaternion.identity;
        currentUnwrapped = 0f;
        targetUnwrapped = 0f;

        if (pollRoutine == null)
            pollRoutine = StartCoroutine(PollTimeCoroutine());
    }

    private void OnDestroy()
    {
        if (pollRoutine != null)
        {
            StopCoroutine(pollRoutine);
            pollRoutine = null;
        }
    }

    private IEnumerator PollTimeCoroutine()
    {
        while (true)
        {
            bool gotAngle = false;
            float nextTarget = targetAngle; // 기본값: 이전 목표 유지

            if (auth != null)
            {
                var task = auth.GetPublicDataAsync<TimeResponse>(Request.time);
                while (!task.IsCompleted)
                    yield return null;

                var resp = task.Result;
                if (resp != null)
                {
                    float angleRaw = resp.angle;
                    GameTime.CurrentTime = angleRaw;
                    nextTarget = angleRaw % 360f;
                    if (nextTarget < 0f) nextTarget += 360f;
                    gotAngle = true;
                }
            }

            if (gotAngle)
            {
                targetAngle = Normalize360(nextTarget);
                float baseTurns = Mathf.Floor(currentUnwrapped / FullTurn);
                float candidate = baseTurns * FullTurn + targetAngle;
                if (candidate < currentUnwrapped)
                    candidate += FullTurn;
                targetUnwrapped = candidate;
            }

            float elapsed = 0f;
            float startUnwrapped = currentUnwrapped;
            while (elapsed < pollInterval)
            {
                if (sun != null)
                {
                    float t = Mathf.Clamp01(elapsed / pollInterval);
                    float newUnwrapped = Mathf.Lerp(startUnwrapped, targetUnwrapped, t);
                    float xWrapped = Mathf.Repeat(newUnwrapped, FullTurn);
                    sun.localRotation = baseLocalRotation * Quaternion.AngleAxis(xWrapped, Vector3.right);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (sun != null)
            {
                currentUnwrapped = targetUnwrapped;
                float xWrappedFinal = Mathf.Repeat(currentUnwrapped, FullTurn);
                sun.localRotation = baseLocalRotation * Quaternion.AngleAxis(xWrappedFinal, Vector3.right);
            }
        }
    }

    private static float Normalize360(float angle)
    {
        angle %= FullTurn;
        if (angle < 0f) angle += FullTurn;
        return angle;
    }

    [System.Serializable]
    private class TimeResponse
    {
        public float angle;
    }
}
