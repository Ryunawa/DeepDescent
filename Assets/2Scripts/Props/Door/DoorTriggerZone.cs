using UnityEngine;
using System.Collections;

public class DoorTriggerZone : MonoBehaviour
{
    public DoorController doorController;
    [SerializeField] public Collider doorCollider;
    [SerializeField] private bool isOpen;
    private int objectsInTrigger = 0;
    private Coroutine closeDoorCoroutine = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            objectsInTrigger++;
            if (!isOpen)
            {
                isOpen = true;
                doorController.OpenDoor();
                doorCollider.enabled = false;
                if (closeDoorCoroutine != null)
                {
                    StopCoroutine(closeDoorCoroutine);
                    closeDoorCoroutine = null;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            objectsInTrigger--;
            if (objectsInTrigger <= 0)
            {
                objectsInTrigger = 0;
                if (closeDoorCoroutine != null)
                {
                    StopCoroutine(closeDoorCoroutine);
                }
                closeDoorCoroutine = StartCoroutine(CloseDoorDelayed(5f));
            }
        }
    }

    private IEnumerator CloseDoorDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (objectsInTrigger <= 0 && isOpen)
        {
            isOpen = false;
            doorController.CloseDoor();
            doorCollider.enabled = true;
            closeDoorCoroutine = null;
        }
    }

    public void setIsOpen(bool isItOpen)
    {
        isOpen = isItOpen;
        doorCollider.enabled = !isItOpen;
    }
}
