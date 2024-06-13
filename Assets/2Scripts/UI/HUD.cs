using System;
using _2Scripts.Interfaces;
using _2Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _2Scripts.UI
{
    public class HUD : GameManagerSync<HUD>
    {
        [SerializeField] private float gameTime = 3;
        
        [Header("Left")]
        [SerializeField] private Slider HP;
        [SerializeField] private Slider BlueBar;
        
        [Space,Header("Right")]
        [SerializeField] private Gradient difficultyGradient;
        [SerializeField] private Image difficultyImage;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _levelText;
        
        [SerializeField] private Image[] quickSlotsImages = new Image[3];
        [SerializeField] private TextMeshProUGUI[] quickSlotsQuantity = new TextMeshProUGUI[3];

        private GameFlowManager _gameFlowManager;

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if (gameState != GameState.InLevel) return;
            Debug.Log("Hud start");
            _gameFlowManager = GameManager.GetManager<GameFlowManager>();
            
            _gameFlowManager.OnNextLevelEvent.AddListener(arg0 =>
            {
                SetLevelNumber(_gameFlowManager.timer.GetTimerElapsedTime());
                UpdateDifficultyColor();
            });
        }

        private void Update()
        {
            if (_gameFlowManager)
                SetTimer(_gameFlowManager.timer.GetTimerElapsedTime());
        }

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

        private void SetTimer(string value)
        {
            _timerText.text = value;
        }

        private void UpdateDifficultyColor()
        {
            float colorSampleValue = (GameManager.GetManager<DifficultyManager>().GetDifficultyMultiplier()-1) / gameTime;
            difficultyImage.color = difficultyGradient.Evaluate(Mathf.Clamp(colorSampleValue, 0, 1));
        }

        private void SetLevelNumber(string value)
        {
            _levelText.text = value;
        }

        public bool SetQuickSlot(Sprite image, int quantity, int index)
        {
            if (index is > 3 or < 0)return false;

            quickSlotsImages[index].color = Color.white;
            quickSlotsImages[index].sprite = image;
            quickSlotsQuantity[index].text = quantity.ToString();
            
            return true;
        }

        public bool ClearQuickSlot(int index)
        {
            if (index is > 3 or < 0)return false;

            quickSlotsImages[index].color = Color.clear;
            quickSlotsQuantity[index].text = "";

            return true;
        }
        
    }
}
