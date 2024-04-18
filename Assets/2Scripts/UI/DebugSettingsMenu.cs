using UnityEngine;

namespace _2Scripts.UI
{
    public class DebugSettingsMenu : MonoBehaviour
    {
        [SerializeField] private GameObject bindingMenu, audioMenu;

        public void ShowHideBindingMenu()
        {
            bindingMenu.SetActive(!bindingMenu.activeSelf);
        }
    
        public void ShowHideAudioMenu()
        {
            audioMenu.SetActive(!audioMenu.activeSelf);
        }
    }
}
