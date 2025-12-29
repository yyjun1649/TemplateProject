using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Library
{
    public class Handlers : SingletonBehaviour<Handlers>
    {
        public static ResourceHandler Resource => Instance._resourceHandler;
        public static SoundHandler Sound => Instance._soundHandler;
        public static UIHandler UI => Instance._uiHandler;
        public static EventHandler Event => Instance._eventHandler;

        public static SceneHandler Scene => Instance._sceneHandler;
        public static TimeHandler Time => Instance._timeHandler;

        private ResourceHandler _resourceHandler;
        private SoundHandler _soundHandler;
        private UIHandler _uiHandler;
        private EventHandler _eventHandler;
        private SceneHandler _sceneHandler;
        private TimeHandler _timeHandler;

        private bool _isInitialized = false;

        public async UniTask Initialize()
        {
            if (_isInitialized)
            {
                DebugUtil.LogWarning("Handlers가 이미 초기화되었습니다.");
                return;
            }

            try
            {
                GetHandlers();
                SetDefaultSetting();

                //await UserDataManager.Instance.Initialize();
                
                await _resourceHandler.InitializeAddressable();
                    
                _timeHandler.StartTimeCoroutine();

                _isInitialized = true;
                
                DebugUtil.Log("Handlers 초기화 완료");
            }
            catch (Exception e)
            {
                Scene.ChangeScene("Splash");
                
                throw;
            }
        }

        private void GetHandlers()
        {
            _resourceHandler = GetComponentInChildren<ResourceHandler>();
            _soundHandler = GetComponentInChildren<SoundHandler>();
            _uiHandler = GetComponentInChildren<UIHandler>();
            _eventHandler = GetComponentInChildren<EventHandler>();
            _sceneHandler = GetComponentInChildren<SceneHandler>();
            _timeHandler = GetComponentInChildren<TimeHandler>();
            
            DebugUtil.Log("[App Initialize] Get Handlers");
        }

        private void SetDefaultSetting()
        {
            Screen.fullScreen = true; //풀스크린
            QualitySettings.vSyncCount = 0; //VSync 비활성화
            Application.targetFrameRate = 60; //프레임레이드 60
            Screen.sleepTimeout = SleepTimeout.NeverSleep; //슬립 없도록
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled; //가비지 콜렉팅 활성화
            
#if UNITY_EDITOR
            Application.runInBackground = true;
#else
        Application.runInBackground = false;
#endif
            
            DebugUtil.Log("[App Initialize] App Default Setting");
        }

        public void Shutdown()
        {
            if (!_isInitialized)
            {
                return;
            }

            DebugUtil.Log("Handlers Shutdown 시작");

            _timeHandler?.OnShutdown();
            _sceneHandler?.OnShutdown();
            _eventHandler?.OnShutdown();
            _uiHandler?.OnShutdown();
            _soundHandler?.OnShutdown();
            _resourceHandler?.OnShutdown();

            _isInitialized = false;

            DebugUtil.Log("Handlers Shutdown 완료");
        }

        protected override void OnDestroy()
        {
            Shutdown();

            base.OnDestroy();
        }
    }
}