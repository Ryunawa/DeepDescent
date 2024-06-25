using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;

namespace _2Scripts.TP
{
    public class PortalActivation : NetworkBehaviour
    {
        private bool isPlayerInRange = false;
        private HashSet<PlayerBehaviour> nearbyPlayers = new HashSet<PlayerBehaviour>();
        [SerializeField] private GameObject particleActivation;

        void Update()
        {
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                if (GameManager.GetManager<GameFlowManager>().CurrentState == GameFlowManager.LevelState.BossDefeated)
                {
                    ActivatePortal();
                }
            }
        }

        [Button]
        void DebugDefeatBoss()
        {
            GameManager.GetManager<GameFlowManager>().SetGameState(GameFlowManager.LevelState.BossDefeated);
            // visual effect
            particleActivation.SetActive(true);
            particleActivation.GetComponent<ParticleSystem>().Play();
        }

        private void ActivatePortal()
        {
            GameManager.GetManager<GameFlowManager>().SetGameState(GameFlowManager.LevelState.BossNotDiscovered);
            // Teleportation
            LoadNextLevelServerRpc();
        }

        [Rpc(SendTo.Server)]
        public void LoadNextLevelServerRpc()
        {
            GameManager.GetManager<GameFlowManager>().LoadNextLevelServer();

            ChangeGameStateRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void ChangeGameStateRpc()
        {
            GameManager.instance.ChangeGameState(GameState.Generating);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerBehaviour playerInRange = other.GetComponent<PlayerBehaviour>();
                if (playerInRange != null)
                {
                    nearbyPlayers.Add(playerInRange); // add player to the collection
                    isPlayerInRange = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerBehaviour playerInRange = other.GetComponent<PlayerBehaviour>();
                if (playerInRange != null && nearbyPlayers.Contains(playerInRange))
                {
                    nearbyPlayers.Remove(playerInRange); // remove player from the collection
                    if (nearbyPlayers.Count == 0)
                    {
                        isPlayerInRange = false;
                    }
                }
            }
        }
    }
}
