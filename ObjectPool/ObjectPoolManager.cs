using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Library
{
    public class ObjectPoolManager : SingletonBehaviour<ObjectPoolManager>
    {
        private Dictionary<Type, IObjectPool> _objectPools = new Dictionary<Type, IObjectPool>();
        private Transform _poolRoot;

        [Header("Global Pool Settings")]
        [SerializeField] private int _defaultMaxPoolSize = 100;
        [SerializeField] private int _defaultPrewarmCount = 5;
        [SerializeField] private float _globalCleanupInterval = 300f; // 5분

        private bool _autoCleanupEnabled = true;
        private Coroutine _cleanupCoroutine;

        protected override void Awake()
        {
            base.Awake();
            _poolRoot = new GameObject("ObjectPoolRoot").transform;
            _poolRoot.SetParent(transform);
        }

        private void Start()
        {
            if (_autoCleanupEnabled)
            {
                StartAutoCleanup();
            }
        }

        private void OnDestroy()
        {
            StopAutoCleanup();
        }

        public static T Get<T>(int id) where T : PoolMonoBehaviour<T>
        {
            return GetObject<T>().Get(id);
        }

        public static void Release<T>(int id, T obj) where T : PoolMonoBehaviour<T>
        {
            GetObject<T>().Release(id, obj);
        }

        public static ObjectPoolBase<T> GetObject<T>() where T : PoolMonoBehaviour<T>
        {
            Type type = typeof(T);

            if (Instance._objectPools.TryGetValue(type, out var objectPool))
            {
                return (ObjectPoolBase<T>)objectPool;
            }

            var newObjectPool = new ObjectPoolBase<T>(type.Name, Instance._poolRoot)
            {
                MaxPoolSize = Instance._defaultMaxPoolSize,
                PrewarmCount = Instance._defaultPrewarmCount
            };
            Instance._objectPools.Add(type, newObjectPool);
            return newObjectPool;
        }

        /// <summary>
        /// 특정 타입의 오브젝트를 미리 생성
        /// </summary>
        public static void Prewarm<T>(int id, int count = -1) where T : PoolMonoBehaviour<T>
        {
            GetObject<T>().Prewarm(id, count);
        }

        /// <summary>
        /// 모든 풀 정리
        /// </summary>
        public static void TrimAllPools()
        {
            foreach (var pool in Instance._objectPools.Values)
            {
                pool.TrimAllPools();
            }
        }

        /// <summary>
        /// 특정 타입의 모든 활성 오브젝트 반환
        /// </summary>
        public static void ReleaseAll<T>() where T : PoolMonoBehaviour<T>
        {
            if (Instance._objectPools.TryGetValue(typeof(T), out var pool))
            {
                pool.ReleaseAll();
            }
        }

        /// <summary>
        /// 모든 타입의 활성 오브젝트 반환
        /// </summary>
        public static void ReleaseAllPools()
        {
            foreach (var pool in Instance._objectPools.Values)
            {
                pool.ReleaseAll();
            }
        }

        /// <summary>
        /// 특정 타입의 풀 완전 삭제
        /// </summary>
        public static void DestroyPool<T>() where T : PoolMonoBehaviour<T>
        {
            Type type = typeof(T);
            if (Instance._objectPools.TryGetValue(type, out var pool))
            {
                var poolBase = (ObjectPoolBase<T>)pool;
                if (poolBase != null && poolBase.Root != null)
                {
                    Destroy(poolBase.Root.gameObject);
                }
                Instance._objectPools.Remove(type);
            }
        }

        /// <summary>
        /// 모든 풀 통계 정보 가져오기
        /// </summary>
        public static Dictionary<string, List<PoolStatistics>> GetAllPoolStatistics()
        {
            var allStats = new Dictionary<string, List<PoolStatistics>>();

            foreach (var kvp in Instance._objectPools)
            {
                var typeName = kvp.Key.Name;
                var pool = kvp.Value;

                var stats = pool.GetAllStatistics();
                var statsList = new List<PoolStatistics>(stats.Count);
                foreach (var stat in stats.Values)
                {
                    statsList.Add(stat);
                }
                allStats[typeName] = statsList;
            }

            return allStats;
        }

        /// <summary>
        /// 통계 정보를 로그로 출력
        /// </summary>
        public static void LogPoolStatistics()
        {
            var allStats = GetAllPoolStatistics();
            var totalActive = 0;
            var totalPooled = 0;

            DebugUtil.Log("=== Object Pool Statistics ===");

            foreach (var kvp in allStats)
            {
                DebugUtil.Log($"[{kvp.Key}]");
                foreach (var stat in kvp.Value)
                {
                    DebugUtil.Log($"  - {stat.PoolName}: Active={stat.ActiveCount}, Pooled={stat.PooledCount}, Total={stat.TotalCount}");
                    totalActive += stat.ActiveCount;
                    totalPooled += stat.PooledCount;
                }
            }

            DebugUtil.Log($"=== Total: Active={totalActive}, Pooled={totalPooled}, Total={totalActive + totalPooled} ===");
        }

        /// <summary>
        /// 자동 정리 시작
        /// </summary>
        public void StartAutoCleanup()
        {
            if (_cleanupCoroutine != null)
            {
                StopCoroutine(_cleanupCoroutine);
            }
            _cleanupCoroutine = StartCoroutine(AutoCleanupRoutine());
        }

        /// <summary>
        /// 자동 정리 중지
        /// </summary>
        public void StopAutoCleanup()
        {
            if (_cleanupCoroutine != null)
            {
                StopCoroutine(_cleanupCoroutine);
                _cleanupCoroutine = null;
            }
        }

        /// <summary>
        /// 자동 정리 코루틴
        /// </summary>
        private IEnumerator AutoCleanupRoutine()
        {
            var wait = new WaitForSeconds(_globalCleanupInterval);

            while (true)
            {
                yield return wait;
                TrimAllPools();
                DebugUtil.Log($"[ObjectPoolManager] Auto cleanup executed at {System.DateTime.Now:HH:mm:ss}");
            }
        }

        /// <summary>
        /// 메모리 압박 시 강제 정리
        /// </summary>
        public static void ForceCleanup()
        {
            ReleaseAllPools();
            TrimAllPools();
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            DebugUtil.Log("[ObjectPoolManager] Force cleanup executed");
        }
    }
}