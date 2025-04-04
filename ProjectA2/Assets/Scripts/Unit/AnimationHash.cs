using System.Collections.Generic;
using UnityEngine;

public static class AnimationHash
{
    public static Dictionary<Action, int> actionHash = new Dictionary<Action, int>();

    /// <summary>
    /// </summary>
    /// <param name="_action"></param>
    public static void AddActionHash(Action _action)
    {
        actionHash.Add(_action, Animator.StringToHash(_action.ToString()));
    }
}
