using _2Scripts.Manager;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        GameManager.GetManager<SceneManager>().ActivateLoadingScreen();
        GameManager.instance.ChangeGameState(GameState.MainMenu);
        GameManager.GetManager<SceneManager>().LoadSceneNetwork(Scenes.MainMenu);
    }
}
