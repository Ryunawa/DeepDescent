using _2Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUsername : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    private Button _playBtn;

    private void Start()
    { 
        TryGetComponent<Button>(out _playBtn);
        CheckForUsername();
    }

    public void CheckForUsername()
    {
        _playBtn.interactable = inputField.text != "";
    }

    public void SetUsername()
    {
        GameManager.GetManager<MultiManager>().PlayerName = inputField.text;
    }
}
