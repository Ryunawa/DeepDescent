using Cinemachine;
using UnityEngine;
using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Entities.AI;
using _2Scripts.Manager;

public class BossPillarInteraction : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private HashSet<PlayerBehaviour> nearbyPlayers = new HashSet<PlayerBehaviour>();
    [SerializeField] private GameObject bossPrefab;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && GameManager.GetManager<GameFlowManager>().CurrentState == GameFlowManager.LevelState.BossNotDiscovered)
        {
            ActivatePillar();
        }
    }

    private void ActivatePillar()
    {
        // Play Music
        GameManager.GetManager<AudioManager>().PlayMusic("BossMusic", 0.1f);

        CinemachineImpulseSource[] impulseSources = FindObjectsOfType<CinemachineImpulseSource>();
        foreach (CinemachineImpulseSource impulseSource in impulseSources)
        {
            // Impulsion parameters
            impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Rumble;
            impulseSource.m_ImpulseDefinition.m_AmplitudeGain = 0.2f;
            impulseSource.m_ImpulseDefinition.m_FrequencyGain = 50.0f;
            impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = 3.0f;
            impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = 0.5f;
            impulseSource.m_ImpulseDefinition.m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Dissipating;

            // Velocity
            impulseSource.m_DefaultVelocity = Vector3.one * 0.1f;

            // Impulsion
            impulseSource.GenerateImpulse(3f);
        }

        // Boss is coming
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z + 5);
        GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        // tell that he is the boss>
        boss.GetComponent<AIController>().isBoss = true;
        boss.transform.localScale *= 2; // make him bigger

        GameManager.GetManager<GameFlowManager>().SetGameState(GameFlowManager.LevelState.BossInProgress);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBehaviour playerInRange = other.GetComponent<PlayerBehaviour>();
            if (playerInRange != null)
            {
                nearbyPlayers.Add(playerInRange); // add player to collection
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
                nearbyPlayers.Remove(playerInRange); // remove player from collection
                if (nearbyPlayers.Count == 0)
                {
                    isPlayerInRange = false;
                }
            }
        }
    }
}
