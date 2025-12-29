using System;
using System.Threading;
using UnityEngine;

namespace Library
{
    public abstract class BaseScene : MonoBehaviour
    {
        public CancellationTokenSource cts = new();
        
        private void Awake()
        {
            Handlers.Scene.RegisterScene(this);
        }

        private void Start()
        {
            OnSceneLoaded();
        }

        private void OnDestroy()
        {
            OnSceneDestroy();
        }
        
        protected abstract void OnSceneLoaded();

        protected virtual void OnSceneDestroy()
        {
            cts.Cancel();
        }
    }
}