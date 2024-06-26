using System;
using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using _2Scripts.Manager;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace _2Scripts
{
    public class Object : GameManagerSync<Object>, IInteractable
    {
        public Item ItemDetails;
        public int amount;
        public GameObject GOText;
        private ParticleSystem _vfx;
        [DoNotSerialize] public PlayerBehaviour playerBehaviourInspecting;

        protected override void Start()
        {
            base.Start();
            _vfx = GetComponentInChildren<ParticleSystem>();
        }

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if (gameState != GameState.InLevel)return;
            
            ParticleSystem.ColorOverLifetimeModule color = _vfx.colorOverLifetime;
            color.color = GameManager.GetManager<ItemManager>().GetGradientFromRarity(ItemDetails.Rarity);
        }

        private void Update()
        {
            if (!GOText.activeSelf || !playerBehaviourInspecting)
                return;

            GOText.transform.rotation = Quaternion.LookRotation(transform.position - playerBehaviourInspecting.transform.position, Vector3.up);
        }

        public void Interact()
        {
            // Pickup Object
            bool isItemAdded = playerBehaviourInspecting.inventory.AddToInventory(ItemDetails.ID, amount);
            if (isItemAdded) DespawnNetworkObjectRpc();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void DespawnNetworkObjectRpc()
        {
            NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();

            networkObject.Despawn(true);
        }
    }
}
