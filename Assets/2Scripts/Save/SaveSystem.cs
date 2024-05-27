using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using NaughtyAttributes;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace _2Scripts.Save
{
    public static class SaveSystem
    {
        private const string Filename = "Cowpocalypse.noext";
        private static readonly string Path = Application.persistentDataPath + "/" + Filename;
        
        public static void Save()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            List<InventoryObject> data = MultiManager.instance.GetPlayerGameObject().GetComponent<Inventory>().InventoryItems;

            using FileStream stream = new FileStream(Path, FileMode.Create);
            formatter.Serialize(stream, data);
        }
        
        public static void LoadGame()
        {
            MultiManager.instance.GetPlayerGameObject().GetComponent<Inventory>().InventoryItems = GetSavedGameData();
        }
        
        private static bool CheckForSave()
        {
            return File.Exists(Path);
        }

        private static List<InventoryObject> GetSavedGameData()
        {
            if (CheckForSave())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(Path, FileMode.Open);

                List<InventoryObject> data = formatter.Deserialize(stream) as List<InventoryObject>;

                stream.Close();
                return data;
            }
            else
            {
                Debug.Log("Save file not found in " + Path);
                return null;
            }
        }
        
        public static void OverrideSave()
        {
            File.Delete(Path);
        }
    }
}