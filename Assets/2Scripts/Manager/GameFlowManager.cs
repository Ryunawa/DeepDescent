using _2Scripts.Interfaces;
using _2Scripts.ProceduralGeneration;
using _2Scripts.UI;
using UnityEngine;
using UnityEngine.Events;

namespace _2Scripts.Manager
{
    public class GameFlowManager : GameManagerSync<GameFlowManager>
    {
    public static GameFlowManager Instance { get; private set; }

    public UnityEvent<Timer.Timer> OnNextLevelEvent;

    public enum LevelState
    {
        BossNotDiscovered,
        BossInProgress,
        BossDefeated
    }

    public LevelState CurrentState { get; private set; } = LevelState.BossNotDiscovered;

    public Timer.Timer timer { get; private set; }

    public int currLevel { get; private set; }

    protected override void OnGameManagerChangeState(GameState gameState)
    {
        GameManager.instance.levelGenerator.dungeonGeneratedEvent.AddListener(StartGame);
        timer = FindObjectOfType<Timer.Timer>();
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
