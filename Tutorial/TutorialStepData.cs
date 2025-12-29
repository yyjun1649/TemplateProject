using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial/Tutorial Step")]
public class TutorialStepData : ScriptableObject
{
    [Header("타겟 설정")]
    [Tooltip("튜토리얼에서 하이라이트할 UI 오브젝트의 ID")]
    public string targetID;

    [Tooltip("스포트라이트 여백 (픽셀)")]
    public Vector2 spotlightPadding = new Vector2(10, 10);

    [Tooltip("둥근 모서리 반경 (픽셀)")]
    public float cornerRadius = 50f;

    [Tooltip("스포트라이트 오프셋 (픽셀) - 스포트라이트와 화살표만 이동")]
    public Vector2 spotlightOffset = Vector2.zero;

    [Header("메시지 설정")]
    [Tooltip("로컬라이징 키")]
    [TextArea(3, 1)]
    public string messageText;

    [Tooltip("메시지 위치")]
    public MessagePosition messagePosition = MessagePosition.Top;

    [Tooltip("화살표 위치 (Up 일 경우 화살표가 위로 바라봄)")]
    public MessagePosition arrowPosition = MessagePosition.Top;

    [Header("등장 딜레이")]
    public float appearDelay = -1f;

    [Header("진행 조건")]
    [Tooltip("자동 진행 시간 (-1이면 클릭 대기)")]
    public float autoProgressDelay = -1f;

    [Tooltip("특정 이벤트 대기")]
    public GuideWaitType waitForEvent;

    public string waitEventName;

    [Header("애니메이션")]
    [Tooltip("페이드인 시간")]
    public float fadeInDuration = 0.3f;

    [Tooltip("펄스 애니메이션")]
    public bool enablePulse = true;

    [Header("종료 이벤트")]
    public GuideEndEvent endEvent;
    public string endEventName;
}

public enum MessagePosition
{
    Top,
    Bottom,
    Left,
    Right,
    Custom
}

public enum GuideWaitType
{
    None,
    PopupOn,
    PopupOff,
}

public enum GuideEndEvent
{
    None,
    PopupOn,
}