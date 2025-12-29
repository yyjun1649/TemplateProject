#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using DebugUtil = Library.DebugUtil;

[CustomEditor(typeof(TutorialSequenceData))]
public class TutorialSequenceEditor : Editor
{
    private TutorialSequenceData sequence;
    private SerializedProperty stepsProperty;
    private int selectedStepIndex = -1;
    private Vector2 scrollPosition;
    
    // Database 참조
    private TutorialTargetDatabase database;
    
    private void OnEnable()
    {
        sequence = (TutorialSequenceData)target;
        stepsProperty = serializedObject.FindProperty("steps");
        
        // Database 찾기
        LoadDatabase();
    }
    
    private void LoadDatabase()
    {
        // 프로젝트에서 Database 찾기
        string[] guids = AssetDatabase.FindAssets("t:TutorialTargetDatabase");
        
        if (guids.Length == 0)
        {
            DebugUtil.LogWarning("[Tutorial] TutorialTargetDatabase를 찾을 수 없습니다. 생성하세요.");
            return;
        }
        
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        database = AssetDatabase.LoadAssetAtPath<TutorialTargetDatabase>(path);
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Database 경고
        if (database == null)
        {
            EditorGUILayout.HelpBox(
                "TutorialTargetDatabase가 없습니다!\n" +
                "Create > Tutorial > Target Database로 생성하세요.",
                MessageType.Error
            );
            
            if (GUILayout.Button("Database 생성"))
            {
                CreateDatabase();
            }
            
            return;
        }
        
        // 헤더
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("튜토리얼 시퀀스 편집기", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Database 상태 표시
        DrawDatabaseStatus();
        
        // 기본 정보
        DrawBasicInfo();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("스텝 목록", EditorStyles.boldLabel);
        
        // 툴바
        DrawToolbar();
        
        EditorGUILayout.Space(5);
        
        // 스텝 리스트
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
        DrawStepList();
        EditorGUILayout.EndScrollView();
        
        // 선택된 스텝 상세 편집
        if (selectedStepIndex >= 0 && selectedStepIndex < sequence.steps.Count)
        {
            EditorGUILayout.Space(10);
            DrawStepDetails(selectedStepIndex);
        }
        
        serializedObject.ApplyModifiedProperties();
        
        // 하단 액션 버튼
        EditorGUILayout.Space(10);
        DrawActionButtons();
    }
    
    private void DrawDatabaseStatus()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField($"📊 Database: {database.registeredTargets.Count}개 타겟", EditorStyles.miniLabel);
        
        if (GUILayout.Button("🔄 새로고침", GUILayout.Width(80)))
        {
            database.ScanAllTargets();
        }
        
        if (GUILayout.Button("열기", GUILayout.Width(50)))
        {
            Selection.activeObject = database;
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawBasicInfo()
    {
        sequence.sequenceName = EditorGUILayout.TextField("시퀀스 이름", sequence.sequenceName);
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("+ 새 스텝 추가", GUILayout.Height(30)))
        {
            AddNewStep();
        }
        
        GUI.enabled = selectedStepIndex >= 0;
        if (GUILayout.Button("복제", GUILayout.Width(60), GUILayout.Height(30)))
        {
            DuplicateStep(selectedStepIndex);
        }
        
        if (GUILayout.Button("삭제", GUILayout.Width(60), GUILayout.Height(30)))
        {
            DeleteStep(selectedStepIndex);
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawStepList()
    {
        for (int i = 0; i < sequence.steps.Count; i++)
        {
            var step = sequence.steps[i];
            if (step == null) continue;
            
            bool isSelected = i == selectedStepIndex;
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = isSelected ? new Color(0.5f, 0.7f, 1f) : Color.white;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = originalColor;
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button(i.ToString(), GUILayout.Width(30)))
            {
                selectedStepIndex = i;
            }
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField($"타겟: {step.targetID}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"메시지: {(string.IsNullOrEmpty(step.messageText) ? "(없음)" : step.messageText.Substring(0, Mathf.Min(30, step.messageText.Length)) + "...")}", EditorStyles.miniLabel);
            
            // Database 기반 검증
            DrawValidationStatus(step);
            
            EditorGUILayout.EndVertical();
            
            // 순서 변경 버튼
            EditorGUILayout.BeginVertical(GUILayout.Width(40));
            GUI.enabled = i > 0;
            if (GUILayout.Button("▲", GUILayout.Width(30)))
            {
                MoveStep(i, i - 1);
            }
            GUI.enabled = i < sequence.steps.Count - 1;
            if (GUILayout.Button("▼", GUILayout.Width(30)))
            {
                MoveStep(i, i + 1);
            }
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(2);
        }
    }
    
    private void DrawValidationStatus(TutorialStepData step)
    {
        EditorGUILayout.BeginHorizontal();
        
        if (string.IsNullOrEmpty(step.targetID))
        {
            GUI.color = Color.red;
            GUILayout.Label("✗ ID 없음", EditorStyles.miniLabel);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            return;
        }
        
        // Database에서 확인
        var targetInfo = database.GetTargetInfo(step.targetID);
        
        if (targetInfo == null)
        {
            GUI.color = Color.red;
            GUILayout.Label("✗ 타겟 없음 (Database 새로고침 필요)", EditorStyles.miniLabel);
            GUI.color = Color.white;
        }
        else
        {
            GUI.color = Color.green;
            string location = targetInfo.isInScene ? "Scene" : "Prefab";
            GUILayout.Label($"✓ {location}", EditorStyles.miniLabel);
            GUI.color = Color.white;
            
            // Asset 경로 표시
            GUILayout.Label($"({targetInfo.assetPath})", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawStepDetails(int index)
    {
        EditorGUILayout.LabelField("스텝 상세 설정", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        var step = sequence.steps[index];
        
        EditorGUILayout.LabelField("타겟 설정", EditorStyles.boldLabel);
        
        // targetID 입력 + 자동완성 버튼
        EditorGUILayout.BeginHorizontal();
        step.targetID = EditorGUILayout.TextField("Target ID", step.targetID);
        if (GUILayout.Button("🔍", GUILayout.Width(30)))
        {
            ShowTargetIDPicker(step);
        }
        EditorGUILayout.EndHorizontal();
        
        // Database 정보 표시
        if (!string.IsNullOrEmpty(step.targetID))
        {
            var targetInfo = database.GetTargetInfo(step.targetID);
            if (targetInfo != null)
            {
                EditorGUILayout.HelpBox(
                    $"위치: {targetInfo.assetPath}\n" +
                    $"타입: {(targetInfo.isInScene ? "Scene" : "")} {(targetInfo.isInPrefab ? "Prefab" : "")}",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "⚠ Database에 없는 타겟입니다. '새로고침' 버튼을 눌러주세요.",
                    MessageType.Warning
                );
            }
        }
        
        step.spotlightPadding = EditorGUILayout.Vector2Field("스포트라이트 여백", step.spotlightPadding);
        step.cornerRadius = EditorGUILayout.FloatField("둥근 모서리", step.cornerRadius);
        step.spotlightOffset = EditorGUILayout.Vector2Field("스포트라이트 오프셋", step.spotlightOffset);
        EditorGUILayout.HelpBox("오프셋은 스포트라이트와 화살표에만 적용되며, 말풍선은 원래 타겟 위치 기준입니다.", MessageType.Info);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("메시지 설정", EditorStyles.boldLabel);
        
        step.messageText = EditorGUILayout.TextArea(step.messageText, GUILayout.Height(60));
        step.messagePosition = (MessagePosition)EditorGUILayout.EnumPopup("메시지 위치", step.messagePosition);

        step.arrowPosition = (MessagePosition)EditorGUILayout.EnumPopup("화살표 위치", step.arrowPosition);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("진행 조건", EditorStyles.boldLabel);
        
        step.autoProgressDelay = EditorGUILayout.FloatField("자동 진행 시간 (초)", step.autoProgressDelay);
        EditorGUILayout.HelpBox("자동 진행 시간이 -1이면 사용자 클릭을 대기합니다.", MessageType.Info);
        
        step.appearDelay = EditorGUILayout.FloatField("등장 딜레이 (초)", step.appearDelay);

        step.waitForEvent = (GuideWaitType)EditorGUILayout.EnumPopup("대기 이벤트 타입", step.waitForEvent);
        step.waitEventName = EditorGUILayout.TextField("대기 이벤트 이름", step.waitEventName);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("애니메이션", EditorStyles.boldLabel);
        
        step.fadeInDuration = EditorGUILayout.FloatField("페이드인 시간", step.fadeInDuration);
        step.enablePulse = EditorGUILayout.Toggle("펄스 효과", step.enablePulse);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("종료 이벤트", EditorStyles.boldLabel);
        step.endEvent = (GuideEndEvent)EditorGUILayout.EnumPopup("종료 이벤트 타입", step.endEvent);
        step.endEventName = EditorGUILayout.TextField("종료 이벤트 이름", step.endEventName);
        
        EditorGUILayout.EndVertical();
        
        EditorUtility.SetDirty(step);
    }
    
    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("▶ 게임에서 테스트", GUILayout.Height(40)))
        {
            TestInGame();
        }
        
        if (GUILayout.Button("검증 실행", GUILayout.Height(40)))
        {
            ValidateSequence();
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    // ===== 유틸리티 메서드 =====
    
    private void ShowTargetIDPicker(TutorialStepData step)
    {
        // Database에서 모든 targetID 가져오기
        string[] targetIDs = database.GetAllTargetIDs();
        
        if (targetIDs.Length == 0)
        {
            EditorUtility.DisplayDialog("타겟 없음", "Database가 비어있습니다. '새로고침' 버튼을 눌러주세요.", "확인");
            return;
        }
        
        GenericMenu menu = new GenericMenu();
        foreach (var id in targetIDs)
        {
            menu.AddItem(new GUIContent(id), false, () =>
            {
                step.targetID = id;
                EditorUtility.SetDirty(step);
            });
        }
        menu.ShowAsContext();
    }
    
    private void ValidateSequence()
    {
        int errorCount = 0;
        int warningCount = 0;
        string report = "=== 튜토리얼 검증 결과 ===\n\n";
        
        if (sequence.steps.Count == 0)
        {
            report += "❌ 스텝이 하나도 없습니다.\n";
            errorCount++;
        }
        
        for (int i = 0; i < sequence.steps.Count; i++)
        {
            var step = sequence.steps[i];
            
            if (string.IsNullOrEmpty(step.targetID))
            {
                report += $"❌ Step {i}: targetID가 비어있습니다.\n";
                errorCount++;
            }
            else
            {
                var targetInfo = database.GetTargetInfo(step.targetID);
                if (targetInfo == null)
                {
                    report += $"❌ Step {i}: targetID '{step.targetID}'가 Database에 없습니다. (Database 새로고침 필요)\n";
                    errorCount++;
                }
            }
            
            if (string.IsNullOrEmpty(step.messageText))
            {
                report += $"⚠ Step {i}: 메시지가 비어있습니다.\n";
                warningCount++;
            }
        }
        
        if (errorCount == 0)
        {
            report += "\n✓ 모든 검증을 통과했습니다!";
            if (warningCount > 0)
            {
                report += $"\n({warningCount}개의 경고)";
            }
        }
        else
        {
            report += $"\n총 {errorCount}개의 오류, {warningCount}개의 경고가 발견되었습니다.";
        }
        
        EditorUtility.DisplayDialog("검증 결과", report, "확인");
    }
    
    private void CreateDatabase()
    {
        var database = CreateInstance<TutorialTargetDatabase>();
        AssetDatabase.CreateAsset(database, "Assets/TutorialTargetDatabase.asset");
        AssetDatabase.SaveAssets();
        
        this.database = database;
        EditorUtility.SetDirty(this);
    }
    
    // ... 나머지 메서드들은 동일 ...
    
    private void AddNewStep()
    {
        var newStep = CreateInstance<TutorialStepData>();
        newStep.name = $"Step_{sequence.steps.Count + 1}";
        
        string assetPath = AssetDatabase.GetAssetPath(sequence);
        string directory = System.IO.Path.GetDirectoryName(assetPath);
        string newStepPath = $"{directory}/{newStep.name}.asset";
        
        AssetDatabase.CreateAsset(newStep, newStepPath);
        sequence.steps.Add(newStep);
        selectedStepIndex = sequence.steps.Count - 1;
        
        EditorUtility.SetDirty(sequence);
        AssetDatabase.SaveAssets();
    }
    
    private void DuplicateStep(int index)
    {
        if (index < 0 || index >= sequence.steps.Count) return;
        
        var original = sequence.steps[index];
        var duplicate = Instantiate(original);
        duplicate.name = $"{original.name}_Copy";
        
        string assetPath = AssetDatabase.GetAssetPath(sequence);
        string directory = System.IO.Path.GetDirectoryName(assetPath);
        string newPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{duplicate.name}.asset");
        
        AssetDatabase.CreateAsset(duplicate, newPath);
        sequence.steps.Insert(index + 1, duplicate);
        selectedStepIndex = index + 1;
        
        EditorUtility.SetDirty(sequence);
        AssetDatabase.SaveAssets();
    }
    
    private void DeleteStep(int index)
    {
        if (index < 0 || index >= sequence.steps.Count) return;
        
        if (EditorUtility.DisplayDialog("스텝 삭제", "정말 이 스텝을 삭제하시겠습니까?", "삭제", "취소"))
        {
            sequence.steps.RemoveAt(index);
            selectedStepIndex = -1;
            EditorUtility.SetDirty(sequence);
        }
    }
    
    private void MoveStep(int from, int to)
    {
        if (from < 0 || from >= sequence.steps.Count || to < 0 || to >= sequence.steps.Count) return;
        
        var step = sequence.steps[from];
        sequence.steps.RemoveAt(from);
        sequence.steps.Insert(to, step);
        selectedStepIndex = to;
        
        EditorUtility.SetDirty(sequence);
    }
    
    private void TestInGame()
    {
        if (!Application.isPlaying)
        {
            EditorApplication.isPlaying = true;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }
        else
        {
            ExecuteTest();
        }
    }
    
    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            ExecuteTest();
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }
    }
    
    private void ExecuteTest()
    {
        var manager = FindObjectOfType<TutorialManager>();
        if (manager != null)
        {
            manager.StartSequence(sequence);
        }
        else
        {
            DebugUtil.LogError("Scene에 TutorialManager가 없습니다.");
        }
    }
}
#endif