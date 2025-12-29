#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using DebugUtil = Library.DebugUtil;

/// <summary>
/// Tools 메뉴를 통해 Tutorial Target Database를 수동으로 갱신
/// </summary>
public static class TutorialTargetScanner
{
    [MenuItem("Tools/Tutorial/Refresh Target Database")]
    private static void RefreshTargetDatabase()
    {
        // Database 찾기
        string[] guids = AssetDatabase.FindAssets("t:TutorialTargetDatabase");
        if (guids.Length == 0)
        {
            DebugUtil.LogWarning("TutorialTargetDatabase를 찾을 수 없습니다.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var database = AssetDatabase.LoadAssetAtPath<TutorialTargetDatabase>(path);

        if (database != null)
        {
            database.ScanAllTargets();
            DebugUtil.Log("Tutorial Target Database가 갱신되었습니다.");
        }
    }
}
#endif