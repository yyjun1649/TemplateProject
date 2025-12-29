using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DebugUtil = Library.DebugUtil;

public class TutorialDimController : MonoBehaviour
{
    [SerializeField] private Material dimMaterial;
    [SerializeField] private RawImage dimImage;
    [SerializeField] private Canvas tutorialCanvas;

    [SerializeField] private RectTransform arrowRectS;

    [SerializeField] private GameObject _goDesc;
    [SerializeField] private TextMeshProUGUI _textDesc;

    private RectTransform canvasRect;
    private Material materialInstance;

    private Vector2 baseHoleSize; // 펄스 애니메이션을 위한 기본 크기
    private bool isPulseRunning = false;

    // 화살표 펄스 애니메이션을 위한 정보 저장
    private Vector2 baseArrowScreenCenter;
    private Vector2 baseArrowScreenSize;
    private MessagePosition currentArrowPosition;

    private void Awake()
    {
        canvasRect = tutorialCanvas.GetComponent<RectTransform>();

        // Material 인스턴스 생성
        materialInstance = new Material(dimMaterial);
        dimImage.material = materialInstance;

        // 초기에는 비활성화
        dimImage.gameObject.SetActive(false);
        arrowRectS.gameObject.SetActive(false);
        _goDesc.SetActive(false);
    }

    /// <summary>
    /// 스포트라이트 활성화 (stepData 기반)
    /// </summary>
    public void ShowSpotlight(RectTransform target, TutorialStepData stepData)
    {
        if (target == null || stepData == null) return;

        // 셰이더 파라미터 업데이트
        UpdateShaderParameters(target, stepData);

        arrowRectS.gameObject.SetActive(true);

        if (stepData.messageText.Length > 0)
        {
            // messagePosition 검증
            if (stepData.messagePosition == MessagePosition.Right || stepData.messagePosition == MessagePosition.Left)
            {
                DebugUtil.LogWarning($"[Tutorial] messagePosition은 Up/Down만 지원합니다. 현재 값: {stepData.messagePosition}");
                _goDesc.SetActive(false);
            }
            else
            {
                _textDesc.text = stepData.messageText;
                _goDesc.SetActive(true);
            }
        }
        else
        {
            _goDesc.SetActive(false);
        }

        // 9. 페이드인 애니메이션 (stepData 기반)
        if (stepData.fadeInDuration > 0)
        {
            StartCoroutine(FadeInSpotlight(stepData.fadeInDuration));
        }
        else
        {
            dimImage.gameObject.SetActive(true);
        }

        // 10. 펄스 애니메이션 (한 번만 시작)
        if (stepData.enablePulse && !isPulseRunning)
        {
            isPulseRunning = true;
            StartCoroutine(PulseAnimation());
        }
    }

