using UnityEngine;
using System;
using System.Diagnostics;

namespace Library
{
    /// <summary>
    /// Unity Editor에서만 로그를 출력하는 디버그 유틸리티 클래스
    /// 빌드된 게임에서는 모든 로그 호출이 완전히 제거됩니다.
    /// </summary>
    public static class DebugUtil
    {
        /// <summary>
        /// Unity Editor에서만 일반 로그를 출력합니다.
        /// </summary>
        /// <param name="message">출력할 메시지</param>
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        /// <summary>
        /// Unity Editor에서만 일반 로그를 출력합니다. (컨텍스트 포함)
        /// </summary>
        /// <param name="message">출력할 메시지</param>
        /// <param name="context">로그와 연결된 오브젝트 (클릭 시 해당 오브젝트로 이동)</param>
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.Log(message, context);
        }

        /// <summary>
        /// Unity Editor에서만 포맷된 로그를 출력합니다.
        /// </summary>
        /// <param name="format">포맷 문자열</param>
        /// <param name="args">포맷 인자</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }

        /// <summary>
        /// Unity Editor에서만 포맷된 로그를 출력합니다. (컨텍스트 포함)
        /// </summary>
        /// <param name="context">로그와 연결된 오브젝트</param>
        /// <param name="format">포맷 문자열</param>
        /// <param name="args">포맷 인자</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(context, format, args);
        }

        /// <summary>
        /// Unity Editor에서만 경고 로그를 출력합니다.
        /// </summary>
        /// <param name="message">출력할 경고 메시지</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        /// <summary>
        /// Unity Editor에서만 경고 로그를 출력합니다. (컨텍스트 포함)
        /// </summary>
        /// <param name="message">출력할 경고 메시지</param>
        /// <param name="context">로그와 연결된 오브젝트</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }

        /// <summary>
        /// Unity Editor에서만 포맷된 경고 로그를 출력합니다.
        /// </summary>
        /// <param name="format">포맷 문자열</param>
        /// <param name="args">포맷 인자</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogWarningFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(format, args);
        }

        /// <summary>
        /// Unity Editor에서만 포맷된 경고 로그를 출력합니다. (컨텍스트 포함)
        /// </summary>
        /// <param name="context">로그와 연결된 오브젝트</param>
        /// <param name="format">포맷 문자열</param>
        /// <param name="args">포맷 인자</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(context, format, args);
        }

        /// <summary>
        /// Unity Editor에서만 에러 로그를 출력합니다.
        /// </summary>
        /// <param name="message">출력할 에러 메시지</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        /// <summary>
        /// Unity Editor에서만 에러 로그를 출력합니다. (컨텍스트 포함)
        /// </summary>
        /// <param name="message">출력할 에러 메시지</param>
        /// <param name="context">로그와 연결된 오브젝트</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogError(message, context);
        }

        /// <summary>
        /// Unity Editor에서만 포맷된 에러 로그를 출력합니다.
        /// </summary>
        /// <param name="format">포맷 문자열</param>
        /// <param name="args">포맷 인자</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogErrorFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(format, args);
        }

        /// <summary>
        /// Unity Editor에서만 포맷된 에러 로그를 출력합니다. (컨텍스트 포함)
        /// </summary>
        /// <param name="context">로그와 연결된 오브젝트</param>
        /// <param name="format">포맷 문자열</param>
        /// <param name="args">포맷 인자</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(context, format, args);
        }

        /// <summary>
        /// Unity Editor에서만 예외를 로그로 출력합니다.
        /// </summary>
        /// <param name="exception">출력할 예외</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogException(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        /// <summary>
        /// Unity Editor에서만 예외를 로그로 출력합니다. (컨텍스트 포함)
        /// </summary>
        /// <param name="exception">출력할 예외</param>
        /// <param name="context">로그와 연결된 오브젝트</param>
        [Conditional("UNITY_EDITOR")]
        public static void LogException(Exception exception, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogException(exception, context);
        }

        /// <summary>
        /// Unity Editor에서만 조건을 검증합니다. 조건이 false면 에러 로그를 출력합니다.
        /// </summary>
        /// <param name="condition">검증할 조건</param>
        [Conditional("UNITY_EDITOR")]
        public static void Assert(bool condition)
        {
            UnityEngine.Debug.Assert(condition);
        }

        /// <summary>
        /// Unity Editor에서만 조건을 검증합니다. 조건이 false면 메시지와 함께 에러 로그를 출력합니다.
        /// </summary>
        /// <param name="condition">검증할 조건</param>
        /// <param name="message">조건이 false일 때 출력할 메시지</param>
        [Conditional("UNITY_EDITOR")]
        public static void Assert(bool condition, string message)
        {
            UnityEngine.Debug.Assert(condition, message);
        }

        /// <summary>
        /// Unity Editor에서만 조건을 검증합니다. 조건이 false면 메시지와 함께 에러 로그를 출력합니다. (컨텍스트 포함)
        /// </summary>
        /// <param name="condition">검증할 조건</param>
        /// <param name="message">조건이 false일 때 출력할 메시지</param>
        /// <param name="context">로그와 연결된 오브젝트</param>
        [Conditional("UNITY_EDITOR")]
        public static void Assert(bool condition, object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.Assert(condition, message, context);
        }

        /// <summary>
        /// Unity Editor에서만 포맷된 조건 검증 메시지를 출력합니다.
        /// </summary>
        /// <param name="condition">검증할 조건</param>
        /// <param name="format">포맷 문자열</param>
        /// <param name="args">포맷 인자</param>
        [Conditional("UNITY_EDITOR")]
        public static void AssertFormat(bool condition, string format, params object[] args)
        {
            UnityEngine.Debug.AssertFormat(condition, format, args);
        }

        /// <summary>
        /// Unity Editor에서만 포맷된 조건 검증 메시지를 출력합니다. (컨텍스트 포함)
        /// </summary>
        /// <param name="condition">검증할 조건</param>
        /// <param name="context">로그와 연결된 오브젝트</param>
        /// <param name="format">포맷 문자열</param>
        /// <param name="args">포맷 인자</param>
        [Conditional("UNITY_EDITOR")]
        public static void AssertFormat(bool condition, UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.AssertFormat(condition, context, format, args);
        }

        /// <summary>
        /// Unity Editor에서만 라인을 그립니다. (Scene 뷰에서 표시)
        /// </summary>
        /// <param name="start">시작 위치</param>
        /// <param name="end">끝 위치</param>
        /// <param name="color">라인 색상</param>
        /// <param name="duration">표시 시간 (초)</param>
        /// <param name="depthTest">깊이 테스트 여부</param>
        [Conditional("UNITY_EDITOR")]
        public static void DrawLine(Vector3 start, Vector3 end, Color color = default, float duration = 0.0f, bool depthTest = true)
        {
            if (color == default) color = Color.white;
            UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
        }

        /// <summary>
        /// Unity Editor에서만 레이를 그립니다. (Scene 뷰에서 표시)
        /// </summary>
        /// <param name="start">시작 위치</param>
        /// <param name="dir">방향</param>
        /// <param name="color">레이 색상</param>
        /// <param name="duration">표시 시간 (초)</param>
        /// <param name="depthTest">깊이 테스트 여부</param>
        [Conditional("UNITY_EDITOR")]
        public static void DrawRay(Vector3 start, Vector3 dir, Color color = default, float duration = 0.0f, bool depthTest = true)
        {
            if (color == default) color = Color.white;
            UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
        }
    }
}
