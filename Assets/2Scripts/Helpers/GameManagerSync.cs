using _2Scripts.Manager;
using Unity.Netcode;

namespace _2Scripts.Helpers
{
    public abstract class GameManagerSync<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        protected virtual void Start()
        {
            GameManager.stateChanged.AddListener(OnGameManagerChangeState);
        }

        protected virtual void OnGameManagerChangeState(GameState gameState){}
    }
}