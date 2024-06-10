using Cinemachine;
using UnityEngine;
using Unity.Netcode;
using NaughtyAttributes;

namespace _2Scripts.Entities.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerBehaviour : NetworkBehaviour
    {
        [SerializeField] private Vector2 camSens = new(100, 100);
        [SerializeField] private float playerSpeed = 2.0f;
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float airControl = 0;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform _camTransform;
        [SerializeField] private bool _overrideNetwork = false;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        public int gold;

        private CharacterController _characterController;
        private float _characterControllerOriginalStepOffset;
        private InputManager _inputManager;
        
        private GameObject _objectToAddToInventory;
        
        private float ySpeed;

        private HealthComponent _health;
        [SerializeField] private NetworkVariable<bool> _isDead = new NetworkVariable<bool>();
        
        public NetworkVariable<bool> IsDead
        {
            get => _isDead;
            set => _isDead = value;
        }

        public Inventory inventory;
        public StatComponent stat;

        private void Start()
        {
            _health = GetComponent<HealthComponent>();

            _health.OnDeath.AddListener(OnDie);
            _health.OnDamaged.AddListener(OnDamaged);

            _characterController = GetComponent<CharacterController>();
            _characterControllerOriginalStepOffset = _characterController.stepOffset;
            _inputManager = InputManager.instance;


            if (!IsOwner)
            {
                if (!_overrideNetwork)
                {
                    _camTransform.gameObject.SetActive(false);
                    _virtualCamera.gameObject.SetActive(false);

                    enabled = false;
                }
            }

            Debug.Log("PLayerBehaviour Done Start ");
            //SwitchWeapon(true);
        }

        private void Update()
        {
            if (_isDead.Value)
            {
                return;
            }
        }

        public void tst()
        {
            _health.InflictDamageTest();
        }

    private void FixedUpdate()
    {
        if (_isDead.Value)
        {
            return;
        }
        Vector3 forward = _camTransform.forward;
        forward.y = 0;
        Vector3 move = forward.normalized * _inputManager.GetPlayerMovement().y + _camTransform.right * _inputManager.GetPlayerMovement().x;
        
        ySpeed += Physics.gravity.y * Time.fixedDeltaTime; 
        
        switch (_characterController.isGrounded)
        {
            case true when _inputManager.PlayerJumped():
                ySpeed = jumpHeight;
                break;
            case true :
                ySpeed = -0.5f;
                _characterController.stepOffset = _characterControllerOriginalStepOffset;
                break;
            case false:
                _characterController.stepOffset = 0;
                move *= airControl;
                break;
        }


        _characterController.Move((move * playerSpeed + Vector3.up * ySpeed)*Time.fixedDeltaTime);
    
        transform.rotation = Quaternion.Euler(0, _camTransform.eulerAngles.y, 0);
        
        animator.SetFloat("XAxis", _inputManager.GetPlayerMovement().x);
        animator.SetFloat("YAxis", _inputManager.GetPlayerMovement().y);
        animator.SetBool("IsJumping", !_characterController.isGrounded);
    }

        private void OnDamaged(float damage)
        {
            //TODO do smthg?
        }

        private void OnDie()
        {
            if (IsServer)
            {
                ModifyDeathValue(true);
            }

            transform.Rotate(transform.right, 90.0f);
        }

        private void ModifyDeathValue(bool newBool)
        {
            Debug.Log("Modify isDead value");
            _isDead.Value = newBool;
        }

        public void TeleportPlayer(Vector3 pos)
        {
            _characterController.enabled = false;
            gameObject.transform.position = pos;
            _characterController.enabled = true;
        }

        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.yellow;
            //Gizmos.DrawWireSphere(_camTransform.position, 1.0f);
            Gizmos.DrawRay(_camTransform.position, _camTransform.TransformDirection(_camTransform.forward));
            //Gizmos.DrawSphere(transform.position, 1);
        }
    }
}