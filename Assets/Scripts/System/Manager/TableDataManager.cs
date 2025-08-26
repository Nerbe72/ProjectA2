using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using GameStuff;

public class TableDataManager : MonoBehaviour
{
    public int InitializationPriority => 0;
    private TableMgr table;
    public TableMgr Table { get { return table; } }

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        table = new TableMgr();
        table.Init();
        Debug.Log($"테이블 init됨{!Singleton.Get<TableDataManager>().IsUnityNull()}");
    }

    // 아이템 테이블에서 무기만 추출 후 레어도에 맞는 id들만 반환
    public List<int> GetItemIDsByRare(Rarity targetRare)
    {
        var itemTable = table.Item.Dictionary;
        List<int> result = new List<int>();

        foreach (var pair in itemTable)
        {
            int id = pair.Key;
            var item = pair.Value;

            if ((ItemType)item.ItemType == ItemType.Weapon)
            {
                if ((Rarity)item.Rarity == targetRare)
                {
                    result.Add(id);
                }
            }
        }
        return result;
    }
}
