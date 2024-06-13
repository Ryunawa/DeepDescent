using _2Scripts.Interfaces;
using _2Scripts.Manager;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainLobby : GameManagerSync<MainLobby>
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject parentUIObject;
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private GameObject[] uiToDeactivate;
    [SerializeField] private TMP_Text lobbyName;
    [FormerlySerializedAs("playButton")] [SerializeField] private Button actualPlayButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button createButton;

    private MultiManager multiManager;

    protected override void OnGameManagerChangeState(GameState gameState)
    {
        multiManager = GameManager.GetManager<MultiManager>();
        
        multiManager.lobbyCreated.AddListener(ShowUI);
        multiManager.lobbyJoined.AddListener(ShowUI);
        multiManager.refreshUI.AddListener(RefreshUI);
        multiManager.kickedEvent.AddListener(ReturnToLobbyList);
        multiManager.CharacterChosen.AddListener(OnCharacterChosen);
        playButton.onClick.AddListener(GameManager.GetManager<MultiManager>().Init);
        createButton.onClick.AddListener(GameManager.GetManager<MultiManager>().CreateLobby);
        actualPlayButton.onClick.AddListener(GameManager.GetManager<MultiManager>().StartGame);
    }

    private void ShowUI()
    {
        foreach (var uiObject in uiToDeactivate)
        {
            uiObject.SetActive(false);
        }

        lobbyUI.SetActive(true);
        lobbyName.text = multiManager.Lobby.Name;
        
        actualPlayButton.interactable = false;
        
        RefreshUI(false);
    }

    void RefreshUI(bool isAllReady, bool changeButtonState = true)
    {
        foreach (Transform transformGo in parentUIObject.transform)
        {
            Destroy(transformGo.gameObject);
        }
        
        foreach (var player in multiManager.Lobby.Players)
        {
            var playerNameText = Instantiate(playerPrefab, parentUIObject.transform).GetComponentInChildren<TMP_Text>();
            playerNameText.text = player.Data["Name"].Value;
        }

        if (!changeButtonState) return;
        if (multiManager.IsLobbyHost())
        {
            actualPlayButton.interactable = isAllReady;
        }

        if (multiManager.Lobby.Players.Count == 1)
        {
            actualPlayButton.interactable = true;
        }
    }

    private void ReturnToLobbyList()
    {
        lobbyUI.SetActive(false);
        uiToDeactivate[0].gameObject.SetActive(true);
    }

    private void OnCharacterChosen()
    {
        GameManager.GetManager<SceneManager>().DeactivateLoadingScreen();
        transform.root.gameObject.SetActive(true);
    }
}
