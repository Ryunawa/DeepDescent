using System;
using _2Scripts.Manager;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _2Scripts.Interfaces
{
    public abstract class GameManagerSync<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        protected virtual void Start()
        {
            GameManager.stateChanged.AddListener(OnGameManagerChangeState);
        }

        protected virtual void OnGameManagerChangeState(GameState gameState){}
    }
}