using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _2Scripts.Manager
{
    public class SceneManager : GameManagerSync<SceneManager>
    {
        public void Init()
        {
            if (GameManager.GetManager<MultiManager>().IsLobbyHost())
            {
                NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Additive);
            }
            NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
            NetworkManager.Singleton.SceneManager.PostSynchronizationSceneUnloading = true;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += SceneManagerOnOnLoadComplete;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += SceneManagerOnOnSynchronizeComplete;
            NetworkManager.Singleton.SceneManager.OnUnloadComplete += SceneManagerOnOnUnloadComplete;
        }

        private void SceneManagerOnOnUnloadComplete(ulong clientid, string scenename)
        {
            Debug.Log($"{scenename} unloaded");
        }

        private void SceneManagerOnOnSynchronizeComplete(ulong clientid)
        {
            Debug.Log("Scenes Synchronized");
        }

        private void SceneManagerOnOnLoadComplete(ulong clientid, string scenename, LoadSceneMode loadscenemode)
        {
            Debug.Log($"{scenename} loaded");
        }

        [Rpc(SendTo.Everyone)]
        private void SetActiveSceneRPC(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName));
        }
        
        public void LoadSceneNetwork(Scenes scenes)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(scenes.ToString(), LoadSceneMode.Single);
        }
        
        public void LoadScene(Scenes scenes)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(scenes.ToString(), LoadSceneMode.Single);
        }

        public void ActivateLoadingScreen()
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
        public void DeactivateLoadingScreen()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public enum Scenes
    {
        MainMenu,
        Level,
        CharacterSelection,
        None
    }
}