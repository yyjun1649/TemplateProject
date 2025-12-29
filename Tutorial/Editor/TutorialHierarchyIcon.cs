#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class TutorialHierarchyIcon
{
    static TutorialHierarchyIcon()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
    }
    
    private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;
        
        var tutorialTarget = obj.GetComponent<TutorialTarget>();
        if (tutorialTarget == null) return;
        
        // 아이콘 표시
        Rect iconRect = new Rect(selectionRect.xMax - 20, selectionRect.y, 18, 18);
        GUI.Label(iconRect, "🎯");
    }
}
#endif