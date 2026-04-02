// csharp
using UnityEngine;
using UnityEditor;

public class PrefabFileIDFixer : EditorWindow
{
    [MenuItem("Tools/Prefab FileID Fixer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabFileIDFixer>("Prefab FileID Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Fix FileID Conflicts in Scene Prefabs", EditorStyles.boldLabel);

        if (GUILayout.Button("Replace Prefab Instances"))
        {
            ReplaceAllPrefabsInScene();
        }
    }

    private void ReplaceAllPrefabsInScene()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int replacedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            // 仅处理是 prefab 实例的最外层根（避免处理嵌套 prefab 的子对象）
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(obj))
                continue;

            GameObject nearestRoot = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
            if (nearestRoot != obj)
                continue;

            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj);
            if (prefab == null)
                continue;

            // 保存父级和局部变换以保持在原父级下的相对位置/缩放/旋转（特别是 UI）
            Transform oldTransform = obj.transform;
            Transform parent = oldTransform.parent;
            Vector3 localPosition = oldTransform.localPosition;
            Quaternion localRotation = oldTransform.localRotation;
            Vector3 localScale = oldTransform.localScale;

            // 实例化 prefab，然后恢复父级与局部变换
            GameObject newInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (newInstance == null)
                continue;

            // 将新实例置于原父级（保留层级结构），并恢复局部变换
            newInstance.transform.SetParent(parent, false);
            newInstance.transform.localPosition = localPosition;
            newInstance.transform.localRotation = localRotation;
            newInstance.transform.localScale = localScale;

            Undo.RegisterCreatedObjectUndo(newInstance, "Replace prefab instance");
            Undo.DestroyObjectImmediate(obj);
            replacedCount++;
        }

        Debug.Log($"Prefab FileID Fixer: Replaced {replacedCount} prefab instance(s) in the scene.");
    }
}
