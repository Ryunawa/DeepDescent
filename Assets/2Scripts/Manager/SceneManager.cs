using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _2Scripts.Manager
{
    public static class SceneManager
    {
        public static Scenes SetActiveScene = Scenes.None;
        private static List<Scenes> _scenesToLoadArray = new List<Scenes>{};
        public static void Init()
        {
            NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Additive);
            NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += SceneManagerOnOnLoadComplete;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += SceneManagerOnOnSynchronizeComplete;
            NetworkManager.Singleton.SceneManager.OnUnloadComplete += SceneManagerOnOnUnloadComplete;
        }

        private static void SceneManagerOnOnUnloadComplete(ulong clientid, string scenename)
        {
            Debug.Log($"{scenename} unloaded");
        }

        private static void SceneManagerOnOnSynchronizeComplete(ulong clientid)
        {
            throw new System.NotImplementedException();
        }

        private static void SceneManagerOnOnLoadComplete(ulong clientid, string scenename, LoadSceneMode loadscenemode)
        {
            if (SetActiveScene.ToString() == scenename)
            {
                Debug.Log(scenename);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(
                    UnityEngine.SceneManagement.SceneManager.GetSceneByName(scenename));
                
                SetActiveScene = Scenes.None;
            }

            if (scenename == Scenes.Loading.ToString())
            {
                LoadScene(_scenesToLoadArray[0]);
            }

            
        }

        //TODO : on scene load --> loading screen --> on event called --> load actual loading screen
        public static void LoadingScene(Scenes scene)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(Scenes.Loading.ToString(), LoadSceneMode.Additive);
            _scenesToLoadArray.Add(scene);
        }

        private static void LoadScene(Scenes scenes)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(scenes.ToString(), LoadSceneMode.Additive);
        }

        public static void UnloadScene(Scenes scene)
        {
            NetworkManager.Singleton.SceneManager.UnloadScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(scene.ToString()));  
        }

        public static void LoadAndSetActiveScene(Scenes scene)
        {
            LoadingScene(scene);

            SetActiveScene = scene;
        }
    }

    public enum Scenes
    {
        MainMenu,
        Level,
        SafeZone,
        Loading,
        None
    }
}