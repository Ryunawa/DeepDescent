using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _2Scripts.UI
{
    public class TextSlider : MonoBehaviour
    {
        [SerializeField] private List<string> Texts;
        
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
                text.text = Texts[Random.Range(0, Texts.Count)];
                
                yield return new WaitForSeconds(7);
            }
        }
    }
}