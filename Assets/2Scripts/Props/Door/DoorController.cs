using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator doorAnimator;
    public float autoCloseDelay = 5f;

    private bool playerNearby = false;

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.Return))
        {
            OpenDoor();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    void OpenDoor()
    {
        doorAnimator.SetTrigger("Open");

        StartCoroutine(AutoCloseDoor());
    }

    IEnumerator AutoCloseDoor()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        doorAnimator.SetTrigger("Close");
    }
}
