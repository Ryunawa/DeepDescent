using _2Scripts.Interfaces;
using _2Scripts.Manager;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SceneManager = _2Scripts.Manager.SceneManager;

public class LobbyList : MonoBehaviour
{
    [SerializeField] private Button btn;
    [SerializeField] private LobbyButton prefab;
    [SerializeField] private GameObject parentMenu;

    private MultiManager _multiManager;
    [SerializeField] private Button btnJoin;

    private void Start()
    {
        _multiManager = GameManager.GetManager<MultiManager>();
        _multiManager.init.AddListener(RefreshUI);
    }

    public void ButtonSelected(LobbyButton lobbyButton)
    {
        btnJoin.interactable = true;

        btn.onClick.RemoveAllListeners();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        btn.onClick.AddListener(() => _multiManager.JoinLobby(lobbyButton.GetLobbyId()));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    public void SelectPlayer()
    {
        GameManager.GetManager<SceneManager>().ActivateLoadingScreen();
        transform.root.gameObject.SetActive(false);
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.CharacterSelection.ToString(), LoadSceneMode.Additive);
    }
    
    private void CreateListOfLobbiesInMenu(QueryResponse lobbies)
    {
        if (lobbies == null)
        {
            return;
        }
        
        for(int i = 0; i < parentMenu.transform.childCount; i++)
        {
            Destroy(parentMenu.transform.GetChild(i).gameObject);
        }

        foreach(Lobby lobby in lobbies.Results)
        {
            LobbyButton lbyBtn = Instantiate(prefab, parentMenu.transform);
            lbyBtn.InitButton(lobby.Id, lobby.Name, lobby.Players.Count + "/" + lobby.MaxPlayers, this);
        }
    }

    public async void RefreshUI()
    {
        Debug.Log("Refresh");
        CreateListOfLobbiesInMenu(await _multiManager.GetAllLobbies());
    }
}
