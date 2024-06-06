using System.Collections;
using UnityEngine;

namespace _2Scripts.Helpers
{
    public static class Delay
    {
        /// <summary>
        /// Wait the given time
        /// </summary>
        /// <param name="pTimeToPause">Time to wait in seconds</param>
        /// <returns></returns>
        public static IEnumerator Pause(float pTimeToPause)
        {
            yield return new WaitForSeconds(pTimeToPause);
        }
    }
}