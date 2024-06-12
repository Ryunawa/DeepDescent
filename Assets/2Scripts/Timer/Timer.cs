using System.Diagnostics;
using NaughtyAttributes;
using UnityEngine;

namespace _2Scripts.Timer
{
    public class Timer : MonoBehaviour
    {
        private Stopwatch _timer;

        private void Start()
        {
            StartTimer();
        }

        /// <summary>
        /// Create or resume the timer.
        /// </summary>
        [Button]
        public void StartTimer()
        {
            _timer ??= new Stopwatch();
            _timer.Start();
        }

        /// <summary>
        /// Pause the timer.
        /// </summary>
        [Button]
        public void StopTimer()
        {
            _timer.Stop();
        }
        
        /// <summary>
        /// Reset the timer value to 0.
        /// </summary>
        public void ResetTimer()
        {
            _timer.Reset();
        }

        /// <summary>
        /// Return the time elapsed in the stop watch.
        /// </summary>
        /// <returns>return a string on the format "00:00".</returns>
        public string GetTimerElapsedTime()
        {
            int minute = Mathf.FloorToInt((float)_timer.Elapsed.TotalSeconds / 60);
            int seconds = Mathf.FloorToInt((float)_timer.Elapsed.TotalSeconds % 60);
            return $"{minute:00}:{seconds:00}";
        }

        /// <summary>
        /// Return the timer object (type Stopwatch)
        /// </summary>
        /// <returns></returns>
        public Stopwatch GetStopWatchObject()
        {
            return _timer;
        }
    }
}
