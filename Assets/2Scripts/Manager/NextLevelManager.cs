using System.Threading.Tasks;
using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;

namespace _2Scripts.Manager
{
    public class NextLevelManager : NetworkBehaviour
    {
        /// <summary>
        /// Generate a new dungeon and clear the previous one
        /// </summary>
        public void GenerateNewDungeon()
        {
           // Show loading screen for players while dungeon generate (show loading screen from scene manager)
           ShowLoadingScreenClientRpc();
           
           // Stop the spawner
           EnemiesSpawnerManager.instance.StopSpawning();
           
           // Remove All the previous generated room (props too)
           ClearPreviousDungeon();
           
           // Set bool depending on the current dungeon level
           MultiManager.instance.levelGenerator.spawnShop = EnemiesSpawnerManager.instance.currLevel % 4 == 0;
               
           // Start the generation
           MultiManager.instance.levelGenerator.StartGeneration();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ShowLoadingScreenClientRpc()
        {
            SceneManager.instance.ActivateLoadingScreen();
        }
        
        private async void ClearPreviousDungeon()
        {
            foreach (var networkObjectChild in MultiManager.instance.levelGenerator.roomsParent1.GetComponentsInChildren<NetworkObject>())
            {
                if(networkObjectChild.gameObject != MultiManager.instance.levelGenerator.roomsParent1)
                    networkObjectChild.Despawn();
            }
            foreach (var networkObjectChild in MultiManager.instance.levelGenerator.propsParent1.GetComponentsInChildren<NetworkObject>())
            {
                if(networkObjectChild.gameObject != MultiManager.instance.levelGenerator.propsParent1)
                    networkObjectChild.Despawn();
            }
            foreach (var networkObjectChild in MultiManager.instance.levelGenerator.doorsParent1.GetComponentsInChildren<NetworkObject>())
            {
                if(networkObjectChild.gameObject != MultiManager.instance.levelGenerator.doorsParent1)
                    networkObjectChild.Despawn();
            }
            await Task.CompletedTask;
            Debug.Log("All dungeon elements clear");
        }
        
        // DEBUG ONLY
        [Button]
        public void DEBUG_GoToNextLevel()
        {
            if(MultiManager.instance.IsLobbyHost())
                GenerateNewDungeon();
        }
    }
}