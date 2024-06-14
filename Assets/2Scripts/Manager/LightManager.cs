using System.Collections.Generic;
using System.Threading.Tasks;
using _2Scripts.Entities.Player;
using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using UnityEngine;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace _2Scripts.Manager
{
    public class LightManager : GameManagerSync<LightManager>
    {
        [SerializeField] private float lightDistance;
        [SerializeField] private int checkIntervalMS;
        [SerializeField] private GameObject player;
        private List<GameObject> _lights = new List<GameObject>();

        private MultiManager _multiManager;

        public void AddToLightsList(GameObject gameObjectLight)
        {
            if (!_lights.Contains(gameObjectLight))
            {
                _lights.Add(gameObjectLight);
            }
        }

        protected override void OnGameManagerChangeState(GameState gameState)
        {
            if (gameState == GameState.Generating)
            {
                _multiManager = GameManager.GetManager<MultiManager>();
                                        
                if (!_multiManager.GetPlayerGameObject()) return;
                                        
                player = _multiManager.GetPlayerGameObject().GetComponentInChildren<PlayerBehaviour>().gameObject;
                CheckForPlayer();
            }
             
        }


        private async Task CheckForPlayer()
        {
            foreach (var gameObjectLight in _lights)
            {
                gameObjectLight.SetActive(
                    Vector3.Distance(gameObjectLight.transform.position, player.transform.position) < lightDistance);
            }

            await Task.Delay(checkIntervalMS);
            
            CheckForPlayer();
        }
    }
}