using System;

namespace Library
{
    using UnityEngine;

    public abstract class Singleton<T> : IDisposable where T : new()
    {
        private static readonly object Lock = new();

        protected static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                lock (Lock)
                {
                    _instance ??= new T();
                }

                return _instance;
            }
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        
        static Singleton()
        {
        }
    }
}
