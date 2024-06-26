using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using _2Scripts.ProceduralGeneration;
using Unity.Netcode;
using UnityEngine;

namespace _2Scripts.Manager
{
    public class NextLevelManager : GameManagerSync<NextLevelManager>
    {
        private void OnDisable()
        {
            GameManager.GetManager<GameFlowManager>().OnNextLevelEvent.RemoveListener(GenerateNewDungeon);
        }

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if (gameState!= GameState.InLevel)return;
            
            GameManager.instance.nextLevelManager = this;
            GameManager.GetManager<GameFlowManager>().OnNextLevelEvent.AddListener(GenerateNewDungeon);
        }

        /// <summary>
        /// Generate a new dungeon and clear the previous one
        /// </summary>
        private void GenerateNewDungeon(Timer.Timer pTimer)
        {
           // Show loading screen for players while dungeon generate (show loading screen from scene manager)
           ShowLoadingScreenClientRpc();
           
           // Stop the spawner
           GameManager.GetManager<EnemiesSpawnerManager>().StopSpawning();
           GameManager.ManagersAndPrefabs[7].ManagerMonoBehaviour = null;
           StartCoroutine(ClearPreviousDungeon(() =>
           {
               GameManager.GetManager<SceneManager>().LoadSceneNetwork(Scenes.Level);
               GameManager.instance.levelGenerator.spawnShop = GameManager.GetManager<GameFlowManager>().CurrLevel % 5 == 0;
           }));
           
           
           //ChangeGameStateRpc();
           
           
           // Set bool depending on the current dungeon level
           
           
           // Remove All the previous generated room (props too)
           
               
           // Start the generation
           //GameManager.instance.levelGenerator.StartGeneration();
           //NO NEED GAME STATE GENERATES IN STATE : generating. so we just call change state to generating
           
        }
        
        // [Rpc(SendTo.ClientsAndHost)]
        // public void ChangeGameStateRpc()
        // {
        //     if (GameManager.GameState != GameState.Generating)
        //     {
        //         GameManager.instance.ChangeGameState(GameState.Generating);
        //     }
        // }

        /// <summary>
        /// Show the loading screen for all clients and host
        /// </summary>
        [Rpc(SendTo.ClientsAndHost)]
        private void ShowLoadingScreenClientRpc()
        {
            GameManager.GetManager<SceneManager>().ActivateLoadingScreen();
        }
        
        /// <summary>
        /// Despawn the props, rooms and doors of the current dungeon
        /// </summary>
        private IEnumerator ClearPreviousDungeon(Action action)
        {
            if (!GameManager.GetManager<MultiManager>().IsLobbyHost()) yield return false;
            
            NetworkObject[] objects = GameManager.instance.levelGenerator.roomsParent1.GetComponentsInChildren<NetworkObject>();
            for (var index = 0; index < objects.Length; index++)
            {
                if (objects.Length == 0) break;
                NetworkObject networkObjectChild = objects[index];
                if (networkObjectChild.gameObject != GameManager.instance.levelGenerator.roomsParent1)
                    networkObjectChild.Despawn();
            }

            NetworkObject[] children = GameManager.instance.levelGenerator.propsParent1.GetComponentsInChildren<NetworkObject>();
            for (var index = 0; index < children.Length; index++)
            {
                if (children.Length == 0) break;
                NetworkObject networkObjectChild = children[index];
                if (networkObjectChild.gameObject != GameManager.instance.levelGenerator.propsParent1)
                    networkObjectChild.Despawn();
            }

            NetworkObject[] inChildren = GameManager.instance.levelGenerator.doorsParent1.GetComponentsInChildren<NetworkObject>();
            for (var index = 0; index < inChildren.Length; index++)
            {
                if (inChildren.Length == 0) break;
                NetworkObject networkObjectChild = inChildren[index];
                if (networkObjectChild.gameObject != GameManager.instance.levelGenerator.doorsParent1)
                    networkObjectChild.Despawn();
            }

            ItemManager itemManager = GameManager.GetManager<ItemManager>();

            for (int i = 0; i < itemManager.ItemSpawned.Count; i++)
            {
                if (itemManager.ItemSpawned.Count == 0) break;
                
                itemManager.ItemSpawned[i].GetComponent<NetworkObject>().Despawn();
            }
            
            GameManager.instance.levelGenerator.Portal.Despawn();

            action.Invoke();
            yield return true;
            Debug.Log("All dungeon elements clear");
        }
    }
}