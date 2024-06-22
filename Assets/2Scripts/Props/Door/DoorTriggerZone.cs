using UnityEngine;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using TMPro;

public class DoorTriggerZone : MonoBehaviour
{
    public DoorController doorController;
    [SerializeField] private bool isOpen;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Enemy")) && !isOpen)
        {
            isOpen = true;
            doorController.OpenDoor();
            StartCoroutine(CloseDoorDelayed(5f));
        }
    }

    private IEnumerator CloseDoorDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        doorController.CloseDoor();
    }

    public void setIsOpen(bool isItOpen)
    {
        isOpen = isItOpen;
    }
}
