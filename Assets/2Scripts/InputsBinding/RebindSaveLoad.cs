using UnityEngine;
using UnityEngine.InputSystem;

namespace _2Scripts.InputsBinding
{
    public class RebindSaveLoad : MonoBehaviour
    {
        public InputActionAsset actions;

        public void OnEnable()
        {
            Debug.Log("load binding");
            var rebinds = PlayerPrefs.GetString("rebinds");
            if (!string.IsNullOrEmpty(rebinds))
                actions.LoadBindingOverridesFromJson(rebinds);
        }

        public void OnDisable()
        {
            var rebinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
        }

        public void SaveBindings()
        {
            var rebinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
        }
    }
}
