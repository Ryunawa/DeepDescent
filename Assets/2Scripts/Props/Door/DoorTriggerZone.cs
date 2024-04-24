using UnityEngine;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using TMPro;

public class DoorTriggerZone : MonoBehaviour
{
    public DoorController doorController;

    [SerializeField] private GameObject[] texts;

    [SerializeField] private bool isOpen;
    [SerializeField] private bool canOpen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canOpen = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canOpen = false;
        }
    }

    private void Update()
    {
        if (!isOpen && canOpen)
        {
            foreach (GameObject text in texts)
            {
                text.SetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && canOpen && !isOpen)
        {
            isOpen = true;
            foreach (GameObject text in texts)
            {
                text.SetActive(false);
            }
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
