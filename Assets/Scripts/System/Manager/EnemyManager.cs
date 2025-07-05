using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour
{
    public int InitializationPriority => 1;

    private TableEnemy tableEnemy;

    [SerializeField]  private List<GameObject> enemyPrefabs = new();
    private readonly Dictionary<int, GameObject> enemyPrefabDictionary = new();

    private readonly Dictionary<int, GameObject> spawnedEnemies = new();

    private bool isInitialized = false;

    public Dictionary<Enemy, Coroutine> RevivingEnemy = new();

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
        tableEnemy = Singleton.Get<TableDataManager>().Table.Enemy;

        // 인덱싱 딕셔너리 초기화
        foreach (var prefab in enemyPrefabs)
        {
            if (prefab == null) continue;
            Enemy enemyComponent = prefab.GetComponent<Enemy>();
            if (enemyComponent == null || enemyComponent.EnemyData == null) continue;
            int id = enemyComponent.EnemyData.ID;
            if (!enemyPrefabDictionary.ContainsKey(id))
            {
                enemyPrefabDictionary.Add(id, prefab);
            }
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private async void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
    {
        var mapManager = Singleton.Get<MapManager>();
        if (mapManager == null)
        {
            Debug.Log("MapManager 초기화되지 않음: EnemyManager");
            return;
        }

        int currentMap = mapManager.CurrentMapID;
        if (_scene.buildIndex == currentMap && currentMap >= 4)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                await SpawnMapEnemiesAsync();
            }
        }
    }

    public async Task SpawnMapEnemiesAsync()
    {
        if (tableEnemy == null)
        {
            Debug.LogError("테이블 로드 안됨");
            return;
        }

        var currentMapID = Singleton.Get<MapManager>().CurrentMapID;

        // 현재 맵에 해당하며 아직 생성되지 않은 적 목록만 필터링
        List<TableEnemy.Info> toSpawn = tableEnemy.Dictionary.Values
            .Where(info => info.MapID == currentMapID && !spawnedEnemies.ContainsKey(info.SpawnID))
            .ToList();

        foreach (TableEnemy.Info info in toSpawn)
        {
            SpawnEnemy(info);
            await Task.Yield();
        }
    }

    private void SpawnEnemy(TableEnemy.Info _info)
    {
        if (spawnedEnemies.ContainsKey(_info.SpawnID))
            return;

        if (!enemyPrefabDictionary.TryGetValue(_info.EnemyID, out GameObject prefab))
        {
            Debug.LogError($"적 딕셔너리 요청 오류: EnemyID {_info.EnemyID}");
            return;
        }

        Vector3 position = new Vector3(_info.SpawnPositionX, _info.SpawnPositionY, _info.SpawnPositionZ);
        Quaternion rotation = Quaternion.Euler(0f, _info.SpawnRotationY, 0f);

        GameObject obj = PhotonNetwork.Instantiate(prefab.name, position, rotation);
        if (obj == null)
        {
            Debug.LogError($"오브젝트 instantiate 실패: {prefab.name}");
            return;
        }
        
        spawnedEnemies.Add(_info.SpawnID, obj);

        Enemy enemyComponent = obj.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.SpawnPoint_Enemy = position;
            enemyComponent.SpawnRotation_Enemy = rotation;
        }
    }

    public void SetDeadFlag(Enemy _enemy)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
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
        //사망 애니메이션 대기
        yield return new WaitForSeconds(1f);

        if (_target != null)
        {
            Vector3 hidePosition = new Vector3(0, -1000, 0);
            _target.transform.position = hidePosition;
            _target.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(_time);

        if (_target == null || _target.photonView == null) yield break;

        _target.gameObject.SetActive(true);
        _target.Respawn(); // 로컬 리스폰 처리
        
        // 모든 클라이언트에 리스폰 동기화 - PhotonView RPC 호출
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"[마스터] 적 리스폰 RPC 호출: {_target.gameObject.name}");
            _target.photonView.RPC("SyncRespawn", RpcTarget.All);
            
        }

        yield break;
    }
}
