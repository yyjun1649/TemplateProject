using UnityEngine;

public class TutorialTarget : MonoBehaviour
{
    [SerializeField] private string targetID;
    [SerializeField] private RectTransform highlightArea;
    
    #if UNITY_EDITOR
    [Header("에디터 전용")]
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private bool showGizmo = true;
    #endif
    
    public RectTransform HighlightArea => highlightArea ? highlightArea : GetComponent<RectTransform>();
    
    public string GetTargetID() => targetID;
    
    private void OnEnable()
    {
        TutorialManager.Instance.RegisterTarget(targetID, this);
    }
    
    private void OnDisable()
    {
        TutorialManager.Instance.UnregisterTarget(targetID);
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        var rect = HighlightArea;
        if (rect == null) return;
        
        // 3D 상자 그리기
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        
        Gizmos.color = gizmoColor;
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
        
        // targetID 표시
        UnityEditor.Handles.Label(corners[0], targetID, new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = gizmoColor },
            fontSize = 12
        });
    }
    #endif
}