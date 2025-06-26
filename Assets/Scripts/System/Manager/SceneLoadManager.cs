using UnityEngine;

public static class SceneLoadManager
{
    public static MapConnection SelectedConnection;
    public static Map NextScene = Map.None;
    public static Vector3 NextPosition;
    public static Quaternion NextRotation = Quaternion.identity;
}
