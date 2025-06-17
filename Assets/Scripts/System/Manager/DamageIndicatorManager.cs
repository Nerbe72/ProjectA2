using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DamageIndicatorManager : MonoBehaviour
{
    public int InitializationPriority => 4;
    [SerializeField] private GameObject indicatorPrefab;

    private List<GameObject> indicators;

    private void Awake()
    {
        if (Singleton.DamageIndicatorManager != null)
        {
            Destroy(gameObject);
            return;
        }

        Singleton.DamageIndicatorManager = this;
        DontDestroyOnLoad(gameObject);

        indicators = new List<GameObject>();
    }

    public void CreateIndicator(Vector3 _spawnPosition, AttackType _type, int _damage)
    {
        for(int i = 0; i < indicators.Count; i++)
        {
            if (!indicators[i].activeSelf)
            {
                indicators[i].transform.position = _spawnPosition;
                indicators[i].GetComponent<DamageIndicator>().InitIndicator(_type, _damage);
                indicators[i].SetActive(true);
                return;
            }
        }

        GameObject obj = Instantiate(indicatorPrefab, _spawnPosition, Quaternion.identity);
        obj.GetComponent<DamageIndicator>().InitIndicator(_type, _damage);
        obj.transform.SetParent(this.transform);
        obj.SetActive(true);
        indicators.Add(obj);
    }
}
