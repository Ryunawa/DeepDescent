using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using _2Scripts.ProceduralGeneration;
using _2Scripts.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace _2Scripts.Manager
{
    public class GameFlowManager : GameManagerSync<GameFlowManager>
    {

        public UnityEvent<Timer.Timer> OnNextLevelEvent;

        public enum LevelState
        {
            BossNotDiscovered,
            BossInProgress,
            BossDefeated
        }

        public LevelState CurrentState { get; private set; } = LevelState.BossNotDiscovered;

        public Timer.Timer Timer { get; private set; }

            public int CurrLevel { get; private set; } = 4;

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if(gameState != GameState.InLevel) { return; }
            Timer = FindObjectOfType<Timer.Timer>();
            StartGame();
        }

        public void SetGameState(LevelState state)
        {
            CurrentState = state;
        }

        private void StartGame()
        {
            if (GameManager.instance.levelGenerator.spawnShop)
            {
                return;
            }

            Timer.StartTimer();
        }

            /// <summary>
            /// Call this to load the next level
            /// </summary>
        public void LoadNextLevelServer()
        {
            GameManager.instance.ResetNumberOfDeadPlayer();
            Timer.StopTimer();
            GameManager.instance._networkData.isHostRdy.Value = false;
            GameManager.instance._networkData.isClientOneRdy.Value = false;
            GameManager.instance._networkData.isClientTwoRdy.Value = false;
            GameManager.instance._networkData.isClientThreeRdy.Value = false;
            CurrLevel++;
            OnNextLevelEvent?.Invoke(Timer);
        }
    }
}
