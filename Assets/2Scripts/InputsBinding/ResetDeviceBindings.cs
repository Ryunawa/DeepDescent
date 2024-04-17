using UnityEngine;
using UnityEngine.InputSystem;

namespace _2Scripts.InputsBinding
{
    public class ResetDeviceBindings : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _inputActions;

        public void ResetAllBindings()
        {
            foreach (InputActionMap map in _inputActions.actionMaps)
            {
                map.RemoveAllBindingOverrides();
            }
        }
    }
}
