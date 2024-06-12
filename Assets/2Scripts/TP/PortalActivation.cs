using _2Scripts.Manager;
using NaughtyAttributes;
using UnityEngine;

public class PortalActivation : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private int playersInRangeCount = 0;
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
        MultiManager.instance.nextLevelManager.GenerateNewDungeon();
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
