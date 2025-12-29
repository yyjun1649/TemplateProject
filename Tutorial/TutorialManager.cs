using System;
using System.Collections;
using System.Collections.Generic;
using Library;
using UnityEngine;
using UnityEngine.UI;
using DebugUtil = Library.DebugUtil;

public class TutorialManager : SingletonBehaviour<TutorialManager>
{
    [SerializeField] private TutorialDimController dimController;
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private float targetWaitTimeout = 5f;

    private Dictionary<string, TutorialTarget> registeredTargets = new Dictionary<string, TutorialTarget>();
    private TutorialTarget currentTarget;
    private TutorialStepData currentStepData;
    private bool isTrackingTarget;

    private TutorialSequenceData currentSequence;
    private int currentStepIndex = 0;

    public bool IsPlaying { get; private set; }

    /// <summary>
    /// 시퀀스 시작 (에디터 툴에서 호출)
    /// </summary>
    public void StartSequence(TutorialSequenceData sequenceData)
    {
        currentSequence = sequenceData;
        currentStepIndex = 0;

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        while (currentStepIndex < currentSequence.steps.Count)
        {
            var step = currentSequence.steps[currentStepIndex];

            IsPlaying = true;

            // 스텝 시작
            yield return StartCoroutine(PlayStep(step));

            currentStepIndex++;
        }

        // 시퀀스 완료
        OnSequenceComplete();
    }

    private IEnumerator PlayStep(TutorialStepData stepData)
    {
        currentStepData = stepData;

        // 1. 타겟 대기
        float elapsed = 0f;
        while (!registeredTargets.ContainsKey(stepData.targetID) && elapsed < targetWaitTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!registeredTargets.TryGetValue(stepData.targetID, out currentTarget))
        {
            DebugUtil.LogError($"[Tutorial] Target not found: {stepData.targetID}");
            yield break;
        }

        if (stepData.waitForEvent != GuideWaitType.None)
        {
            yield return new WaitForSeconds(0.2f);
            yield return new WaitUntil(() => CheckEvent(stepData.waitForEvent, stepData.waitEventName));
        }


        // 2. 팝업 애니메이션 대기
        yield return new WaitForEndOfFrame();

        if(stepData.appearDelay > 0)
        {
            yield return new WaitForSeconds(stepData.appearDelay);
        }

        // 3. 스포트라이트 표시 (stepData 전달)
        dimController.ShowSpotlight(currentTarget.HighlightArea, stepData);

        // 4. 메시지 표시 (TODO: 메시지 시스템 구현)
        // ShowMessage(stepData.messageText, stepData.messagePosition);

        isTrackingTarget = true;
        EnableOnlyTarget(currentTarget.gameObject);

        // 5. 진행 대기
        if (stepData.autoProgressDelay > 0)
        {
            yield return new WaitForSeconds(stepData.autoProgressDelay);
        }
        else
        {
            // 클릭 대기
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }


        if(stepData.endEvent != GuideEndEvent.None)
        {
            CheckEvent(stepData.endEvent, stepData.endEventName);
        }

        // 6. 스텝 완료
        CompleteStep();
    }

    private void Update()
    {
        // 타겟이 움직이는 경우 (스크롤뷰 내부 등)
        if (isTrackingTarget && currentTarget != null && currentStepData != null)
        {
            dimController.UpdateSpotlight(currentTarget.HighlightArea, currentStepData);
        }
    }

    /// <summary>
    /// 튜토리얼 스텝 종료
    /// </summary>
    public void CompleteStep()
    {
        isTrackingTarget = false;
        currentTarget = null;
        currentStepData = null;
        dimController.HideSpotlight();
        RestoreAllInteractions();
    }

    private void OnSequenceComplete()
    {
        DebugUtil.Log($"튜토리얼 '{currentSequence.sequenceName}' 완료");
        currentSequence = null;
        IsPlaying= false;
    }

    private bool CheckEvent(GuideWaitType waitType, string waitEventName)
    {
        switch (waitType)
        {
            case GuideWaitType.PopupOn:
                return Handlers.UI.GetPopup(waitEventName).IsActive;
            case GuideWaitType.PopupOff:
                return !Handlers.UI.GetPopup(waitEventName).IsActive;
        }

        return true;
    }

    private void CheckEvent(GuideEndEvent endEvent, string endEventName)
    {
        switch (endEvent)
        {
            case GuideEndEvent.PopupOn:
                Handlers.UI.GetPopup(endEventName).Show();
                break;
        }
    }

    // ===== 타겟 등록 시스템 =====
    public void RegisterTarget(string id, TutorialTarget target)
    {
        registeredTargets[id] = target;
    }

    public void UnregisterTarget(string id)
    {
        registeredTargets.Remove(id);
    }

    // ===== 인터랙션 제어 =====
    private List<Graphic> disabledGraphics = new List<Graphic>();

    private void EnableOnlyTarget(GameObject target)
    {
        Graphic[] allGraphics = FindObjectsOfType<Graphic>(true);

        foreach (var graphic in allGraphics)
        {
            if (graphic.raycastTarget && !IsChildOf(graphic.transform, target.transform))
            {
                graphic.raycastTarget = false;
                disabledGraphics.Add(graphic);
            }
        }
    }

    private void RestoreAllInteractions()
    {
        foreach (var graphic in disabledGraphics)
        {
            if (graphic != null)
                graphic.raycastTarget = true;
        }
        disabledGraphics.Clear();
    }

    private bool IsChildOf(Transform child, Transform parent)
    {
        while (child != null)
        {
            if (child == parent) return true;
            child = child.parent;
        }
        return false;
    }
}