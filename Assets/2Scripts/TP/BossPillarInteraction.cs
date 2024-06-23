using Cinemachine;
using UnityEngine;
using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Entities.AI;
using _2Scripts.Manager;
using _2Scripts.ProceduralGeneration;

public class BossPillarInteraction : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private HashSet<PlayerBehaviour> nearbyPlayers = new HashSet<PlayerBehaviour>();
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject pillarFx;

    [SerializeField] public Room roomTp;

    private void Start()
    {
        if (pillarFx != null)
        {
            pillarFx.GetComponent<ParticleSystem>().Stop();
            pillarFx.SetActive(true);
            pillarFx.GetComponent<ParticleSystem>().Play();
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && GameManager.GetManager<GameFlowManager>().CurrentState == GameFlowManager.LevelState.BossNotDiscovered)
        {
            ActivatePillar();
        }

        if (GameManager.GetManager<GameFlowManager>().CurrentState == GameFlowManager.LevelState.BossDefeated)
        {
            if (pillarFx != null)
            {
                pillarFx.SetActive(true);
            }

            // Play Music
            GameManager.GetManager<AudioManager>().PlayMusic("InsideTheDungeonMusic", 0.1f);
        }
    }

    private void ActivatePillar()
    {
        // Play Music
        GameManager.GetManager<AudioManager>().PlayMusic("BossMusic", 0.1f);

        if (pillarFx != null)
        {
            pillarFx.SetActive(false);
            pillarFx.GetComponent<ParticleSystem>().Stop();
        }

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

        // spawn the boss
        EnemiesSpawnerManager.instance.SpawnBossEnemy(roomTp);

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
