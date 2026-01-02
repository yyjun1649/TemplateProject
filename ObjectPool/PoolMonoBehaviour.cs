using UnityEngine;

namespace Library
{
    public abstract class PoolMonoBehaviour<T> : MonoBehaviour where T : PoolMonoBehaviour<T>
    {
        public int poolObjectId;
        public void Release()
        {
            ObjectPoolManager.GetObject<T>().Release(poolObjectId,(T)this);
        }
        
        /// <summary>
        /// ObjectPool에 담길 때 초기화 정보
        /// </summary>
        public virtual void OnRelease()
        {
            gameObject.SetActive(false);
        }
    }
}