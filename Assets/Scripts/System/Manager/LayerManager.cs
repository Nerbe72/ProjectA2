using System.Collections.Generic;
using UnityEngine;

public class LayerManager : MonoBehaviour
{
    public int InitializationPriority => 1;
    private Dictionary<LayerType, int> layermasks;
    private void Awake()
    {
        if (Singleton.Add<LayerManager>(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        layermasks = new Dictionary<LayerType, int>();
    }

    public int GetLayerMask(LayerType _layer)
    {
        if (!layermasks.ContainsKey(_layer))
        {
            layermasks.Add(_layer, LayerMask.GetMask(_layer.ToString()));
        }

        return layermasks[_layer];
    }
}
