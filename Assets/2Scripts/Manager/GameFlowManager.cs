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

            public int CurrLevel { get; private set; } = 1;

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if(gameState != GameState.InLevel) { return; }
            GameManager.instance.levelGenerator.dungeonGeneratedEvent.AddListener(StartGame);
            Timer = FindObjectOfType<Timer.Timer>();
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
            [Rpc(SendTo.Server)]
        public void LoadNextLevelServerRpc()
        {
            GameManager.instance.ResetNumberOfDeadPlayer();
            LoadNextLevelClientRpc();
            Timer.StopTimer();
            OnNextLevelEvent?.Invoke(Timer);
            CurrLevel++;
        }


        [Rpc(SendTo.ClientsAndHost)]
        private void LoadNextLevelClientRpc()
        {
            GameManager.instance.ChangeGameState(GameState.Generating);
        }

    }
}
