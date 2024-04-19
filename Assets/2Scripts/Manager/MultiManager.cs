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
	[SerializeField] private Canvas mainMenu;
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

	
	/// <summary>
	/// This method initiates the unity services and authenticates the user. It also adds listeners for the lobby events
	/// </summary>
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
		
		NetworkManager.Singleton.OnConnectionEvent += (manager, data) =>
		{
			Debug.Log("Client connected");
			if (!_IsOwnerOfLobby)
			{
				HideMainMenu();
			}
		} ;

	}

	/// <summary>
	/// This is the OnStateChanges event callback for the lobby
	/// </summary>
	/// <param name="obj"></param>
	private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState obj)
	{
		Debug.Log("StateChange");
	}

	/// <summary>
	/// This is the OnKickedFromLobby event callback for the lobby
	/// </summary>
	private void OnKickedFromLobby()
	{
		Debug.Log("Kicked");
		_lobby = null;
		kickedEvent.Invoke();
	}
	
	/// <summary>
	/// This is the OnLobbyChanged event callback, it is used to sync the lobby data across clients and host.
	/// It launches the game for the client as soon as  the Relay code is received
	/// </summary>
	/// <param name="lobbyChanges"></param>
	private void OnLobbyChanged(ILobbyChanges lobbyChanges)
	{
		Debug.Log("Lobby changed");
		lobbyChanges.ApplyToLobby(_lobby);

		if (_lobby.Data["startGame"].Value != "0" && !_IsOwnerOfLobby)
		{
			MultiManager.instance.JoinRelay(_lobby.Data["startGame"].Value);
		}
		
		refreshUI.Invoke();
	}
	
	/// <summary>
	/// This async Task is used for joining a lobby with a given lobby code or lobby ID,
	/// once the lobby is joined it checks for the Relay ID, joins the relay if it exists and launches the game 
	/// </summary>
	/// <param name="joinCode"> lobby ID or Code </param>
	/// <param name="isLobbyCode">is join method lobby Code</param>
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
			
			if (_lobby.Data["startGame"].Value != "0" && !_IsOwnerOfLobby)
			{
				MultiManager.instance.JoinRelay(_lobby.Data["startGame"].Value);
			}

			SubToLobbyEvents();

		}
		catch(LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	/// <summary>
	/// Returns all open existing lobbies
	/// </summary>
	/// <returns></returns>
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

	/// <summary>
	/// Used for creating a lobby, the lobby name is the player's name, it wil create a 4 place lobby
	/// </summary>
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

	/// <summary>
	/// Called to sub to lobby events 
	/// </summary>
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

	/// <summary>
	/// Called to unsub from lobby events
	/// </summary>
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

	/// <summary>
	/// Called to leave a lobby
	/// </summary>
	public async void LeaveLobby()
	{
		await LeaveLobbyAsync();
	}

	/// <summary>
	/// This Task is used to leave the lobby you are currently in. it will also unsub from events.
	/// IF you are host it will kick every other player in lobby
	/// </summary>
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

	/// <summary>
	/// This will send a virtual heartbeat to the lobby in order to keep it alive, if all players quit the lobby will die 
	/// </summary>
	/// <returns></returns>
	private IEnumerator LobbyHeartBeat()
	{
		while(_lobby != null)
		{
			LobbyService.Instance.SendHeartbeatPingAsync(_lobby.Id);

			yield return new WaitForSeconds(heartBeatFrequency);
		}
	}

	/// <summary>
	/// gets the player object, with ID and name
	/// </summary>
	/// <returns> player object</returns>
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

	/// <summary>
	/// This task is used to create a relay
	/// </summary>
	/// <returns>Relay join code</returns>
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

	/// <summary>
	/// Used by host to create and join Relay. This also updates lobby with Relay code
	/// </summary>
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
			
			LoadGameScene();
		}
	}

	/// <summary>
	/// This is called by non host clients to join the same relay as the host 
	/// </summary>
	/// <param name="joinCode">Relay join Code</param>
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
	
	/// <summary>
	/// This is used by host to change scenes for everyone joining including himself.
	/// </summary>
	private void LoadGameScene()
	{
		string sceneName = "";
		if (PlayerPrefs.GetInt("Level", -1) == -1)
		{
			sceneName = "Level";
		}
		else
		{
			sceneName = "SafeZone";
		}
		NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
		HideMainMenu();
	}
	
	/// <summary>
	/// called by clients to hide the main menu UIs 
	/// </summary>
	private void HideMainMenu()
	{
		mainMenu.gameObject.SetActive(false);
	}
}
