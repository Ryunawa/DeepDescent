using _2Scripts.Manager;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainLobby : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject parentUIObject;
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private GameObject[] uiToDeactivate;
    [SerializeField] private TMP_Text lobbyName;
    [SerializeField] private Button playButton;

    
    void Start()
    {
        MultiManager.instance.lobbyCreated.AddListener(ShowUI);
        MultiManager.instance.lobbyJoined.AddListener(ShowUI);
        MultiManager.instance.refreshUI.AddListener(RefreshUI);
        MultiManager.instance.kickedEvent.AddListener(ReturnToLobbyList);
        MultiManager.instance.CharacterChosen.AddListener(OnCharacterChosen);
    }

    private void ShowUI()
    {
        foreach (var uiObject in uiToDeactivate)
        {
            uiObject.SetActive(false);
        }

        lobbyUI.SetActive(true);
        lobbyName.text = MultiManager.instance.Lobby.Name;
        
        playButton.interactable = false;
        
        RefreshUI(false);
    }

    void RefreshUI(bool isAllReady, bool changeButtonState = true)
    {
        foreach (Transform transformGo in parentUIObject.transform)
        {
            Destroy(transformGo.gameObject);
        }
        
        foreach (var player in MultiManager.instance.Lobby.Players)
        {
            var playerNameText = Instantiate(playerPrefab, parentUIObject.transform).GetComponentInChildren<TMP_Text>();
            playerNameText.text = player.Data["Name"].Value;
        }

        if (!changeButtonState) return;
        if (MultiManager.instance.IsLobbyHost())
        {
            playButton.interactable = isAllReady;
        }

        if (MultiManager.instance.Lobby.Players.Count == 1)
        {
            playButton.interactable = true;
        }
    }

    private void ReturnToLobbyList()
    {
        lobbyUI.SetActive(false);
        uiToDeactivate[0].gameObject.SetActive(true);
    }

    private void OnCharacterChosen()
    {
        SceneManager.instance.DeactivateLoadingScreen();
        transform.root.gameObject.SetActive(true);
    }
    
}