    /// <summary>
    /// 셰이더 파라미터만 업데이트 (애니메이션 없이)
    /// </summary>
    private void UpdateShaderParameters(RectTransform target, TutorialStepData stepData)
    {
        if (target == null || stepData == null) return;

        // 타겟 Canvas의 카메라 가져오기 (Overlay면 null)
        Canvas targetCanvas = target.GetComponentInParent<Canvas>();
        Camera targetCamera = targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? targetCanvas.worldCamera
            : null;

        // 1. 타겟의 월드 좌표 → 스크린 좌표 변환
        Vector3[] worldCorners = new Vector3[4];
        target.GetWorldCorners(worldCorners);

        // 중심점 계산
        Vector3 worldCenter = (worldCorners[0] + worldCorners[2]) * 0.5f;
        Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(targetCamera, worldCenter);

        // 2. 스포트라이트 오프셋 적용 (픽셀 단위)
        Vector2 offsetScreenCenter = screenCenter + stepData.spotlightOffset;

        // 3. 스크린 좌표를 UV 좌표로 직접 변환 (0~1 범위, offset 적용됨)
        Vector2 uvCenter = new Vector2(
            offsetScreenCenter.x / Screen.width,
            offsetScreenCenter.y / Screen.height
        );

        // 4. 타겟 크기 계산 (스크린 좌표)
        Vector2 screenSize = new Vector2(
            Vector2.Distance(
                RectTransformUtility.WorldToScreenPoint(targetCamera, worldCorners[0]),
                RectTransformUtility.WorldToScreenPoint(targetCamera, worldCorners[3])
            ),
            Vector2.Distance(
                RectTransformUtility.WorldToScreenPoint(targetCamera, worldCorners[0]),
                RectTransformUtility.WorldToScreenPoint(targetCamera, worldCorners[1])
            )
        );

        // 5. stepData의 padding 적용 (픽셀 단위)
        screenSize += stepData.spotlightPadding * 2f;

        // 6. UV 좌표계로 크기 변환
        Vector2 uvSize = new Vector2(
            screenSize.x / Screen.width,
            screenSize.y / Screen.height
        );

        // 7. Shader에 전달
        materialInstance.SetVector("_HoleCenter", uvCenter);

        // 펄스 애니메이션이 실행 중이 아닐 때만 크기 업데이트
        if (!isPulseRunning)
        {
            materialInstance.SetVector("_HoleSize", uvSize);
        }
        else
        {
            // 펄스 애니메이션 중에는 baseHoleSize만 업데이트
            baseHoleSize = uvSize;
        }

        materialInstance.SetFloat("_CornerRadius", stepData.cornerRadius);

        // 화면 해상도를 셰이더에 전달 (픽셀 단위 계산용)
        materialInstance.SetVector("_ScreenResolution", new Vector4(Screen.width, Screen.height, 0, 0));

        // 8. 화살표 위치 및 회전 설정 (offset 적용됨)
        // 펄스 애니메이션을 위해 기본 값 항상 업데이트 (타겟이 이동하는 경우 대응)
        baseArrowScreenCenter = offsetScreenCenter;
        baseArrowScreenSize = screenSize;
        currentArrowPosition = stepData.arrowPosition;

        // 펄스 애니메이션이 실행 중이 아닐 때만 화살표 위치 직접 업데이트
        if (!isPulseRunning)
        {
            UpdateArrowTransform(offsetScreenCenter, screenSize, stepData.arrowPosition, 1f);
        }
        // 펄스 애니메이션 중에는 PulseAnimation 코루틴에서 처리

        // 9. 말풍선 위치 설정 (offset 적용 안됨, 원래 타겟 위치 기준)
        UpdateDescriptionTransform(screenCenter, screenSize, stepData.messagePosition);
    }

    /// <summary>
    /// 화살표 위치 및 회전 업데이트
    /// </summary>
    /// <param name="spotlightCenter">스포트라이트 중심 (스크린 좌표)</param>
    /// <param name="spotlightSize">스포트라이트 크기 (스크린 좌표)</param>
    /// <param name="arrowPosition">화살표 위치</param>
    /// <param name="pulseScale">펄스 애니메이션 스케일 (1.0 = 기본)</param>
    private void UpdateArrowTransform(Vector2 spotlightCenter, Vector2 spotlightSize, MessagePosition arrowPosition, float pulseScale)
    {
        if (arrowRectS == null) return;

        // 화살표가 spotlight 바깥쪽에 떨어진 거리 (픽셀)
        float arrowOffset = 20f;

        // 펄스 효과를 받는 크기
        Vector2 scaledSize = spotlightSize * pulseScale;

        // arrowPosition에 따라 위치와 회전 계산
        // 화살표의 pivot은 (1, 0.5) = 오른쪽 끝, 세로 중앙
        Vector2 arrowScreenPos = spotlightCenter;
        float rotation = 0f;

        switch (arrowPosition)
        {
            case MessagePosition.Right:
                // 오른쪽을 가리킴: 화살표는 왼쪽에 위치, pivot(오른쪽 끝)이 spotlight 왼쪽 가장자리 - offset
                arrowScreenPos.x -= scaledSize.x / 2f + arrowOffset;
                rotation = 0f; // 오른쪽을 바라봄 (기본)
                break;

            case MessagePosition.Left:
                // 왼쪽을 가리킴: 화살표는 오른쪽에 위치, pivot이 spotlight 오른쪽 가장자리 + offset
                arrowScreenPos.x += scaledSize.x / 2f + arrowOffset;
                rotation = 180f; // 왼쪽을 바라봄
                break;

            case MessagePosition.Top:
                // 위를 가리킴: 화살표는 아래에 위치, pivot이 spotlight 아래 가장자리 - offset
                arrowScreenPos.y -= scaledSize.y / 2f + arrowOffset;
                rotation = 90f; // 위를 바라봄
                break;

            case MessagePosition.Bottom:
                // 아래를 가리킴: 화살표는 위에 위치, pivot이 spotlight 위 가장자리 + offset
                arrowScreenPos.y += scaledSize.y / 2f + arrowOffset;
                rotation = -90f; // 아래를 바라봄
                break;

            case MessagePosition.Custom:
                // Custom은 별도 처리 없음
                return;
        }

        // 스크린 좌표를 Canvas 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            arrowScreenPos,
            null, // Overlay Canvas는 null
            out Vector2 localPos
        );

