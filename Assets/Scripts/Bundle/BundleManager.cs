using System.Collections;
using System.IO;
using UnityEngine;

public class BundleManager : MonoBehaviour
{
    private void Start()
    {
        //StartCoroutine(Load());
    }

    private IEnumerator Load()
    {
        AssetBundleCreateRequest async = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "weapon.sword_7"));

        yield return async;

        AssetBundle local = async.assetBundle;

        if (local == null)
            yield break;
    }
}
