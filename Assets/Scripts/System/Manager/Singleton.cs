using System.Collections.Generic;
using UnityEngine;

public static class Singleton
{
    // key: typeof(T), value: instance
    private static Dictionary<System.Type, UnityEngine.Object> singletons = new Dictionary<System.Type, UnityEngine.Object>();

    #region Player
    public static Player Player = null;
    public static Inventory Inventory = null;
    #endregion

    public static T Get<T>() where T : class
    {
        var type = typeof(T);

        if (singletons.ContainsKey(type))
        {
            if (singletons[type] != null)
            {
                return singletons[type] as T;
            }
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_instance"></param>
    /// <returns>������ �����Ͱ� �����ϴ���</returns>
    public static bool Add<T>(T _instance) where T : UnityEngine.Object
    {
        var type = typeof(T);
        if (singletons.ContainsKey(type))
        {
            // UnityEngine.Object로 캐스팅하여 Unity의 null 체크(파괴되었는지)를 수행
            var existingInstance = singletons[type] as UnityEngine.Object;
            if (existingInstance != null)
            {
                return true; // 파괴되지 않은 유효한 인스턴스가 존재함
            }
            else
            {
                // 키는 있지만 값이 null(파괴된 경우)이므로, 새 인스턴스로 교체
                singletons[type] = _instance;
                return false;
            }
        }

        singletons.Add(typeof(T), _instance);
        return false;
    }

    public static void UnloadAllSingleton()
    {
        //bootstrapmanager���� �������� ��ε�
    }
}
