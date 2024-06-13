using UnityEngine.Events;

namespace _2Scripts.Manager
{
    public class GameFlowManager : Singleton<GameFlowManager>
    {
        public static GameFlowManager Instance { get; private set; }

        public UnityEvent<Timer.Timer> OnNextLevelEvent;
        
        public enum GameState { BossNotDiscovered, BossInProgress, BossDefeated }
        public GameState CurrentState { get; private set; } = GameState.BossNotDiscovered;

        public Timer.Timer Timer { get; private set; }

        public int CurrLevel { get; private set; } = 1;

        private void Start()
        {
            MultiManager.instance.levelGenerator.dungeonGeneratedEvent.AddListener(StartGame);
            Timer = FindObjectOfType<Timer.Timer>();
        }

        public void SetGameState(GameState state)
        {
            CurrentState = state;
        }
        
        private void StartGame()
        {
            if (MultiManager.instance.levelGenerator.spawnShop)
            {
                return;
            }
            Timer.StartTimer();
        }
        
        /// <summary>
        /// Call this to load the next level
        /// </summary>
        public void LoadNextLevel()
        {
            Timer.StopTimer();
            OnNextLevelEvent?.Invoke(Timer);
            CurrLevel++;
        }
    
    }
}
