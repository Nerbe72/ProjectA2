using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(CircuableToggle))]
public class CircuableToggleEditor : ToggleEditor
{
    SerializedProperty isCircuable;
    SerializedProperty icon;
    SerializedProperty cycleIcons;
    SerializedProperty currentCycle;

    protected override void OnEnable()
    {
        base.OnEnable();

        isCircuable = serializedObject.FindProperty("isCircuable");
        icon = serializedObject.FindProperty("icon");
        cycleIcons = serializedObject.FindProperty("cycleIcons");
        currentCycle = serializedObject.FindProperty("currentCycle");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CircuableToggle circuableToggle = serializedObject.targetObject as CircuableToggle;
        EditorGUILayout.PropertyField(isCircuable);
        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.PropertyField(cycleIcons);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        base.OnInspectorGUI();
    }
}
