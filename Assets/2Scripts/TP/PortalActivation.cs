using NaughtyAttributes;
using UnityEngine;

public class PortalActivation : MonoBehaviour
{
    private bool isPlayerInRange = false;
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
        Debug.Log("clicked: " + GameFlowManager.GameState.BossDefeated);
    }

    private void ActivatePortal()
    {
        Debug.Log("Portal activated");
        // visual effect
        particleActivation.SetActive(true);
        particleActivation.GetComponent<ParticleSystem>().Play();
        // TODO: Teleportation
        GameFlowManager.instance.SetGameState(GameFlowManager.GameState.BossNotDiscovered);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
