using _2Scripts.Manager;
using UnityEditor.SearchService;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        GameManager.GetManager<SceneManager>().ActivateLoadingScreen();
        GameManager.instance.ChangeGameState(GameState.MainMenu);
        GameManager.GetManager<SceneManager>().LoadScene(Scenes.MainMenu);
    }
}
