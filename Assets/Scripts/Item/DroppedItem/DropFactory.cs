using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class DropFactory : MonoBehaviour
{
    [Header("드롭 프리팹")]
    [SerializeField] private GameObject droppedItemPrefab;
    
    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }
    
    public async Task<List<DroppedItem>> CreateDrops(EnemyData _enemyData, Vector3 _position)
    {
        if (_enemyData == null)
        {
            Debug.LogWarning("EnemyData is null!");
            return new List<DroppedItem>();
        }
        
        List<DroppedItem> droppedItems = new List<DroppedItem>();
        
        // 기본 드롭 확률 체크
        if (Random.Range(0f, 100f) > _enemyData.BaseDropChance)
            return droppedItems;
        
        // 드롭할 아이템들 선택
        List<DropItemData> selectedDrops = SelectDrops(_enemyData);
        
        // 드롭 아이템 생성
        for (int i = 0; i < selectedDrops.Count; i++)
        {
            var dropData = selectedDrops[i];
            var droppedItem = await CreateDroppedItem(dropData, _position);
            if (droppedItem != null)
                droppedItems.Add(droppedItem);
        }
        
        return droppedItems;
    }
    
    private List<DropItemData> SelectDrops(EnemyData _enemyData)
    {
        List<DropItemData> selectedDrops = new List<DropItemData>();
        
        for (int i = 0; i < _enemyData.DropItems.Length; i++)
        {
            var dropItem = _enemyData.DropItems[i];
            
            // 개별 아이템 드롭 확률 체크
            if (Random.Range(0f, 100f) <= dropItem.DropRate)
            {
                selectedDrops.Add(dropItem);
                
                // 최대 드롭 개수 체크
                if (selectedDrops.Count >= _enemyData.MaxDropsPerKill)
                    break;
            }
        }
        
        return selectedDrops;
    }
    
    private async Task<DroppedItem> CreateDroppedItem(DropItemData _dropData, Vector3 _position)
    {
        if (droppedItemPrefab == null)
        {
            Debug.LogError("DroppedItem prefab is not assigned!");
            return null;
        }
        
        // 수량 설정
        int quantity = Random.Range(_dropData.MinQuantity, _dropData.MaxQuantity + 1);
        
        // 랜덤 드롭 위치
        Vector3 dropPosition = _position + new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        );
        
        GameObject droppedItemObj = Instantiate(droppedItemPrefab, dropPosition, Quaternion.identity);
        var droppedItem = droppedItemObj.GetComponent<DroppedItem>();
        
        if (droppedItem != null)
        {
            ((IItemContainer)droppedItem).SetItemContainer(_dropData.ItemID, quantity);
        }
        
        return droppedItem;
    }
    

} 