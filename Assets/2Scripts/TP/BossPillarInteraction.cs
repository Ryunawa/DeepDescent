using Cinemachine;
using UnityEngine;

public class BossPillarInteraction : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private int playersInRangeCount = 0;
    [SerializeField] private GameObject bossPrefab;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && GameFlowManager.instance.CurrentState == GameFlowManager.GameState.BossNotDiscovered)
        {
            ActivatePillar();
        }
    }

    private void ActivatePillar()
    {
        CinemachineImpulseSource[] impulseSources = FindObjectsOfType<CinemachineImpulseSource>();
        foreach (CinemachineImpulseSource impulseSource in impulseSources)
        {
            // impulsion parameters
            impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Rumble;
            impulseSource.m_ImpulseDefinition.m_AmplitudeGain = 0.2f;
            impulseSource.m_ImpulseDefinition.m_FrequencyGain = 50.0f;
            impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = 3.0f;
            impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = 0.5f;
            impulseSource.m_ImpulseDefinition.m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Dissipating;

            // velocity
            impulseSource.m_DefaultVelocity = Vector3.one * 0.1f;

            // impulsion
            impulseSource.GenerateImpulse(3f);
        }

        // boss is coming
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z + 5);
        GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        // TODO -> tell that he is the boss
        // boss.gameObject.GetComponent<AIController>.isBoss = true;
        boss.transform.localScale *= 2; // make him bigger

        GameFlowManager.instance.SetGameState(GameFlowManager.GameState.BossInProgress);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInRangeCount++;
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInRangeCount--;

            if (playersInRangeCount <= 0)
            {
                isPlayerInRange = false;
            }
        }
    }
}
