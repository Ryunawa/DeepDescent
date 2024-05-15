using Cinemachine;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBehaviour : NetworkBehaviour
{
	[SerializeField] private Vector2 camSens = new(100, 100);
	[SerializeField] private float playerSpeed = 2.0f;
	[SerializeField] private float jumpHeight = 1.0f;
	[SerializeField] private float airControl = 0;
	[SerializeField] private Animator animator;
	[SerializeField] private Transform _camTransform;
	[SerializeField] private CinemachineVirtualCamera _virtualCamera;
	
	private Rigidbody _rb;
	private InputManager _inputManager;
	private bool _hasJumped = false;
	private Collider _ground = null;

	private HealthComponent _health;
	[SerializeField]private NetworkVariable<bool> _isDead = new NetworkVariable<bool>();

	public NetworkVariable<bool> IsDead { get => _isDead; set => _isDead = value; }
    public Inventory inventory;
    private void Start()
	{
		_health = GetComponent<HealthComponent>();

		_health.OnDeath.AddListener(OnDie);
		_health.OnDamaged.AddListener(OnDamaged);

		_rb = GetComponent<Rigidbody>();
		_inputManager = InputManager.instance;
		
		_virtualCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = camSens.x * 0.01f;
		_virtualCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = camSens.y * 0.01f;
		
		if (!IsOwner) 
		{
			_camTransform.gameObject.SetActive(false);
			_virtualCamera.gameObject.SetActive(false);
			enabled = false;
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
		if (IsGrounded() && _inputManager.PlayerJumped())
			_hasJumped = true;
		
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

		if (!IsGrounded())
			move *= airControl;

		transform.rotation = Quaternion.Euler(0, transform.rotation.y, 0);
		
		_rb.velocity = move * playerSpeed + (_rb.velocity.y * Vector3.up);

		gameObject.transform.rotation = Quaternion.Euler(0,_camTransform.eulerAngles.y,0);
		
		if (_hasJumped)
		{
			_hasJumped = false;
			_rb.AddForce(Vector3.up * jumpHeight, ForceMode.VelocityChange);
		}

		animator.SetFloat("Speed", _rb.velocity.magnitude * 2);
	}

	private bool IsGrounded()
	{
		return _ground != null;
	}

	private void OnCollisionEnter(Collision iCollision)
	{
		if (Vector3.Dot(iCollision.GetContact(0).normal, Vector3.up) > 0.8f)
			_ground = iCollision.collider;
	}

	private void OnCollisionExit(Collision iCollision)
	{
		if (iCollision.collider == _ground)
			_ground = null;
	}

	private void OnDamaged(int damage)
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
}