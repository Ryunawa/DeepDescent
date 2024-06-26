using System;
using _2Scripts.Entities;
using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using _2Scripts.Manager;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _2Scripts.UI
{
    public class HUD : GameManagerSync<HUD>
    {
        [SerializeField] private float gameTime = 3;

        [Header("Visual Feedback")]
        private bool isFlashing = false;
        [SerializeField] private Image bloodImg;
        [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.1f);
        [SerializeField] private float flashSpeed = 5f;
        [SerializeField] private float flashDuration = 0.5f;

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
                SetLevelNumber(_gameFlowManager.CurrLevel.ToString());
                UpdateDifficultyColor();
            });
        }

        private void Update()
        {
            if (_gameFlowManager)
                SetTimer(_gameFlowManager.Timer.GetTimerElapsedTime());


            if (isFlashing)
            {
                Color currentColor = bloodImg.color;
                currentColor.a = Mathf.Lerp(currentColor.a, 0, flashSpeed * Time.deltaTime / flashDuration);
                bloodImg.color = currentColor;

                if (currentColor.a <= 0.01f)
                {
                    bloodImg.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0);
                    isFlashing = false;
                }
            }
        }
        
        public bool SetHp()
        {
           HP.value = GameManager.playerBehaviour.Health.GetHealth() / GameManager.playerBehaviour.Health.MaxHealth;
           
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
            quickSlotsQuantity[index].text = quantity == 1 ? "": quantity.ToString();
            
            return true;
        }

        public bool ClearQuickSlot(int index)
        {
            if (index is > 3 or < 0)return false;

            quickSlotsImages[index].color = Color.clear;
            quickSlotsQuantity[index].text = "";

            return true;
        }

        public void FlashDamageEffect(float currentHealth, float maxHealth)
        {
            isFlashing = true;
            Color flashColor = bloodImg.color;
            float healthPercentage = currentHealth / maxHealth;

            // Inverse the health percentage to make the image more visible as health decreases
            flashColor.a = 0.5f * (1 - healthPercentage);

            bloodImg.color = flashColor;
        }

    }
}
