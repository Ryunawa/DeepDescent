using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace _2Scripts.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class Loading : MonoBehaviour
    {
        private int state = 0;
        private TextMeshProUGUI text;

        private void Start()
        {
            TryGetComponent(out text);
            StartCoroutine(LoadingText());
        }
        
        private IEnumerator LoadingText()
        {
            while (true)
            {
                switch (state)
                {
                    default:
                        text.text = " Loading    ";
                        break;
                    case 0:
                        text.text = " Loading .  ";
                        break;
                    case 1:
                        text.text = " Loading .. ";
                        break;
                    case 2:
                        text.text = " Loading ...";
                        break;
                }

                state = (state + 1) % 4;

                yield return new WaitForSeconds(1);
            }
        }
    }
}