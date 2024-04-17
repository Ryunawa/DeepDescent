using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NaughtyAttributes;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Player = Unity.Services.Lobbies.Models.Player;
using DataObject = Unity.Services.Lobbies.Models.DataObject;

public class MultiManager : Singleton<MultiManager>
{
	[SerializeField] private float heartBeatFrequency = 15f;

	private string _playerName;
	private Lobby _lobby;
	private bool _IsOwnerOfLobby = false;
	private ILobbyEvents _lobbyEvents;
	private LobbyEventCallbacks _lobbyEventCallbacks = new LobbyEventCallbacks();

	private string ServerIp;
	private ushort ServerPort;

	public UnityEvent lobbyCreated;
	public UnityEvent lobbyJoined;
	public UnityEvent kickedEvent;
	public UnityEvent refreshUI;
	public UnityEvent init;

	public string PlayerName
	{
		get => _playerName;
		set => _playerName = value;
	}

	public Lobby Lobby
	{
		get => _lobby;
	}

	protected override async void OnDestroy()
	{
		base.OnDestroy();

		if(_lobby != null)
		{
			await LeaveLobbyAsync();
		}
	}
	
	public bool IsLobbyHost()
	{
		return _IsOwnerOfLobby;
	}

	public async void Init()
	{
		var options = new InitializationOptions();
		options.SetProfile(_playerName);

		if(UnityServices.State != ServicesInitializationState.Initialized)
			await UnityServices.InitializeAsync(options);
		if(!AuthenticationService.Instance.IsSignedIn)
			await AuthenticationService.Instance.SignInAnonymouslyAsync();

		Debug.Log(AuthenticationService.Instance.PlayerId);

		_lobbyEventCallbacks.LobbyChanged += OnLobbyChanged;
		_lobbyEventCallbacks.KickedFromLobby += OnKickedFromLobby;
		_lobbyEventCallbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

		init.Invoke();
	}

