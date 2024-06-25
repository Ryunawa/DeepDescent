using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _2Scripts.Entities.Player;
using _2Scripts.ProceduralGeneration;
using Unity.Netcode;
using Unity.Services.Qos.V2.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace _2Scripts.Manager
{
    public class GameManager : Singleton<GameManager>
    {
        private static GameState _gameState;

        public Timer.Timer timer;

        private static int numberOfDeadPlayers;

        public static GameState GameState => _gameState;

        [Header("Managers")]
        [SerializeField]private GameObject  multiManagerPrefab;
        [SerializeField]private GameObject inventoryUIManagerPrefab;
        [SerializeField]private GameObject gameFlowManagerPrefab;
        [SerializeField]private GameObject itemManagerPrefab;
        [SerializeField]private GameObject  sceneManagerPrefab;
        [SerializeField]private GameObject  difficultyManagerPrefab;
        [SerializeField]private GameObject  lightManagerPrefab;
        [SerializeField]private GameObject enemiesSpawnerManagerPrefab;
        [SerializeField]private GameObject  audioManagerPrefab;
        
        [Header("Other")]
        [SerializeField] private GameObject endGameUI;

        private static ManagerObject[] _managersAndPrefabs = {};
        private static Coroutine _stateChangeCoroutine;

        public static ManagerObject[] ManagersAndPrefabs => _managersAndPrefabs;

        public static UnityEvent<GameState> stateChanged = new ();
        
        #region Data and objects

        public LevelGenerator levelGenerator;
        public NextLevelManager nextLevelManager;
        public static PlayerBehaviour playerBehaviour;
        
        #endregion
        

        protected override void Awake()
        {
            base.Awake();
            
            _managersAndPrefabs = new ManagerObject[]
            {
                new (ManagerType.Multi, null,multiManagerPrefab),
                new (ManagerType.Scene, null,sceneManagerPrefab),
                new (ManagerType.UI, null,inventoryUIManagerPrefab),
                new (ManagerType.GameFlow, null,gameFlowManagerPrefab),
                new (ManagerType.Item, null,itemManagerPrefab),
                new (ManagerType.Difficulty, null,difficultyManagerPrefab),
                new (ManagerType.Light, null,lightManagerPrefab),
                new (ManagerType.Enemy, null,enemiesSpawnerManagerPrefab),
                new (ManagerType.Audio, null,audioManagerPrefab)
            };
        }

        public void AddADeadPlayer()
        {
            if (!GetManager<MultiManager>().IsLobbyHost())return;
            
            numberOfDeadPlayers++;
            Debug.Log("numberOfDeadPlayers : " + numberOfDeadPlayers);

            if (numberOfDeadPlayers >= NetworkManager.Singleton.ConnectedClients.Count)
            {
                if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
                {
                    EndGameRpc();
                }
            }
        }

        public void ResetNumberOfDeadPlayer()
        {
            numberOfDeadPlayers = 0;
            Debug.Log("numberOfDeadPlayers : " + numberOfDeadPlayers);
        }


        [Rpc(SendTo.ClientsAndHost)]
        private void EndGameRpc()
        {
            Debug.Log("Game Over. Returning to main menu.");
            endGameUI = GetManager<SceneManager>().gameObject.transform.GetChild(1).gameObject;
            endGameUI.SetActive(true);
            timer = FindObjectOfType<Timer.Timer>();
            timer.StopTimer();

            // Activate the cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }


        private void Start()
        {
           ChangeGameState(GameState.MainMenu);
           
           stateChanged.AddListener(arg0 => Debug.Log($"Changed GameState to {arg0}"));
        }

        /// <summary>
        /// Used to change game state, edit the needed Managers list according to your needs
        /// </summary>
        /// <param name="gameState"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ChangeGameState(GameState gameState)
        {
            List<ManagerType> neededManagers;
            switch (gameState)
            {
                case GameState.MainMenu:
                 
                    neededManagers = new List<ManagerType>
                    {
                        ManagerType.Multi, ManagerType.Audio, ManagerType.Scene
                    };
                 
                    ActivateNeededManagers(neededManagers);
                 
                    break;
                
                case GameState.Lobby:
                    stateChanged.Invoke(gameState);
                    break;
                
                case GameState.Loading:
                    stateChanged.Invoke(gameState);
                    break;
                
                case GameState.InLevel:

                    neededManagers = new List<ManagerType>
                    {
                        ManagerType.UI, ManagerType.Enemy
                    };
                     
                    ActivateNeededManagers(neededManagers, true);
                 
                    break;
                
                case GameState.Generating:
                    neededManagers = new List<ManagerType>
                    {
                        ManagerType.GameFlow, ManagerType.Item, ManagerType.Difficulty, ManagerType.Light
                    };
                    
                    ActivateNeededManagers(neededManagers, true);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(gameState), gameState, null);
            }

            _gameState = gameState;
        }

        private void ActivateNeededManagers(List<ManagerType> neededManagers, bool isAdditive = false)
        {
            _stateChangeCoroutine = StartCoroutine(SetManagers(neededManagers, isAdditive));
        }

        private IEnumerator SetManagers(List<ManagerType> neededManagers, bool isAdditive = false)
        {
            for (var index = 0; index < _managersAndPrefabs.Length; index++)
            {
                ManagerObject managerObject = _managersAndPrefabs[index];

                switch (neededManagers.Contains(managerObject.ManagerType))
                {
                    case true when !managerObject.ManagerMonoBehaviour:
                        
                        Instantiate(managerObject.ManagerPrefab,
                            gameObject.transform).TryGetComponent(out managerObject.ManagerMonoBehaviour);
                        Debug.Log($"Instanciated {managerObject.ManagerType} manager");
                        yield return true;
                     
                        break;
                 
                    case true:
                        managerObject.ManagerMonoBehaviour.gameObject.SetActive(true);
                        break;
                 
                    case false when !isAdditive && managerObject.ManagerMonoBehaviour:
                        managerObject.ManagerMonoBehaviour.gameObject.SetActive(false);
                        break;
                    default:
                        break;
                }

                _managersAndPrefabs[index] = managerObject;
            }

            stateChanged.Invoke(_gameState);
            
            yield return true;
        }

        /// <summary>
        /// Gets the manager of the type given 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetManager<T>() where T : class
        {
             return _managersAndPrefabs.First(x => x.ManagerMonoBehaviour is T).ManagerMonoBehaviour as T;
        }
        
        /// <summary>
        /// Get the component in the local player 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetPlayerComponent<T>() where T : class
        {
            return playerBehaviour.GetComponentInChildren<T>();
        }
    }

    public struct ManagerObject
    {
        public readonly ManagerType ManagerType;
        public MonoBehaviour ManagerMonoBehaviour;
        public readonly GameObject ManagerPrefab;

        public ManagerObject(ManagerType managerType, MonoBehaviour managerMonoBehaviour, GameObject managerPrefab)
        {
            ManagerType = managerType;
            ManagerMonoBehaviour = managerMonoBehaviour;
            ManagerPrefab = managerPrefab;
        }
    }


    public enum ManagerType
    {
        Multi,
        Scene,
        UI,
        GameFlow,
        Item,
        Difficulty,
        Light,
        Enemy,
        Audio
    }

    public enum GameState
    {
        MainMenu,
        Lobby,
        Loading,
        Generating,
        InLevel
    }
}