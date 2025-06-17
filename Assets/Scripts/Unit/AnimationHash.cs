using System.Collections.Generic;
using UnityEngine;

public static class AnimationHash
{
    public static Dictionary<ActionType, int> ActionHash;
    public static Dictionary<WeaponType, int> WeaponHash;
    public static Dictionary<string, int> ExtraHash;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        ActionHash = new Dictionary<ActionType, int>();
        WeaponHash = new Dictionary<WeaponType, int>();
        ExtraHash = new Dictionary<string, int>();
    }

    public static int GetHash(ActionType _action)
    {
        if (!ActionHash.ContainsKey(_action))
        {
            ActionHash.Add(_action, Animator.StringToHash(_action.ToString()));
        }

        return ActionHash[_action];
    }

    public static int GetHash(WeaponType _weaponType)
    {
        if (!WeaponHash.ContainsKey(_weaponType))
        {
            WeaponHash.Add(_weaponType, Animator.StringToHash(_weaponType.ToString()));
        }

        return WeaponHash[_weaponType];
    }

    public static int GetHash(string _name)
    {
        if (!ExtraHash.ContainsKey(_name))
        {
            ExtraHash.Add(_name, Animator.StringToHash(_name));
        }

        return ExtraHash[_name];
    }
}
