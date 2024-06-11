using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using _2Scripts.Manager;
using NaughtyAttributes;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace _2Scripts.Save
{
    [Serializable]
    public class SaveData
    {
        public List<InventoryObject> inventory;
        public List<int> equipment;

        public SaveData(List<InventoryObject> inventoryObjects, List<int> equipment)
        {
            inventory = inventoryObjects;
            this.equipment = equipment;
        }

        public SaveData()
        {
        }
    }
    public static class SaveSystem
    {
        private const string Filename = "DeepDescent.noext";
        private static readonly string Path = Application.persistentDataPath + "/" + Filename;
        
        public static void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            Inventory inventory = MultiManager.instance.GetPlayerGameObject().GetComponentInChildren<Inventory>();

            SaveData data = new SaveData(inventory.InventoryItems, inventory.GetEquipmentIds());

            TextWriter writer = new StreamWriter(Path);
            serializer.Serialize(writer, data);
            writer.Close();
            
            Debug.Log("Saved Inventory");
        }
        
        public static void LoadInventory()
        {
            if (!CheckForSave()) return;
            
            MultiManager.instance.GetPlayerGameObject().GetComponentInChildren<Inventory>().InventoryItems = GetSavedGameData().inventory;
            MultiManager.instance.GetPlayerGameObject().GetComponentInChildren<Inventory>().SetEquipment(GetSavedGameData().equipment);
            Debug.Log("Loaded Inventory");
        }
        
        private static bool CheckForSave()
        {
            return File.Exists(Path);
        }

        private static SaveData GetSavedGameData()
        {
            if (CheckForSave())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
                FileStream stream = new FileStream(Path, FileMode.Open);

                SaveData data = serializer.Deserialize(stream) as SaveData;

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
        
        //TODO: encrypt and Decrypt using XmlSerializer or just keep binarySerializer ?????
    }
}