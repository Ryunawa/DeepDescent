using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace _2Scripts.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class Loading : MonoBehaviour
    {
        private int _state;
        private TextMeshProUGUI _text;

        private void OnEnable()
        {
            TryGetComponent(out _text);
            StartCoroutine(LoadingText());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator LoadingText()
        {
            while (!_text)
            {
                TryGetComponent(out _text);
            }
            
            while (true)
            {
                switch (_state)
                {
                    default:
                        _text.text = " Loading    ";
                        break;
                    case 0:
                        _text.text = " Loading .  ";
                        break;
                    case 1:
                        _text.text = " Loading .. ";
                        break;
                    case 2:
                        _text.text = " Loading ...";
                        break;
                }

                _state = (_state + 1) % 4;

                yield return new WaitForSeconds(1);
            }
        }
    }
}