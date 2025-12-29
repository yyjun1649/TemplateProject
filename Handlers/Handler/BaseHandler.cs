using UnityEngine;

namespace Library
{
    public abstract class BaseHandler : MonoBehaviour
    {
        /// <summary>
        /// 핸들러 종료 시 호출되는 메서드
        /// 각 핸들러는 이 메서드를 오버라이드하여 정리 로직을 구현해야 함
        /// </summary>
        public virtual void OnShutdown()
        {
            // 기본 구현은 비어있음
            // 각 핸들러에서 필요에 따라 오버라이드
        }
    }
}
