using System;
using System.Collections.Generic;
using _2Scripts.Helpers;
using _2Scripts.Manager;
using Cinemachine;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;
using NaughtyAttributes;
using Unity.VisualScripting;
using System.Collections;

namespace _2Scripts.Entities.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerBehaviour : GameManagerSync<PlayerBehaviour>, IController
    {
        [SerializeField] private Vector2 camSens = new(100, 100);
        [SerializeField] private float playerSpeed = 2.0f;
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float airControl = 0;
        [SerializeField] private Animator animator;
        [SerializeField] private Animator animatorFPS;
        [SerializeField] private Transform _camTransform;
        [SerializeField] private bool _overrideNetwork = false;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private Camera _Camera;
        [SerializeField] private List<GameObject> playerModels;
        [SerializeField] private List<GameObject> playerModelsFPS;
        [SerializeField] private VisibleItems weaponModelsFPS;
        [SerializeField] private VisibleItems weaponModels;

        [SerializeField] private Transform handPosition;

        public Transform HandPosition => handPosition;

        private CharacterController _characterController;
        private float _characterControllerOriginalStepOffset;
        private InputManager _inputManager;
        private bool _isAttacking;
        private int characterID = 0;


        [DoNotSerialize] public GameObject ObjectToAddToInventory;
        
        private float ySpeed;

        private HealthComponent _health;

        public HealthComponent Health => _health;

        public Camera Camera => _Camera;

        public bool IsAttacking => _isAttacking;

        [SerializeField] private NetworkVariable<bool> _isDead = new NetworkVariable<bool>();
        
        public NetworkVariable<bool> IsDead
        {
            get => _isDead;
            set => _isDead = value;
        }
        public CinemachineVirtualCamera VirtualCamera { get => _virtualCamera; set => _virtualCamera = value; }
        public Camera Camera1 { get => _Camera; set => _Camera = value; }

        public Inventory inventory;
        public StatComponent stat;

        protected override void Start()
        {
            base.Start();
            
            if (IsOwner)
            {
                _health = GetComponent<HealthComponent>();

                _health.OnDeath.AddListener(OnDie);
                _health.OnDamaged.AddListener(OnDamaged);

                _characterController = GetComponent<CharacterController>();
                _characterControllerOriginalStepOffset = _characterController.stepOffset;
                _inputManager = InputManager.instance;

                characterID = GameManager.GetManager<MultiManager>().GetSelectedCharacterID();

                _inputManager.Inputs.Player.QuickSlot.performed += context => UseQuickSlot(context.ReadValue<float>());
                _inputManager.Inputs.Player.Attack.performed += context => Attack();

                GameManager.playerBehaviour = this;
                
                for (int i = 0; i < 4; i++)
                {
                    playerModels[i].SetActive(i == characterID);
                    stat.SetStats(characterID);
                    animator.SetFloat("Class", characterID);
                    animator.SetFloat("Weapon", characterID);
                }
                
                //13 layer 13 rendered on cam
                foreach (var playerModelFPS in playerModelsFPS)
                {
                    playerModelFPS.layer = 13;
                    weaponModelsFPS.ChangeWeaponAndShieldLayer(13);
                }
                //14 layer14 not rendered on cam
                foreach (var playerModel in playerModels)
                {
                    playerModel.layer = 14;
                    weaponModels.ChangeWeaponAndShieldLayer(14);
                }
                

            }
            
            if (!IsOwner)
            {
                if (!_overrideNetwork)
                {
                    //13 layer 13 rendered on cam
                    foreach (var playerModelFPS in playerModelsFPS)
                    {
                        playerModelFPS.layer = 14;
                        weaponModelsFPS.ChangeWeaponAndShieldLayer(14);
                    }
                    //14 layer14 not rendered on cam
                    foreach (var playerModel in playerModels)
                    {
                        playerModel.layer = 13;
                        weaponModels.ChangeWeaponAndShieldLayer(13);
                    }
                    
                    _camTransform.gameObject.SetActive(false);
                    _virtualCamera.gameObject.SetActive(false);

                    enabled = false;
                }
                
                
                
            }

            Debug.Log("PLayerBehaviour Done Start ");
        }
        

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if (gameState != GameState.InLevel) return;
            UpdateCharRpc(characterID);
            if (IsOwner)
            {
                for (int i = 0; i < 4; i++)
                {
                    playerModelsFPS[i].SetActive(i == characterID);
                    animatorFPS.SetFloat("Class", characterID);
                }
            }
        }


        [Rpc(SendTo.NotMe)]
        private void UpdateCharRpc(int id)
        {
            for (int i = 0; i < 4; i++)
            {
                playerModels[i].SetActive(i == id);
                stat.SetStats(id);
                animator.SetFloat("Class", id);
            }
        }

        private void Update()
        {
            if (_isDead.Value)
            {
                return;
            }
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
                    {
                        switch (characterID)
                        {
                            case 0:
                                GameManager.GetManager<AudioManager>().PlaySfx("ArcherJump", this, 1, 5);
                                break;
                            case 1:
                                GameManager.GetManager<AudioManager>().PlaySfx("DwarfJump", this, 1, 5);
                                break;
                            case 2:
                                GameManager.GetManager<AudioManager>().PlaySfx("WitchJump", this, 1, 5);
                                break;
                            case 3:
                                GameManager.GetManager<AudioManager>().PlaySfx("GoblinJump", this, 1, 5);
                                break;
                        }
                        ySpeed = jumpHeight;
                    }
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

        if (ObjectToAddToInventory && _inputManager.PlayerUsed())
        {
            if (ObjectToAddToInventory.TryGetComponent(out Object obj))
                obj.Interact();
        }

        transform.rotation = Quaternion.Euler(0, _camTransform.eulerAngles.y, 0);
        
        if (IsOwner)
        {
            animatorFPS.SetBool("IsRuning", _inputManager.GetPlayerMovement().magnitude > 0);
            animator.SetBool("IsRuning", _inputManager.GetPlayerMovement().magnitude > 0);
        }
        
        animator.SetFloat("XAxis", _inputManager.GetPlayerMovement().x);
        animator.SetFloat("YAxis", _inputManager.GetPlayerMovement().y);
        animator.SetBool("IsJumping", !_characterController.isGrounded);
        
        if (inventory.MainHandItem == null)
        {
            animatorFPS.SetFloat("Weapon", -1);
            animator.SetFloat("Weapon", -1);
        }
        else
        {
            float weaponType = inventory.MainHandItem.WeaponType switch
            {
                WeaponType.AXE => (float)WeaponType.SWORD,
                _ => (float)inventory.MainHandItem.WeaponType
            };
            
            animatorFPS.SetFloat("Weapon", weaponType);
            animator.SetFloat("Weapon", weaponType);
        }
        
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

            switch (characterID)
            {
                case 0:
                    GameManager.GetManager<AudioManager>().PlaySfx("ArcherDeath", this, 2, 10);
                    break;
                case 1:
                    GameManager.GetManager<AudioManager>().PlaySfx("DwarfDeath", this, 2, 10);
                    break;
                case 2:
                    GameManager.GetManager<AudioManager>().PlaySfx("WitchDeath", this, 2, 10);
                    break;
                case 3:
                    GameManager.GetManager<AudioManager>().PlaySfx("GoblinDeath", this, 2, 10);
                    break;
            }
            transform.Rotate(transform.right, 90.0f);

            StartCoroutine(SetupDeath());
        }

        private IEnumerator SetupDeath()
        {
            yield return new WaitForSeconds(1.0f);

            _virtualCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = 0.0f;
            _virtualCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = 0.0f;
            _Camera.gameObject.SetActive(false);
            _virtualCamera.gameObject.SetActive(false);

            List<GameObject> allPlayers = GameManager.GetManager<MultiManager>().GetAllPlayerGameObjects();
            allPlayers.Remove(transform.parent.gameObject);
            GameObject DeathCamPlayer = allPlayers[UnityEngine.Random.Range(0, allPlayers.Count)];
            if (!DeathCamPlayer.Equals(gameObject))
            {
                DeathCamPlayer.GetComponent<PlayerBehaviour>().Camera.gameObject.SetActive(true);
                DeathCamPlayer.GetComponent<PlayerBehaviour>().VirtualCamera.gameObject.SetActive(true);
            }
            this.enabled = false;

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

        private void UseQuickSlot(float index)
        {
            Debug.Log("Use Quickslot");
            if ( inventory.QuickSlots[(int)index].ID == -1)return;
            ((ConsumableItem)GameManager.GetManager<ItemManager>().GetItem(inventory.QuickSlots[(int)index].ID)).Use(gameObject);
        }
        
        private void Attack()
        {
            if (_isAttacking) return;
            
            _isAttacking = true;


            if (inventory.MainHandItem != null)
            {
                animator.SetTrigger("IsAttacking");
                animatorFPS.SetTrigger("IsAttacking");
            }
            else{
                _isAttacking = false;
                Debug.Log("Can't Attack no weapon equipped");
            }
            
        }
        
        public void ResetIsAttacking()
        {
            _isAttacking = false;
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.yellow;
            //Gizmos.DrawWireSphere(_camTransform.position, 1.0f);
            Gizmos.DrawRay(_camTransform.position, _camTransform.TransformDirection(_camTransform.forward));
            //Gizmos.DrawSphere(transform.position, 1);
        }

        public event Action<bool> OnSwingStateChanged;
    }
}