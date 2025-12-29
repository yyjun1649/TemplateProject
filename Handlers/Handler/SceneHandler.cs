using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Library
{
    public class SceneHandler : BaseHandler
    {
        public BaseScene CurrentScene { get; private set; }

        private bool _isLoading = false;
        
        public async void ChangeScene(string sceneName)
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;
            
            await SceneManager.LoadSceneAsync(sceneName);

            _isLoading = false;
        }

        public void RegisterScene(BaseScene scene)
        {
            CurrentScene = scene;
        }

        public override void OnShutdown()
        {
            // 현재 씬 참조 제거
            CurrentScene = null;
            _isLoading = false;
        }
    }
}