using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    private Dictionary<EffectColor, List<GameObject>> effectPools = new Dictionary<EffectColor, List<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 각 EffectColor에 대한 빈 리스트 초기화
        foreach (EffectColor color in System.Enum.GetValues(typeof(EffectColor)))
        {
            effectPools[color] = new List<GameObject>();
        }
    }

    public void StartEffect(EffectColor _color, Vector3 _position)
    {
        List<GameObject> pool = effectPools[_color];
        
        // 비활성화된 오브젝트 순차탐색
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                // 찾았으면 재사용
                ActivateEffect(pool[i], _position);
                return;
            }
        }

        // 비활성화된게 없으면 새로 생성
        GameObject newEffect = CreateNewEffect(_color, _position);
        if (newEffect != null)
        {
            pool.Add(newEffect);
        }
    }

    private void ActivateEffect(GameObject _effect, Vector3 _position)
    {
        _effect.transform.position = _position;
        _effect.SetActive(true);
        
        ParticleSystem ps = _effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }
    }

    private GameObject CreateNewEffect(EffectColor _color, Vector3 _position)
    {
        GameObject prefab = ResourceLoader.Load<GameObject>(_color.ToString(), LoadType.HitEffect);
        if (prefab == null) return null;

        GameObject newEffect = Instantiate(prefab, _position, Quaternion.identity);
        
        // ParticleSystem의 StopAction을 Disable로 설정 (파티클 종료시 자동 비활성화)
        ParticleSystem ps = newEffect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.stopAction = ParticleSystemStopAction.Disable;
            ps.Play();
        }

        return newEffect;
    }
}

