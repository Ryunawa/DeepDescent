using _2Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private int playersInRangeCount = 0;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ActivateDoor();
        }
    }

    private void ActivateDoor()
    {
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
