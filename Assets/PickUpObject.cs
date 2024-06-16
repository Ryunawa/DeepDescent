using _2Scripts.Entities.Player;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _2Scripts
{
    public class PickUpObject : MonoBehaviour
    {
        public PlayerBehaviour playerBehaviour;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Object obj))
            {
                playerBehaviour.ObjectToAddToInventory = other.gameObject;
                obj.GOText.SetActive(true);
                obj.playerBehaviourInspecting = playerBehaviour;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == playerBehaviour.ObjectToAddToInventory)
                playerBehaviour.ObjectToAddToInventory = null;

            if (other.TryGetComponent(out Object obj))
            {
                obj.GOText.SetActive(false);
                obj.playerBehaviourInspecting = null;
            }
        }
    }
}

