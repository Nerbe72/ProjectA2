using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class ResourceLoader
{
    private static Dictionary<(string name, LoadType type), Object> objects = new Dictionary<(string name, LoadType type), Object>();

    public static T Load<T>(string _name, LoadType _type) where T : UnityEngine.Object
    {
        if (objects.TryGetValue((_name, _type), out Object obj))
        {
            return obj as T;
        }

        string fileName = _name;
        string path = Path.Combine("Prefabs", _type.ToString(), fileName);
        var loaded = Resources.Load<T>(path);

        if (loaded == null) return null;

        objects.Add((_name, _type), loaded);
        return loaded;
    }

    public static async Task<T> LoadAsync<T>(string _name, LoadType _type) where T : UnityEngine.Object
    {
        if (objects.TryGetValue((_name, _type), out Object obj))
        {
            return obj as T;
        }

        string fileName = _name;
        string path = Path.Combine("Prefabs", _type.ToString(), fileName);

        var loaded = Resources.LoadAsync<T>(path);

        if (!loaded.isDone)
        {
            await Task.Yield();
        }

        if (loaded == null) return null;

        T result = loaded.asset as T;

        if (objects.TryGetValue((_name, _type), out obj))
        {
            return obj as T;
        }

        objects.Add((_name, _type), result);
        return result;
    }
}
