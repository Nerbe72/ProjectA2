using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BootstrapManager
{
    //public static void Init()
    //{
    //    IInitializable[] initializables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<IInitializable>().ToArray();
    //    List<IInitializable> sortedInitializables = initializables.OrderBy(init => init.InitializationPriority).ToList();
    //    int count = sortedInitializables.Count;

    //    for (int i = 0; i < count; i++)
    //    {
    //        sortedInitializables[i].Initialize();
    //        Debug.Log($"{sortedInitializables[i].InitializationPriority}:{sortedInitializables[i].GetType().Name} initµÊ");
    //    }
    //}
}