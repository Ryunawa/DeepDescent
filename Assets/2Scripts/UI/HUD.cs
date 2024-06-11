using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace _2Scripts.UI
{
    public class HUD : Singleton<HUD>
    {
        [Header("Left")]
        [SerializeField] private Slider HP;
        [SerializeField] private Slider BlueBar;
        
        [Space,Header("Right")]
        [SerializeField] private Gradient difficultyGradient;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _levelText;
        
        [SerializeField] private Image[] quickSlotsImages = new Image[4];
        
        
        public bool SetHp(float value)
        {
            if (value is > 1 or < 0)return false;
            HP.value = value;
            return true;
        }
        
        public bool SetBlueBar(float value)
        {
            if (value is > 1 or < 0)return false;
            BlueBar.value = value;
            return true;
        }

        public void SetTimer(string value)
        {
            _timerText.text = value;
        }

        public void SetDifficultyColor(float value)
        {
            
        }

        public void SetLevelNumber(string value)
        {
            _levelText.text = value;
        }

        public bool SetQuickSlotImage(Sprite image, int index)
        {
            if (index is > 4 or < 0)return false;

            quickSlotsImages[index].sprite = image;
            
            return true;
        }
        
    }
}
