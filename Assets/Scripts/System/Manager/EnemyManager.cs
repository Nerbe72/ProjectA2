using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public int InitializationPriority => 1;

    public Dictionary<Enemy, Coroutine> RevivingEnemy = new Dictionary<Enemy, Coroutine>();

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void SetDeadFlag(Enemy _enemy)
    {
        if (RevivingEnemy.ContainsKey(_enemy))
        {
            StopCoroutine(RevivingEnemy[_enemy]);
            RevivingEnemy[_enemy] = StartCoroutine(RespawnTimer(_enemy, 5f));
            return;
        }

        RevivingEnemy.Add(_enemy, StartCoroutine(RespawnTimer(_enemy, 5f)));
    }

    public IEnumerator RespawnTimer(Enemy _target, float _time)
    {
        yield return new WaitForSeconds(1f);

        _target.gameObject.SetActive(false);

        yield return new WaitForSeconds(_time);

        _target.gameObject.SetActive(true);
        _target.Respawn();

        yield break;
    }
}
