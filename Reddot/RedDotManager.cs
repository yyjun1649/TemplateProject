using System;
using System.Collections.Generic;
using System.Linq;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.Pool;

namespace Library
{
    /// <summary>
    /// 레드닷 시스템 중앙 관리자 (트리 구조)
    /// - 경로 기반으로 레드닷 관리 (예: "MainTab/StageReward/1/2")
    /// - 부모 경로는 자식 경로들의 OR 조합으로 자동 계산
    /// - 동적으로 경로 생성 가능
    /// </summary>
    public class RedDotManager : Singleton<RedDotManager>
    {
        [Tooltip("조건 관리")]
        private readonly Dictionary<string, Func<bool>> _conditions = new();

        [Tooltip("리 구조 (경로 → 자식 경로들)")]
        private readonly Dictionary<string, HashSet<string>> _children = new();
        
        public readonly Relay<string> OnRedDotChanged = new();
        
        public readonly Relay OnAllRedDotsChanged = new();
        
        private readonly Dictionary<string, bool> _cache = new();
        
        public void RegisterCondition(string path, Func<bool> condition)
        {
            _conditions[path] = condition;
            
            RegisterPathInTree(path);

            InvalidateCache(path);
        }
        
        public void UnregisterCondition(string path)
        {
            _conditions.Remove(path);
            InvalidateCache(path);
        }
        
        public bool HasRedDot(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (_cache.TryGetValue(path, out bool cached))
            {
                return cached;
            }
            
            bool result = CheckCondition(path);
            
            _cache[path] = result;

            return result;
        }
        
        /// <summary>
        /// 특정 경로의 모든 자식 레드닷 개수 반환
        /// </summary>
        public int GetRedDotCount(string path)
        {
            if (!_children.TryGetValue(path, out var children))
            {
                return HasRedDot(path) ? 1 : 0;
            }

            int count = 0;
            foreach (var child in children)
            {
                if (HasRedDot(child))
                {
                    count++;
                }
            }

            return count;
        }
        
        public void Refresh(string path)
        {
            InvalidateCache(path);
            OnRedDotChanged.Dispatch(path);
            
            RefreshParents(path);
        }

        /// <summary>
        /// 여러 경로 갱신
        /// </summary>
        public void Refresh(params string[] paths)
        {
            foreach (var path in paths)
            {
                InvalidateCache(path);
                OnRedDotChanged.Dispatch(path);
            }
            
            using (HashSetPool<string>.Get(out var parents))
            {
                foreach (var path in paths)
                {
                    CollectParents(path, parents);
                }

                foreach (var parent in parents)
                {
                    InvalidateCache(parent);
                    OnRedDotChanged.Dispatch(parent);
                }
            }
        }

        /// <summary>
        /// 모든 레드닷 상태 갱신
        /// </summary>
        public void RefreshAll()
        {
            _cache.Clear();
            OnAllRedDotsChanged.Dispatch();

            // 모든 경로에 대해 개별 이벤트도 발생
            foreach (var path in _conditions.Keys)
            {
                OnRedDotChanged.Dispatch(path);
            }
        }

        private bool CheckCondition(string path)
        {
            if (_conditions.TryGetValue(path, out var condition))
            {
                try
                {
                    return condition.Invoke();
                }
                catch (Exception e)
                {
                    DebugUtil.LogError($"[RedDotManager] Error checking condition for {path}: {e.Message}");
                    return false;
                }
            }
            
            if (_children.TryGetValue(path, out var children) && children.Count > 0)
            {
                foreach (var child in children)
                {
                    if (HasRedDot(child))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void RegisterPathInTree(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // 모든 부모 경로에 이 경로를 자식으로 등록
            string currentPath = path;
            while (true)
            {
                string parentPath = RedDotPath.GetParentPath(currentPath);
                
                if (string.IsNullOrEmpty(parentPath))
                {
                    break;
                }

                // 부모의 자식 목록에 추가
                if (!_children.ContainsKey(parentPath))
                {
                    _children[parentPath] = new HashSet<string>();
                }

                _children[parentPath].Add(currentPath);

                currentPath = parentPath;
            }
        }

        private void RefreshParents(string childPath)
        {
            string currentPath = childPath;
            while (true)
            {
                string parentPath = RedDotPath.GetParentPath(currentPath);
                if (string.IsNullOrEmpty(parentPath))
                {
                    break;
                }

                InvalidateCache(parentPath);
                OnRedDotChanged.Dispatch(parentPath);

                currentPath = parentPath;
            }
        }

        private void CollectParents(string path, HashSet<string> parents)
        {
            string currentPath = path;
            while (true)
            {
                string parentPath = RedDotPath.GetParentPath(currentPath);
                if (string.IsNullOrEmpty(parentPath))
                {
                    break;
                }

                parents.Add(parentPath);
                currentPath = parentPath;
            }
        }

        private void InvalidateCache(string path)
        {
            if (_cache.ContainsKey(path))
            {
                _cache.Remove(path);
            }
        }
        
        /// <summary>
        /// 모든 레드닷 상태 로그 출력
        /// </summary>
        public void LogAllRedDots()
        {
            DebugUtil.Log("=== RedDot Status ===");
            foreach (var path in _conditions.Keys.OrderBy(p => p))
            {
                bool has = HasRedDot(path);
                DebugUtil.Log($"{path}: {(has ? "ON" : "OFF")}");
            }
        }

        /// <summary>
        /// 트리 구조 로그 출력
        /// </summary>
        public void LogTree(string rootPath = null)
        {
            DebugUtil.Log("=== RedDot Tree ===");

            if (string.IsNullOrEmpty(rootPath))
            {
                // 루트 경로들 찾기 (부모가 없는 경로)

                using (HashSetPool<string>.Get(out var allPaths))
                {
                    foreach (var key in _conditions.Keys)
                    {
                        allPaths.Add(key);
                    }
                    
                    foreach (var children in _children.Values)
                    {
                        foreach (var child in children)
                        {
                            allPaths.Add(child);
                        }
                    }

                    var rootPaths = allPaths.Where(p => string.IsNullOrEmpty(RedDotPath.GetParentPath(p)));
                    
                    foreach (var root in rootPaths.OrderBy(p => p))
                    {
                        LogTreeRecursive(root, 0);
                    }
                }
            }
            else
            {
                LogTreeRecursive(rootPath, 0);
            }
        }

        private void LogTreeRecursive(string path, int depth)
        {
            string indent = new string(' ', depth * 2);
            bool has = HasRedDot(path);
            string status = has ? "[ON]" : "[OFF]";
            DebugUtil.Log($"{indent}{status} {path}");

            if (_children.TryGetValue(path, out var children))
            {
                foreach (var child in children.OrderBy(c => c))
                {
                    LogTreeRecursive(child, depth + 1);
                }
            }
        }
    }
}