        // 화살표 위치 설정
        arrowRectS.anchoredPosition = localPos;

        // 화살표 회전 설정
        arrowRectS.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }

    /// <summary>
    /// 말풍선 위치 업데이트
    /// </summary>
    /// <param name="spotlightCenter">스포트라이트 중심 (스크린 좌표)</param>
    /// <param name="spotlightSize">스포트라이트 크기 (스크린 좌표)</param>
    /// <param name="messagePosition">메시지 위치</param>
    private void UpdateDescriptionTransform(Vector2 spotlightCenter, Vector2 spotlightSize, MessagePosition messagePosition)
    {
        if (_goDesc == null || !_goDesc.activeSelf) return;

        // 말풍선이 타겟으로부터 떨어진 거리 (픽셀)
        float descOffset = 500f;

        // 말풍선 위치: x는 항상 화면 중앙, y는 messagePosition에 따라
        Vector2 descScreenPos = new Vector2(Screen.width / 2f, spotlightCenter.y);

        switch (messagePosition)
        {
            case MessagePosition.Top:
                // 위: 타겟 위쪽에 100픽셀 떨어져서
                descScreenPos.y += spotlightSize.y / 2f + descOffset;
                break;

            case MessagePosition.Bottom:
                // 아래: 타겟 아래쪽에 100픽셀 떨어져서
                descScreenPos.y -= spotlightSize.y / 2f + descOffset;
                break;

            case MessagePosition.Right:
            case MessagePosition.Left:
            case MessagePosition.Custom:
                // 지원하지 않음 (경고는 이미 ShowSpotlight에서 처리)
                return;
        }

        // 스크린 좌표를 Canvas 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            descScreenPos,
            null, // Overlay Canvas는 null
            out Vector2 localPos
        );

        // 말풍선 위치 설정
        RectTransform descRect = _goDesc.GetComponent<RectTransform>();
        if (descRect != null)
        {
            descRect.anchoredPosition = localPos;
        }
    }

    /// <summary>
    /// 스포트라이트 비활성화
    /// </summary>
    public void HideSpotlight()
    {
        StopAllCoroutines();
        isPulseRunning = false;
        dimImage.gameObject.SetActive(false);
        _goDesc.SetActive(false);
        arrowRectS.gameObject.SetActive(false);
    }

    /// <summary>
    /// 타겟이 이동하는 경우 (Update에서 호출)
    /// </summary>
    public void UpdateSpotlight(RectTransform target, TutorialStepData stepData)
    {
        // 애니메이션 시작 없이 파라미터만 업데이트
        UpdateShaderParameters(target, stepData);
    }

    // ===== 애니메이션 =====

    private System.Collections.IEnumerator FadeInSpotlight(float duration)
    {
        dimImage.gameObject.SetActive(true);

        float elapsed = 0f;
        Color dimColor = materialInstance.GetColor("_DimColor");
        float targetAlpha = dimColor.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, targetAlpha, elapsed / duration);
            dimColor.a = alpha;
            materialInstance.SetColor("_DimColor", dimColor);
            yield return null;
        }

        dimColor.a = targetAlpha;
        materialInstance.SetColor("_DimColor", dimColor);
    }

    private System.Collections.IEnumerator PulseAnimation()
    {
        // 초기 크기 저장
        baseHoleSize = materialInstance.GetVector("_HoleSize");
        float pulseAmount = 1.05f; // 5% 확대
        float pulseDuration = 0.8f;

        while (true)
        {
            float elapsed = 0f;

            // 확대
            while (elapsed < pulseDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(1f, pulseAmount, elapsed / (pulseDuration / 2f));
                materialInstance.SetVector("_HoleSize", baseHoleSize * scale);

                // 화살표도 함께 펄스
                if (arrowRectS != null && arrowRectS.gameObject.activeSelf)
                {
                    UpdateArrowTransform(baseArrowScreenCenter, baseArrowScreenSize, currentArrowPosition, scale);
                }

                yield return null;
            }

            // 축소
            elapsed = 0f;
            while (elapsed < pulseDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(pulseAmount, 1f, elapsed / (pulseDuration / 2f));
                materialInstance.SetVector("_HoleSize", baseHoleSize * scale);

                // 화살표도 함께 펄스
                if (arrowRectS != null && arrowRectS.gameObject.activeSelf)
                {
                    UpdateArrowTransform(baseArrowScreenCenter, baseArrowScreenSize, currentArrowPosition, scale);
                }

                yield return null;
            }
        }
    }

    private void OnDestroy()
    {
        // Material 인스턴스 정리
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}