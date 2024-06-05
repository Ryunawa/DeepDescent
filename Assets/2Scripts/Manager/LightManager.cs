using System.Collections.Generic;
using System.Threading.Tasks;
using _2Scripts.Entities.Player;
using UnityEngine;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace _2Scripts.Manager
{
    public class LightManager : Singleton<LightManager>
    {
        [SerializeField] private float lightDistance;
        [SerializeField] private int checkIntervalMS;
        [SerializeField] private GameObject player;
        private List<GameObject> lights = new List<GameObject>();


        public void AddToLightsList(GameObject gameObjectLight)
        {
            if (!lights.Contains(gameObjectLight))
            {
                lights.Add(gameObjectLight);
            }
        }
        
        private void Start()
        {
            player = MultiManager.instance.GetPlayerGameObject().GetComponentInChildren<PlayerBehaviour>().gameObject;
            
            CheckForPlayer();
        }

        private async Task CheckForPlayer()
        {
            foreach (var gameObjectLight in lights)
            {
                gameObjectLight.SetActive(
                    Vector3.Distance(gameObjectLight.transform.position, player.transform.position) < lightDistance);
            }

            await Task.Delay(checkIntervalMS);
            
            CheckForPlayer();
        }
    }
}