using _2Scripts.Manager;
using TMPro;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement;
using SceneManager = _2Scripts.Manager.SceneManager;

public class CharacterSelection : MonoBehaviour
{
    [SerializeField] private GameObject[] characters;
    [SerializeField] private int selectedCharacter = 0;
    [SerializeField] private TextMeshProUGUI characterName;

    private void Start()
    {
        SetCharacterNameUI();
        SceneManager.instance.DeactivateLoadingScreen();
    }

    public void NextCharacter()
    {
        AudioManager.instance.PlaySfx("UINormalBtn");
        characters[selectedCharacter].SetActive(false);
        selectedCharacter = (selectedCharacter + 1) % characters.Length;
        SetCharacterNameUI();
        characters[selectedCharacter].SetActive(true);
    }

    public void PreviousCharacter()
    {
        AudioManager.instance.PlaySfx("UINormalBtn");
        characters[selectedCharacter].SetActive(false);
        selectedCharacter--;
        if (selectedCharacter < 0) selectedCharacter += characters.Length;
        SetCharacterNameUI();
        characters[selectedCharacter].SetActive(true);
    }

    private void SetCharacterNameUI()
    {
        AudioManager.instance.PlaySfx("UINormalBtn");
        string originalName = characters[selectedCharacter].gameObject.name;
        string modifiedName = originalName.Replace("_", " - ");
        characterName.text = modifiedName;
    }

    public void Ready()
    {
        AudioManager.instance.PlaySfx("UIValidationBtn");
        PlayerPrefs.SetInt("selectedCharacter", selectedCharacter);
        SceneManager.instance.ActivateLoadingScreen();
        MultiManager.instance.UpdatePlayer(selectedCharacter, true);

        UnitySceneManager.SceneManager.UnloadSceneAsync(Scenes.CharacterSelection.ToString());
    }
}
