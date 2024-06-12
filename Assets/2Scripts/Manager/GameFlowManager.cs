using _2Scripts.ProceduralGeneration;
using UnityEngine.Events;

namespace _2Scripts.Manager
{
    public class GameFlowManager : Singleton<GameFlowManager>
    {
        public static GameFlowManager Instance { get; private set; }

        public UnityEvent<Timer.Timer> OnNextLevelEvent;
        
        public enum GameState { BossNotDiscovered, BossInProgress, BossDefeated }
        public GameState CurrentState { get; private set; } = GameState.BossNotDiscovered;

        public Timer.Timer timer { get; private set; }

        public int currLevel { get; private set; }

        private void Start()
        {
            MultiManager.instance.levelGenerator.dungeonGeneratedEvent.AddListener(StartGame);
            timer = FindObjectOfType<Timer.Timer>();
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
            timer.StartTimer();
        }
        
        /// <summary>
        /// Call this to load the next level
        /// </summary>
        public void LoadNextLevel()
        {
            timer.StopTimer();
            OnNextLevelEvent?.Invoke(timer);
            currLevel++;
        }
    
    }
}
