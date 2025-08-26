using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    private enum PostProcessingType
    {
        None = -1,
        Hurt,
        Running,
        Dead,
    }

    [SerializeField] private List<Volume> postprocessings;

    [SerializeField] private float hurtFadeMultiply;
    [SerializeField] [Tooltip("fadeMultiply * fadeInMultiply")] private float hurtFadeInMultiply;

    private Coroutine runningCoroutine;
    private Coroutine runningEndCoroutine;

    bool runningEnter = true;
    bool runningExit = false;

    private void Awake()
    {
        if(Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Init();
    }

    private void Init()
    {
        
    }

    public void SetHurtEffect()
    {
        StopAllCoroutines();
        StartCoroutine(VolumeWeightLerp(postprocessings[(int)PostProcessingType.Hurt]));
    }

    public void SetRunningEffect()
    {
        if (postprocessings[(int)PostProcessingType.Running].weight == 0f)
        {
            if (runningEnter)
            {
                if (runningCoroutine != null)
                {
                    StopCoroutine(runningCoroutine);
                }
                runningCoroutine = StartCoroutine(VolumeWeightHalfLerp(postprocessings[(int)PostProcessingType.Running], true));
                
                runningEnter = false;
                runningExit = true;
            }
        }
    }

    public void ResetRunningEffect()
    {
        if (postprocessings[(int)PostProcessingType.Running].weight == 1f)
        {
            if (runningExit)
            {
                if (runningEndCoroutine != null)
                {
                    StopCoroutine(runningEndCoroutine);
                }
                runningEndCoroutine = StartCoroutine(VolumeWeightHalfLerp(postprocessings[(int)PostProcessingType.Running], false));
                
                runningExit = false;
                runningEnter = true;
            }
        }
    }

    private IEnumerator VolumeWeightLerp(Volume _target)
    {
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime * hurtFadeMultiply * hurtFadeInMultiply;
            
            _target.weight = Mathf.Lerp(0, 1, time);

            if (time >= 1f)
                break;

            yield return null;
        }

        yield return null;

        time = 0f;
        while (true)
        {
            time += Time.deltaTime * hurtFadeMultiply;

            _target.weight = Mathf.Lerp(1, 0, time);

            if (time >= 1f)
                break;

            yield return null;
        }

        yield break;
    }

    private IEnumerator VolumeWeightHalfLerp(Volume _target, bool _ascending)
    {
        float time = 0f;

        float start = 0f;
        float end = 1f;

        if (!_ascending)
        {
            start = 1f;
            end = 0f;
        }

        while (true)
        {
            time += Time.deltaTime * hurtFadeMultiply * hurtFadeInMultiply;

            _target.weight = Mathf.Lerp(start, end, time);

            if (time >= 1f)
                break;

            yield return null;
        }

        yield break;
    }

    public IEnumerator DeadFadeIn(float _duration)
    {
        var dead = postprocessings[(int)PostProcessingType.Dead];
        dead.weight = 0f;
        
        float time = 0f;

        if (_duration <= 0f)
        {
            dead.weight = 1f;
            yield break;
        }

        while (true)
        {
            time += Time.deltaTime;
            dead.weight = Mathf.Clamp01(time / _duration);

            if (time >= _duration)
                break;

            yield return null;
        }

        dead.weight = 1f;
        
        yield break;
    }

    public void ResetDeadEffectImmediate()
    {
        var dead = postprocessings[(int)PostProcessingType.Dead];
        dead.weight = 0f;
    }
}
