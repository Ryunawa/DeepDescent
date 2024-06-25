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
    [SerializeField] private Button mainMenuButton;


    [SerializeField] private Sprite checkToggle;
    [SerializeField] private Sprite uncheckToggle;
    [SerializeField] private Toggle easyToggle;
    [SerializeField] private Toggle normalToggle;
    [SerializeField] private Toggle hardToggle;
    private int difficultyLevel = 2;

    private MultiManager multiManager;

    protected override void OnGameManagerChangeState(GameState gameState)
    {
        if (gameState != GameState.MainMenu) return;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameManager.GetManager<SceneManager>().DeactivateLoadingScreen();
        
        // Play Music
        GameManager.GetManager<AudioManager>().PlayMusic("MenuMusic", 0.1f);
        
        multiManager = GameManager.GetManager<MultiManager>();
        
        multiManager.lobbyCreated.AddListener(ShowUI);
        multiManager.lobbyJoined.AddListener(ShowUI);
        multiManager.refreshUI.AddListener(RefreshUI);
        multiManager.kickedEvent.AddListener(ReturnToLobbyList);
        multiManager.CharacterChosen.AddListener(OnCharacterChosen);
        playButton.onClick.AddListener(multiManager.Init);
        createButton.onClick.AddListener(multiManager.CreateLobby);
        actualPlayButton.onClick.AddListener(multiManager.StartGame);
        //mainMenuButton.onClick.AddListener(multiManager.LeaveLobby);

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

        if (!multiManager) return;
        bool isActive = multiManager.IsLobbyHost();
        
        actualPlayButton.gameObject.SetActive(isActive);
        easyToggle.transform.parent.gameObject.SetActive(isActive);
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
