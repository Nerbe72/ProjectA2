using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GameStuff;

public class GroundShockwave : MonoBehaviour
{
    [System.Serializable]
    private class TriggerMinMax
    {
        public float min;
        public float max;
    }

    private AttackType attackType;
    private int damage;
    private Character owner;

    private List<ParticleSystem> particleList = new List<ParticleSystem>();

    private Transform player;
    [Header("Start Range")][SerializeField] private TriggerMinMax startMinMax = new TriggerMinMax { min = 5f, max = 5.5f };
    [Header("End Range")][SerializeField] private TriggerMinMax endMinMax = new TriggerMinMax { min = 10.6f, max = 11.4f };
    [Header("Wave Time (seconds)")][SerializeField] private float startTime = 0.3f;
    [SerializeField] private float endTime = 0.61f;

    private bool isHit = false;

    private void Awake()
    {
        particleList = GetComponentsInChildren<ParticleSystem>(true).ToList();
    }

    private void Start()
    {
        player = Singleton.Player.transform;
        if (player == null)
        {
            Debug.LogError("Player transform not found. Ensure the Player is initialized in Singleton.");
        }
    }

    public void Configure(AttackType _type, int _damage, Character _owner)
    {
        attackType = _type;
        damage = _damage;
        owner = _owner;
    }

    public void Play()
    {
        for (int i = 0; i < particleList.Count; i++)
            particleList[i].Play();
        isHit = false;
        StartCoroutine(ShockTriggerLerp());
    }

    private IEnumerator ShockTriggerLerp()
    {
        float time = 0f;
        while (time < endTime)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01((time - startTime) / (endTime - startTime));
            float minRadius = Mathf.Lerp(startMinMax.min, endMinMax.min, progress);
            float maxRadius = Mathf.Lerp(startMinMax.max, endMinMax.max, progress);
            if (player != null && !isHit)
            {
                Vector3 flatCenter = transform.position; flatCenter.y = 0f;
                Vector3 flatPlayer = player.position; flatPlayer.y = 0f;
                float distance = Vector3.Distance(flatCenter, flatPlayer);
                if (player.position.y <= transform.position.y)
                {
                    if (distance >= minRadius && distance <= maxRadius)
                    {
                        isHit = true;
                        Singleton.Player.TakeDamage(attackType, damage);
                    }
                }
            }
            yield return null;
        }
    }
}
