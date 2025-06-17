using UnityEngine;
using UnityEditor;

public class FindMissingReferences : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts In Scene")]
    public static void FindMissingScriptsInScene()
    {
        GameObject[] gameObjects = FindObjectsOfType<GameObject>(true);
        int missingScriptCount = 0;
        int foundObjects = 0;

        Debug.Log("=============== Missing Scripts Search Started ===============");

        foreach (GameObject go in gameObjects)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component == null)
                {
                    Debug.LogError($"[Missing Script] GameObject: {GetGameObjectPath(go)}", go);
                    missingScriptCount++;
                    if (foundObjects == 0) foundObjects++; // 첫 번째 문제를 찾은 오브젝트
                }
            }
        }

        if (missingScriptCount == 0)
        {
            Debug.Log("No missing scripts found in the current scene.");
        }
        else
        {
            Debug.LogWarning($"{missingScriptCount} missing script(s) found. Check the console for details.");
        }
        Debug.Log("=============== Missing Scripts Search Finished ===============");
    }

    [MenuItem("Tools/Find All Missing References In Scene")]
    public static void FindAllMissingReferencesInScene()
    {
        GameObject[] gameObjects = FindObjectsOfType<GameObject>(true);
        int missingRefCount = 0;

        Debug.Log("=============== All Missing References Search Started ===============");

        foreach (GameObject go in gameObjects)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component == null)
                {
                    Debug.LogError($"[Missing Script] GameObject: {GetGameObjectPath(go)}", go);
                    missingRefCount++;
                }
            }

            if (go != null)
            {
                SerializedObject serializedObject = new SerializedObject(go);
                SerializedProperty prop = serializedObject.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                        prop.objectReferenceValue == null &&
                        prop.objectReferenceInstanceIDValue != 0)
                    {
                        Debug.LogError($"[Missing Reference] GameObject: {GetGameObjectPath(go)}, Property: {prop.displayName} ({prop.propertyPath})", go);
                        missingRefCount++;
                    }
                }

                foreach (Component component in components)
                {
                    if (component != null)
                    {
                        SerializedObject componentSO = new SerializedObject(component);
                        SerializedProperty componentProp = componentSO.GetIterator();
                        while (componentProp.NextVisible(true))
                        {
                            if (componentProp.propertyType == SerializedPropertyType.ObjectReference &&
                                componentProp.objectReferenceValue == null &&
                                componentProp.objectReferenceInstanceIDValue != 0)
                            {
                                Debug.LogError($"[Missing Reference] GameObject: {GetGameObjectPath(go)}, Component: {component.GetType().Name}, Property: {componentProp.displayName} ({componentProp.propertyPath})", component.gameObject);
                                missingRefCount++;
                            }
                        }
                    }
                }
            }
        }

        if (missingRefCount == 0)
        {
            Debug.Log("No missing references found in the current scene.");
        }
        else
        {
            Debug.LogWarning($"{missingRefCount} missing reference(s) found. Check the console for details.");
        }
        Debug.Log("=============== All Missing References Search Finished ===============");
    }

    private static string GetGameObjectPath(GameObject obj)
    {
        if (obj == null) return "[Object is null or destroyed]";
        string path = "/" + obj.name;
        Transform currentParent = obj.transform.parent;
        while (currentParent != null)
        {
            path = "/" + currentParent.name + path;
            currentParent = currentParent.parent;
        }
        return path;
    }

    [MenuItem("Tools/Find Missing m_Targets In GameObjectInspector")]
    public static void FindMissingTargetsInGameObjectInspector()
    {
        GameObject[] gameObjects = FindObjectsOfType<GameObject>(true);
        int issueCount = 0;
        Debug.Log("=============== Find Missing 'm_Targets' in GameObjectInspector Started ===============");
        foreach (GameObject go in gameObjects)
        {
            Component[] comps = go.GetComponents<Component>();
            foreach (Component comp in comps)
            {
                if (comp != null && comp.GetType().Name == "GameObjectInspector")
                {
                    SerializedObject so = new SerializedObject(comp);
                    SerializedProperty prop = so.FindProperty("m_Targets");
                    if (prop == null)
                    {
                        Debug.LogError($"[Missing Field] {GetGameObjectPath(go)}: GameObjectInspector missing 'm_Targets'", go);
                        issueCount++;
                    }
                    else if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                             prop.objectReferenceValue == null &&
                             prop.objectReferenceInstanceIDValue != 0)
                    {
                        Debug.LogError($"[Null Reference] {GetGameObjectPath(go)}: 'm_Targets' is null", go);
                        issueCount++;
                    }
                    else if (prop.isArray)
                    {
                        for (int i = 0; i < prop.arraySize; i++)
                        {
                            SerializedProperty element = prop.GetArrayElementAtIndex(i);
                            if (element.propertyType == SerializedPropertyType.ObjectReference &&
                                element.objectReferenceValue == null &&
                                element.objectReferenceInstanceIDValue != 0)
                            {
                                Debug.LogError($"[Null Element] {GetGameObjectPath(go)}: 'm_Targets[{i}]' is null", go);
                                issueCount++;
                                break;
                            }
                        }
                    }
                }
            }
        }
        if (issueCount == 0)
            Debug.Log("No missing 'm_Targets' issues found in GameObjectInspector components.");
        else
            Debug.LogWarning($"{issueCount} issue(s) found. Please reassign or remove problematic components.");
        Debug.Log("=============== Find Missing 'm_Targets' Finished ===============");
    }
} 