using Sigtrap.Relays;

namespace Library
{
    using System;
    using System.Collections;
    using UnityEngine;

    public sealed class TimeHandler : BaseHandler
    {
        #region Coroutine

        private Coroutine _dayOverCoroutine;
        private Coroutine _tickerCoroutine;
        private Coroutine _minuteCoroutine;
        private Coroutine _onTheHourCoroutine;

        private bool _isPaused;
        private bool _isActivate;

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                _isPaused = true;
                StopTimeManager();
            }
            else
            {
                if (_isPaused)
                {
                    _isPaused = false;
                    StartTimeCoroutine();
                }
            }
        }

        public void StartTimeCoroutine()
        {
            if (!_isActivate)
            {
                return;
            }

            _tickerCoroutine = StartCoroutine(TickCoroutine());
            _minuteCoroutine = StartCoroutine(MinuteCoroutine());
            
            DebugUtil.Log("[App Initialize] TimeHandler Start");
        }


        private void StopTimeManager()
        {
            if (_dayOverCoroutine != null)
            {
                StopCoroutine(_dayOverCoroutine);
            }

            if (_tickerCoroutine != null)
            {
                StopCoroutine(_tickerCoroutine);
            }

            if (_minuteCoroutine != null)
            {
                StopCoroutine(_minuteCoroutine);
            }

            if (_onTheHourCoroutine != null)
            {
                StopCoroutine(_onTheHourCoroutine);
            }

            StopAllCoroutines();
        }

        #endregion

        #region Date

        public DateTime DateTimeToday => DateTimeNow.Date;

        public DateTime DateTimeNow
        {
            get
            {
                var dateTime = UtilLibrary.UnixTimeToDateTime(ServerUnixTime);

                return dateTime;
            }
        }

        public long ServerUnixTime => _serverUnixTime + ((long)Time.realtimeSinceStartup - _refreshTime);

        private long _serverUnixTime;
        private long _refreshTime;

        public void SetServerTime(long unixTime)
        {
            if (unixTime <= 0)
            {
                return;
            }

            _serverUnixTime = unixTime;
            _refreshTime = (long)Time.realtimeSinceStartup;
        }


        public DayOfWeek GetServerDayOfWeek()
        {
            return DateTimeNow.DayOfWeek;
        }

        #endregion


        public void SetTimeScale(float scale)
        {
            Time.timeScale = scale;
        }


        #region delegate

        private IEnumerator TickCoroutine()
        {
            var second = new WaitForSecondsRealtime(1f);

            while (true)
            {
                yield return second;

                try
                {
                    _onTick?.Dispatch();
                }
                catch (Exception e)
                {
                    DebugUtil.LogError($"[Error] TimeSystem Exception {e}");
                }
            }
        }

        private void Update()
        {
            if (!_isActivate || _isPaused)
            {
                return;
            }
            
            _onUpdate?.Dispatch(Time.deltaTime);
        }


        private IEnumerator MinuteCoroutine()
        {
#if __DEV || UNITY_EDITOR
            var second = new WaitForSecondsRealtime(10);
#else
        var second = new WaitForSecondsRealtime(60);
#endif

            while (true)
            {
                _onMinute?.Dispatch();

                yield return second;
            }
        }


        private IEnumerator OnTheHourCoroutine()
        {
            while (true)
            {
                var preHour = DateTimeToday.AddHours(DateTimeNow.Hour);
                var nextHour = preHour.AddHours(1).AddMinutes(1);
                var diff = nextHour - DateTimeNow;
                var second = (int)diff.TotalSeconds;

                second = Mathf.Max(0, second);

                if (second <= 0)
                {
                    yield return new WaitForSecondsRealtime(1);
                }
                else
                {
                    yield return new WaitForSecondsRealtime(second);
                }
            }
        }


        private Relay _onTick = new Relay();
        private Relay _onMinute = new Relay();
        private Relay _onNextDay = new Relay();
        private Relay<float> _onUpdate = new Relay<float>();
        
        public void AddOnUpdate(Action<float> callback)
        {
            _onUpdate.AddListener(callback);

            callback.Invoke(Time.deltaTime);
        }
        
        public void RemoveOnUpdate(Action<float> callback)
        {
            _onUpdate.RemoveListener(callback);
        }


        public void AddOnTickCallback(Action callback)
        {
            
            _onTick.AddListener(callback);

            callback.Invoke();
        }


        public void RemoveOnTickCallback(Action callback)
        {
            _onTick.RemoveListener(callback);
        }


        public void AddOnMinuteCallback(Action callback)
        {
            _onMinute.AddListener(callback);
        }


        public void RemoveOnMinuteCallback(Action callback)
        {
            _onMinute.RemoveListener(callback);
        }


        public void AddOnNextDay(Action callback)
        {
            _onNextDay.AddListener(callback);
        }


        public void RemoveOnNextDay(Action callback)
        {
            _onNextDay.RemoveListener(callback);
        }

        public void SetActivate()
        {
            _isActivate = true;

            StartTimeCoroutine();
        }

        public void ResetActivate()
        {
            _onTick.RemoveAll();
            _onNextDay.RemoveAll();
            _onMinute.RemoveAll();
            _onUpdate.RemoveAll();

            _isActivate = false;

            StopTimeManager();
        }

        public override void OnShutdown()
        {
            // ResetActivate 메서드를 사용하여 모든 타이머와 이벤트 정리
            ResetActivate();
        }

        #endregion
    }
}
