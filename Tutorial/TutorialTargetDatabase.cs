using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using DebugUtil = Library.DebugUtil;
#endif

[CreateAssetMenu(fileName = "TutorialTargetDatabase", menuName = "Tutorial/Target Database")]
public class TutorialTargetDatabase : ScriptableObject
{
    [System.Serializable]
    public class TargetInfo
    {
        public string targetID;
        public string assetPath;
        public bool isInScene;
        public bool isInPrefab;
        
        public TargetInfo(string id, string path, bool scene, bool prefab)
        {
            targetID = id;
            assetPath = path;
            isInScene = scene;
            isInPrefab = prefab;
        }
    }
    
    public List<TargetInfo> registeredTargets = new List<TargetInfo>();
    
    #if UNITY_EDITOR
    /// <summary>
    /// 프로젝트 전체에서 TutorialTarget 스캔
    /// </summary>
    public void ScanAllTargets()
    {
        registeredTargets.Clear();
        
        // 1. Scene에서 검색
        var sceneTargets = FindObjectsOfType<TutorialTarget>(true);
        foreach (var target in sceneTargets)
        {
            string id = target.GetTargetID();
            if (!string.IsNullOrEmpty(id))
            {
                registeredTargets.Add(new TargetInfo(
                    id, 
                    target.gameObject.scene.name, 
                    true, 
                    false
                ));
            }
        }
        
        // 2. 모든 Prefab에서 검색
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab == null) continue;
            
            // Prefab 내부의 모든 TutorialTarget 검색
            var targets = prefab.GetComponentsInChildren<TutorialTarget>(true);
            
            foreach (var target in targets)
            {
                string id = target.GetTargetID();
                if (!string.IsNullOrEmpty(id))
                {
                    // 중복 체크
                    var existing = registeredTargets.Find(t => t.targetID == id);
                    if (existing != null)
                    {
                        existing.isInPrefab = true;
                    }
                    else
                    {
                        registeredTargets.Add(new TargetInfo(
                            id, 
                            path, 
                            false, 
                            true
                        ));
                    }
                }
            }
        }
        
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        DebugUtil.Log($"[TutorialTargetDatabase] 스캔 완료: {registeredTargets.Count}개의 타겟 발견");
    }
    
    /// <summary>
    /// targetID 존재 여부 확인
    /// </summary>
    public bool IsValidTargetID(string targetID)
    {
        return registeredTargets.Exists(t => t.targetID == targetID);
    }
    
    /// <summary>
    /// targetID로 정보 가져오기
    /// </summary>
    public TargetInfo GetTargetInfo(string targetID)
    {
        return registeredTargets.Find(t => t.targetID == targetID);
    }
    
    /// <summary>
    /// 모든 targetID 목록 가져오기
    /// </summary>
    public string[] GetAllTargetIDs()
    {
        return registeredTargets.ConvertAll(t => t.targetID).ToArray();
    }
    #endif
}