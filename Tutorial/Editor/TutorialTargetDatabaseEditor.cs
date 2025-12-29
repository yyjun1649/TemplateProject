#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TutorialTargetDatabase))]
public class TutorialTargetDatabaseEditor : Editor
{
    private TutorialTargetDatabase database;
    private Vector2 scrollPosition;
    private string searchFilter = "";
    
    private void OnEnable()
    {
        database = (TutorialTargetDatabase)target;
    }
    
    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("튜토리얼 타겟 데이터베이스", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "프로젝트 내 모든 TutorialTarget을 스캔하여 관리합니다.\n" +
            "팝업 Prefab을 수정하거나 새로 추가한 경우 '전체 스캔' 버튼을 눌러주세요.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        // 스캔 버튼
        if (GUILayout.Button("🔍 전체 스캔 (Scene + Prefabs)", GUILayout.Height(40)))
        {
            database.ScanAllTargets();
        }
        
        EditorGUILayout.Space(10);
        
        // 검색 필터
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("검색:", GUILayout.Width(50));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        if (GUILayout.Button("✕", GUILayout.Width(30)))
        {
            searchFilter = "";
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // 통계
        DrawStatistics();
        
        EditorGUILayout.Space(10);
        
        // 타겟 목록
        EditorGUILayout.LabelField($"등록된 타겟 목록 ({database.registeredTargets.Count}개)", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
        DrawTargetList();
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawStatistics()
    {
        int sceneCount = database.registeredTargets.FindAll(t => t.isInScene).Count;
        int prefabCount = database.registeredTargets.FindAll(t => t.isInPrefab).Count;
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("통계", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Scene: {sceneCount}개");
        EditorGUILayout.LabelField($"Prefab: {prefabCount}개");
        EditorGUILayout.LabelField($"전체: {database.registeredTargets.Count}개");
        EditorGUILayout.EndVertical();
    }
    
    private void DrawTargetList()
    {
        foreach (var target in database.registeredTargets)
        {
            // 검색 필터 적용
            if (!string.IsNullOrEmpty(searchFilter) && 
                !target.targetID.ToLower().Contains(searchFilter.ToLower()))
            {
                continue;
            }
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            // targetID
            EditorGUILayout.LabelField(target.targetID, EditorStyles.boldLabel);
            
            // 위치 태그
            if (target.isInScene)
            {
                GUI.color = Color.green;
                GUILayout.Label("Scene", EditorStyles.miniButton, GUILayout.Width(50));
                GUI.color = Color.white;
            }
            
            if (target.isInPrefab)
            {
                GUI.color = Color.cyan;
                GUILayout.Label("Prefab", EditorStyles.miniButton, GUILayout.Width(50));
                GUI.color = Color.white;
            }
            
            // Asset 선택 버튼
            if (GUILayout.Button("→", GUILayout.Width(30)))
            {
                PingAsset(target.assetPath);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Asset 경로
            EditorGUILayout.LabelField(target.assetPath, EditorStyles.miniLabel);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
    }
    
    private void PingAsset(string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (asset != null)
        {
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }
    }
}
#endif