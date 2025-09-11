using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Singleton
{
    // key: typeof(T), value: instance
    private static Dictionary<System.Type, (int priority, UnityEngine.MonoBehaviour data)> singletons = new Dictionary<System.Type, (int priority, UnityEngine.MonoBehaviour data)>();
    private static int lastPriority = 0;
    public static Exit exit;

    #region Player
    public static Player Player = null;
    public static Inventory Inventory = null;
    #endregion

    public static T Get<T>() where T : class
    {
        var type = typeof(T);

        if (singletons.ContainsKey(type))
        {
            if (singletons[type].data != null)
            {
                return singletons[type].data as T;
            }
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_instance"></param>
    /// <returns>singleton 사전 존재 여부</returns>
    public static bool Add<T>(T _instance) where T : UnityEngine.MonoBehaviour
    {
        var type = typeof(T);
        if (singletons.ContainsKey(type))
        {
            var existingInstance = singletons[type].data as UnityEngine.MonoBehaviour;
            if (existingInstance != null)
            {
                return true;
            }
            else
            {
                singletons[type] = (lastPriority, _instance);
                lastPriority += 1;
                return false;
            }
        }

        singletons.Add(typeof(T), (lastPriority, _instance));

        lastPriority += 1;
        return false;
    }

    public static void UnloadAllSingleton()
    {
        // 딕셔너리 역순 해제
        // 플레이어 해제
        // 인벤토리 해제

        List<(int priority, MonoBehaviour data)> values = (singletons.Values).ToList();
        values.OrderByDescending((v) => { return (v.priority); });

        int count = values.Count;

        for (int i = count - 1; i >= 0; i--)
        {
            var value = values[i];
            if (value.data != null)
            {
                Object.Destroy(value.data.gameObject);
            }
        }

        lastPriority = 0;
        singletons.Clear();

        Object.Destroy(Player.gameObject);
        Object.Destroy(Inventory.gameObject);
        Player = null;
        Inventory = null;
    }
}
