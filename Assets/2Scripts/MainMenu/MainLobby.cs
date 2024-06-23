using _2Scripts.Enum;
using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using _2Scripts.Manager;
using NaughtyAttributes;
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


    [SerializeField] private Sprite checkToggle;
    [SerializeField] private Sprite uncheckToggle;
    [SerializeField] private Toggle easyToggle;
    [SerializeField] private Toggle normalToggle;
    [SerializeField] private Toggle hardToggle;
    private int difficultyLevel = 2;

    private MultiManager multiManager;

    protected override void OnGameManagerChangeState(GameState gameState)
    {
        // Play Music
        GameManager.GetManager<AudioManager>().PlayMusic("MenuMusic", 0.1f);
        
        multiManager = GameManager.GetManager<MultiManager>();
        
        multiManager.lobbyCreated.AddListener(ShowUI);
        multiManager.lobbyJoined.AddListener(ShowUI);
        multiManager.refreshUI.AddListener(RefreshUI);
        multiManager.kickedEvent.AddListener(ReturnToLobbyList);
        multiManager.CharacterChosen.AddListener(OnCharacterChosen);
        playButton.onClick.AddListener(GameManager.GetManager<MultiManager>().Init);
        createButton.onClick.AddListener(GameManager.GetManager<MultiManager>().CreateLobby);
        actualPlayButton.onClick.AddListener(GameManager.GetManager<MultiManager>().StartGame);

        // Add listeners to the difficulty toggles
        easyToggle.onValueChanged.AddListener(delegate { ChangeDifficultyBtn(1); });
        normalToggle.onValueChanged.AddListener(delegate { ChangeDifficultyBtn(2); });
        hardToggle.onValueChanged.AddListener(delegate { ChangeDifficultyBtn(3); });

        // Initialize the difficulty to a default value
        ChangeDifficultyBtn(2);
    }

    private void ShowUI()
    {
        foreach (var uiObject in uiToDeactivate)
        {
            uiObject.SetActive(false);
        }

        lobbyUI.SetActive(true);
        lobbyName.text = multiManager.Lobby.Name;
        
        actualPlayButton.interactable = true;
        
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

        // if (!changeButtonState) return;
        // if (multiManager.IsLobbyHost())
        // {
        //     actualPlayButton.interactable = isAllReady;
        // }
        //
        // if (multiManager.Lobby.Players.Count == 1)
        // {
        //     actualPlayButton.interactable = true;
        // }
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


    private void ChangeDifficultyBtn(int difficulty)
    {
        difficultyLevel = difficulty;

        // Reset all buttons to unchecked
        ResetAllButtons();

        // Change the appropriate button's child image to checked
        switch (difficulty)
        {
            case 1:
                SetToggleImage(easyToggle, checkToggle);
                multiManager.AdjustDifficulty(DifficultyMode.Easy);
                break;
            case 2:
                SetToggleImage(normalToggle, checkToggle);
                multiManager.AdjustDifficulty(DifficultyMode.Normal);
                break;
            case 3:
                SetToggleImage(hardToggle, checkToggle);
                multiManager.AdjustDifficulty(DifficultyMode.Hard);
                break;
        }

    }

    private void ResetAllButtons()
    {
        SetToggleImage(easyToggle, uncheckToggle);
        SetToggleImage(normalToggle, uncheckToggle);
        SetToggleImage(hardToggle, uncheckToggle);
    }

    private void SetToggleImage(Toggle toggle, Sprite newSprite)
    {
        // Assuming the child image is the first child of the toggle
        Image toggleImage = toggle.transform.GetChild(0).GetComponent<Image>();
        toggleImage.sprite = newSprite;
    }

    public int GetSelectedDifficulty()
    {
        return difficultyLevel;
    }

}
