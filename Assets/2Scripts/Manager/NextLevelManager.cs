using System;
using System.Threading.Tasks;
using _2Scripts.Helpers;
using _2Scripts.Interfaces;
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
           ClearPreviousDungeon();
           
           // Set bool depending on the current dungeon level
           GameManager.instance.levelGenerator.spawnShop = GameManager.GetManager<GameFlowManager>().CurrLevel % 4 == 0;
               
           // Start the generation
           GameManager.instance.levelGenerator.StartGeneration();
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
        private async void ClearPreviousDungeon()
        {
            foreach (var networkObjectChild in GameManager.instance.levelGenerator.roomsParent1.GetComponentsInChildren<NetworkObject>())
            {
                if(networkObjectChild.gameObject != GameManager.instance.levelGenerator.roomsParent1)
                    networkObjectChild.Despawn();
            }
            foreach (var networkObjectChild in GameManager.instance.levelGenerator.propsParent1.GetComponentsInChildren<NetworkObject>())
            {
                if(networkObjectChild.gameObject != GameManager.instance.levelGenerator.propsParent1)
                    networkObjectChild.Despawn();
            }
            foreach (var networkObjectChild in GameManager.instance.levelGenerator.doorsParent1.GetComponentsInChildren<NetworkObject>())
            {
                if(networkObjectChild.gameObject != GameManager.instance.levelGenerator.doorsParent1)
                    networkObjectChild.Despawn();
            }
            await Task.CompletedTask;
            Debug.Log("All dungeon elements clear");
        }
    }
}