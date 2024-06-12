using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using NaughtyAttributes;
using UnityEngine;

namespace _2Scripts.TP
{
    public class PortalActivation : MonoBehaviour
    {
        private bool isPlayerInRange = false;
        private HashSet<PlayerBehaviour> nearbyPlayers = new HashSet<PlayerBehaviour>();
        [SerializeField] private GameObject particleActivation;

        void Update()
        {
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                if (GameFlowManager.instance.CurrentState == GameFlowManager.GameState.BossDefeated)
                {
                    ActivatePortal();
                }
            }
        }

        [Button]
        void DebugDefeatBoss()
        {
            GameFlowManager.instance.SetGameState(GameFlowManager.GameState.BossDefeated);
            // visual effect
            particleActivation.SetActive(true);
            particleActivation.GetComponent<ParticleSystem>().Play();
        }

        private void ActivatePortal()
        {
            GameFlowManager.instance.SetGameState(GameFlowManager.GameState.BossNotDiscovered);
            // Teleportation
            GameFlowManager.instance.LoadNextLevel();
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