	private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState obj)
	{
		Debug.Log("StateChange");
	}

	private void OnKickedFromLobby()
	{
		Debug.Log("Kicked");
		_lobby = null;
		kickedEvent.Invoke();
	}
	private void OnLobbyChanged(ILobbyChanges lobbyChanges)
	{
		Debug.Log("Lobby changed");
		lobbyChanges.ApplyToLobby(_lobby);

		if (_lobby.Data["startGame"].Value != "0")
		{
			MultiManager.instance.JoinRelay(_lobby.Data["startGame"].Value);
		}
		
		
		refreshUI.Invoke();
	}
	

	public async Task JoinLobby(String joinCode, bool isLobbyCode = false)
	{
		try
		{
			if(isLobbyCode)
			{
				JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
				{
					Player = GetPlayer()
				};
				_lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, joinLobbyByCodeOptions);
				lobbyJoined.Invoke();
			}
			else
			{
				JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
				{
					Player = GetPlayer()
				};
				_lobby = await LobbyService.Instance.JoinLobbyByIdAsync(joinCode, joinLobbyByIdOptions);
				lobbyJoined.Invoke();
			}

			SubToLobbyEvents();

		}
		catch(LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	public async Task<QueryResponse> GetAllLobbies()
	{
		QueryResponse lobbies = null;
		try
		{
			QueryLobbiesOptions options = new QueryLobbiesOptions();
			options.Count = 25;

			// Filter for open lobbies only
			options.Filters = new List<QueryFilter>()
			{
				new QueryFilter(
					field: QueryFilter.FieldOptions.AvailableSlots,
					op: QueryFilter.OpOptions.GT,
					value: "0")
			};

			// Order by newest lobbies first
			options.Order = new List<QueryOrder>()
			{
				new QueryOrder(
					asc: false,
					field: QueryOrder.FieldOptions.Created)
			};

			lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
		}
		catch(LobbyServiceException e)
		{
			Debug.Log(e);
		}

		return lobbies;
	}

	[Button("CreateLobby")]
	public async void CreateLobby()
	{
		string lobbyName = _playerName + "'s Lobby";
		int maxPlayers = 4;
		CreateLobbyOptions options = new CreateLobbyOptions();
		options.IsPrivate = false;
		options.Player = GetPlayer();
		options.Data = new Dictionary<string, DataObject>
		{
			{"startGame", new DataObject(DataObject.VisibilityOptions.Member, "0")} 
		};

		try
		{
			if(_lobby == null)
			{
				_lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
				_IsOwnerOfLobby = true;

				StartCoroutine(LobbyHeartBeat());
				SubToLobbyEvents();

				lobbyCreated.Invoke();
			}
		}
		catch(Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}

	private async void SubToLobbyEvents()
	{
		try
		{
			_lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(_lobby.Id, _lobbyEventCallbacks);
		}
		catch(LobbyServiceException ex)
		{
			switch(ex.Reason)
			{
				case LobbyExceptionReason.AlreadySubscribedToLobby:
					Debug.LogWarning($"Already subscribed to lobby[{MultiManager.instance.Lobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}");
					break;
				case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy:
					Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}");
					throw;
				case LobbyExceptionReason.LobbyEventServiceConnectionError:
					Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}");
					throw;
				default:
					throw;
			}
		}
	}

	private async Task UnSubToLobbyEvents()
	{
		try
		{
			Debug.Log("Unsubing from Lobby events");
			await _lobbyEvents.UnsubscribeAsync();
		}
		catch(Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}

	public async void LeaveLobby()
	{
		await LeaveLobbyAsync();
	}

	public async Task LeaveLobbyAsync()
	{
		Debug.Log("LeavingLobby");

		if(_lobby == null)
		{
			return;
		}

		await UnSubToLobbyEvents();

		if(_IsOwnerOfLobby)
		{
			foreach(var player in _lobby.Players)
			{
				if(player.Id != AuthenticationService.Instance.PlayerId)
				{
					try
					{
						await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, player.Id);
						Debug.Log($"Kicked {player.Id}");
					}
					catch { }
				}
			}

			Debug.Log("Kicked all players");


			try
			{
				await LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
				Debug.Log("Deleted Lobby");
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

		}
		else
		{
			try
			{
				await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, AuthenticationService.Instance.PlayerId);
				Debug.Log("Removed self from lobby");
			}
			catch
			{
				Debug.LogWarning("Could not remove player from lobby");
			}
		}

		//Calls Kicked Event for host and kickes clients

		_lobby = null;
		kickedEvent.Invoke();
	}

	private IEnumerator LobbyHeartBeat()
	{
		while(_lobby != null)
		{
			LobbyService.Instance.SendHeartbeatPingAsync(_lobby.Id);

			yield return new WaitForSeconds(heartBeatFrequency);
		}
	}

	private Player GetPlayer()
	{
		return new Player(
			id: AuthenticationService.Instance.PlayerId,
			data: new Dictionary<string, PlayerDataObject>
			{
				{ "Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
			}
		);
	}

	private async Task<string> CreateRelay()
	{
		try
		{
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
			
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
			NetworkManager.Singleton.StartHost();

			return joinCode;

		}
		catch (RelayServiceException e)
		{
			Debug.LogError(e);
			throw;
		}
		
	}

	private async void StartGame()
	{
		if (_IsOwnerOfLobby)
		{
			try
			{
				string relayCode = await CreateRelay();

				await Lobbies.Instance.UpdateLobbyAsync(_lobby.Id, new UpdateLobbyOptions
				{
					Data = new Dictionary<string, DataObject>
					{
						{"startGame", new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
					}
				});
			}
			catch (LobbyServiceException e)
			{
				Debug.LogError(e);
				throw;
			}
		}
	}

	private async void JoinRelay(string joinCode)
	{
		try
		{
			JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

			RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
			NetworkManager.Singleton.StartClient();

		}
		catch (RelayServiceException e)
		{
			Console.WriteLine(e);
			throw;
		}
		
	}
}
