using System.Collections.Generic;
using UnityEngine;

using GameStuff;
using SoundStuff;

public class LayerManager : MonoBehaviour
{
    public int InitializationPriority => 1;
    private Dictionary<LayerType, int> layerMasks;

    private void Awake()
    {
        if (Singleton.Add<LayerManager>(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        layerMasks = new Dictionary<LayerType, int>();
    }

    public int GetLayerMask(LayerType _layer)
    {
        if (!layerMasks.ContainsKey(_layer))
        {
            layerMasks.Add(_layer, LayerMask.GetMask(_layer.ToString()));
        }

        return layerMasks[_layer];
    }
}
