using _2Scripts.Manager;

public class GameFlowManager : Singleton<GameFlowManager>
{
    public static GameFlowManager Instance { get; private set; }

    public enum GameState { BossNotDiscovered, BossInProgress, BossDefeated }
    public GameState CurrentState { get; private set; } = GameState.BossNotDiscovered;

    public void SetGameState(GameState state)
    {
        CurrentState = state;
    }
}
