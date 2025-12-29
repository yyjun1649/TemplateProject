using System;
using Library;
using TMPro;
using UnityEngine;

namespace Library
{
    public class RedDotUI : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private string dotPath;

        [SerializeField] private GameObject redDotObject;

        [SerializeField] private TextMeshProUGUI txtCount;

        [Header("옵션")]
        [Tooltip("자동 갱신 (OnEnable 시)")]
        [SerializeField] private bool autoRefreshOnEnable = true;

        [Tooltip("부모 경로도 감시 (부모 변경 시 갱신)")]
        [SerializeField] private bool listenToParent = false;

        [Tooltip("자식 경로도 감시 (자식 변경 시 갱신)")]
        [SerializeField] private bool listenToChildren = false;
        
        [Tooltip("디버그 로그 출력")]
        [SerializeField] private bool debugLog = false;
        
        private void Awake()
        {
            // redDotObject가 없으면 자신을 사용
            if (redDotObject == null)
                redDotObject = gameObject;
        }

        private void OnEnable()
        {
            RedDotManager.Instance.OnRedDotChanged.AddListener(OnRedDotChanged);
            RedDotManager.Instance.OnAllRedDotsChanged.AddListener(OnAllRedDotsChanged);
            
            if (autoRefreshOnEnable)
            {
                UpdateRedDot();
            }
        }

        private void OnDisable()
        {
            if (RedDotManager.Instance != null)
            {
                RedDotManager.Instance.OnRedDotChanged.RemoveListener(OnRedDotChanged);
                RedDotManager.Instance.OnAllRedDotsChanged.RemoveListener(OnAllRedDotsChanged);
            }
        }

        private void OnRedDotChanged(string path)
        {
            // 자신의 경로가 변경되었을 때
            if (path == dotPath)
            {
                UpdateRedDot();
                return;
            }

            // 부모 경로 감시 옵션이 켜져 있으면
            if (listenToParent && RedDotPath.IsChildOf(dotPath, path))
            {
                UpdateRedDot();
                return;
            }

            // 자식 경로 감시 옵션이 켜져 있으면
            if (listenToChildren && RedDotPath.IsChildOf(path, dotPath))
            {
                UpdateRedDot();
                return;
            }
        }

        private void OnAllRedDotsChanged()
        {
            UpdateRedDot();
        }
        
        public void UpdateRedDot()
        {
            if (redDotObject == null)
            {
                DebugUtil.LogWarning($"[RedDotUI] redDotObject is null on {gameObject.name}");
                return;
            }

            if (string.IsNullOrEmpty(dotPath))
            {
                DebugUtil.LogWarning($"[RedDotUI] dotPath is empty on {gameObject.name}");
                redDotObject.SetActive(false);
                return;
            }

            bool shouldShow = RedDotManager.Instance.HasRedDot(dotPath);

            redDotObject.SetActive(shouldShow);
        }
        
        public void SetDotPath(string newPath)
        {
            dotPath = newPath;
            UpdateRedDot();
        }
        

#if UNITY_EDITOR
        [ContextMenu("Force Update")]
        private void EditorForceUpdate()
        {
            if (Application.isPlaying)
                UpdateRedDot();
            else
                DebugUtil.Log("[RedDotUI] Force Update는 플레이 모드에서만 동작합니다.");
        }

        [ContextMenu("Log Status")]
        private void EditorLogStatus()
        {
            if (Application.isPlaying)
            {
                bool has = RedDotManager.Instance.HasRedDot(dotPath);
                int count = RedDotManager.Instance.GetRedDotCount(dotPath);
                DebugUtil.Log($"[RedDotUI] {gameObject.name} - Path: {dotPath}, HasRedDot: {has}, Count: {count}");
            }
            else
            {
                DebugUtil.Log($"[RedDotUI] {gameObject.name} - Path: {dotPath} (플레이 모드 아님)");
            }
        }

        [ContextMenu("Log Tree")]
        private void EditorLogTree()
        {
            if (Application.isPlaying)
            {
                RedDotManager.Instance.LogTree(dotPath);
            }
            else
            {
                DebugUtil.Log("[RedDotUI] Log Tree는 플레이 모드에서만 동작합니다.");
            }
        }
#endif
    }
}
