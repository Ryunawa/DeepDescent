using System.Diagnostics;
using UnityEngine;

namespace _2Scripts.Timer
{
    public class Timer
    {
        private Stopwatch _timer;

        /// <summary>
        /// Create or resume the timer.
        /// </summary>
        public void StartTimer()
        {
            _timer ??= new Stopwatch();
            _timer.Start();
        }

        /// <summary>
        /// Pause the timer.
        /// </summary>
        public void StopTimer()
        {
            _timer.Stop();
        }
        
        /// <summary>
        /// Reset the timer value to 0.
        /// </summary>
        private void ResetTimer()
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
    }
}
