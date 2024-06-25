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
           
           // Remove All the previous generated room (props too)
           StartCoroutine(ClearPreviousDungeon());
           
           // Set bool depending on the current dungeon level
           GameManager.instance.levelGenerator.spawnShop = GameManager.GetManager<GameFlowManager>().CurrLevel % 5 == 0;
               
           // Start the generation
           //GameManager.instance.levelGenerator.StartGeneration();
           //NO NEED GAME STATE GENERATES IN STATE : generating. so we just call change state to generating
           
           ChangeGameStateRpc();
           
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void ChangeGameStateRpc()
        {
            GameManager.instance.ChangeGameState(GameState.Generating);
        }

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
        private IEnumerator ClearPreviousDungeon()
        {
            if (!GameManager.GetManager<MultiManager>().IsLobbyHost()) yield return false;
            
            NetworkObject[] objects = GameManager.instance.levelGenerator.roomsParent1.GetComponentsInChildren<NetworkObject>();
            for (var index = 0; index < objects.Length; index++)
            {
                NetworkObject networkObjectChild = objects[index];
                if (networkObjectChild.gameObject != GameManager.instance.levelGenerator.roomsParent1)
                    networkObjectChild.Despawn();
            }

            NetworkObject[] children = GameManager.instance.levelGenerator.propsParent1.GetComponentsInChildren<NetworkObject>();
            for (var index = 0; index < children.Length; index++)
            {
                NetworkObject networkObjectChild = children[index];
                if (networkObjectChild.gameObject != GameManager.instance.levelGenerator.propsParent1)
                    networkObjectChild.Despawn();
            }

            NetworkObject[] inChildren = GameManager.instance.levelGenerator.doorsParent1.GetComponentsInChildren<NetworkObject>();
            for (var index = 0; index < inChildren.Length; index++)
            {
                NetworkObject networkObjectChild = inChildren[index];
                if (networkObjectChild.gameObject != GameManager.instance.levelGenerator.doorsParent1)
                    networkObjectChild.Despawn();
            }

            ItemManager itemManager = GameManager.GetManager<ItemManager>();

            for (int i = 0; i < itemManager.ItemSpawned.Count; i++)
            {
                Destroy(itemManager.ItemSpawned[i]);
            }
            //TODO: clear stuff

            yield return true;
            Debug.Log("All dungeon elements clear");
        }
    }
}