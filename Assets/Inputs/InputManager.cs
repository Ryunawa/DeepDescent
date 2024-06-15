using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : Singleton<InputManager>
{
	private Inputs _inputs;
	private bool _isHoldingShoot = false;
	private bool _isHoldingAimDownSight = false;

	public Inputs Inputs => _inputs;

	public bool IsHoldingShoot
	{
		get => _isHoldingShoot; set => _isHoldingShoot = value;
	}
	public bool IsHoldingAimDownSight
	{
		get => _isHoldingAimDownSight; set => _isHoldingAimDownSight = value;
	}

	protected override void Awake()
	{
		base.Awake();

		_inputs = new Inputs();
		Cursor.visible = false;
	}

	private void Start()
	{
		_inputs.Player.Attack.performed += (InputAction) =>
		{
			_isHoldingShoot = true;
		};

		_inputs.Player.Attack.canceled += (InputAction) =>
		{
			_isHoldingShoot = false;
		};
	}

	private void OnEnable()
	{
		_inputs.Enable();
	}

	private void OnDisable()
	{
		_inputs.Disable();
	}

	public Vector2 GetPlayerMovement()
	{
		return _inputs.Player.Move.ReadValue<Vector2>();
	}

	public bool PlayerJumped()
	{
		return _inputs.Player.Jump.triggered;
	}

	public bool PlayerUsed()
	{
		return _inputs.Player.Use.triggered;
	}

	public bool PlayerAttacked()
	{
		return _inputs.Player.Attack.triggered;
	}

	public bool PlayerHoldDownFire()
	{
		return _inputs.Player.Attack.IsInProgress();
	}

	public bool PlayerCancelFire()
	{
		return _inputs.Player.Attack.WasReleasedThisFrame();
	}
}