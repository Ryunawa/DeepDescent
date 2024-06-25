using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VisibleItems : NetworkBehaviour
{
    [Header("Inventory")]
    public List<GameObject> scraps;
    public List<GameObject> swords;
    public GameObject shield;
    public GameObject potion;
    public GameObject spellBook;

    [Header("Equipped")]
    public List<GameObject> armors;
    public List<GameObject> swordEquippable;
    public List<GameObject> shieldEquippable;
    [SerializeField] private GameObject rightHandFolderParent;
    [SerializeField] private GameObject leftHandFolderParent;
    public GameObject equippedWeapon;
    public GameObject equippedShield;

    private int armorIndex = 0;

    [Rpc(SendTo.ClientsAndHost)]
    // Activates a random non-visible scrap
    public void AddVisibleScrapRpc()
    {
        List<GameObject> invisibleScraps = scraps.FindAll(scrap => !scrap.activeSelf);
        if (invisibleScraps.Count > 0)
        {
            GameObject randomScrap = invisibleScraps[Random.Range(0, invisibleScraps.Count)];
            randomScrap.SetActive(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Deactivates a random visible scrap
    public void RemoveVisibleScrapRpc()
    {
        List<GameObject> visibleScraps = scraps.FindAll(scrap => scrap.activeSelf);
        if (visibleScraps.Count > 0)
        {
            GameObject randomScrap = visibleScraps[Random.Range(0, visibleScraps.Count)];
            randomScrap.SetActive(false);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Activates a random non-visible sword
    public void AddVisibleSwordRpc()
    {
        List<GameObject> invisibleSwords = swords.FindAll(sword => !sword.activeSelf);
        if (invisibleSwords.Count > 0)
        {
            GameObject randomSword = invisibleSwords[Random.Range(0, invisibleSwords.Count)];
            randomSword.SetActive(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Deactivates a random visible sword
    public void RemoveVisibleSwordRpc()
    {
        List<GameObject> visibleSwords = swords.FindAll(sword => sword.activeSelf);
        if (visibleSwords.Count > 0)
        {
            GameObject randomSword = visibleSwords[Random.Range(0, visibleSwords.Count)];
            randomSword.SetActive(false);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Activates the shield
    public void AddVisibleShieldRpc()
    {
        if (!shield.activeSelf && shield)
        {
            shield.SetActive(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Deactivates the shield
    public void RemoveVisibleShieldRpc()
    {
        if (shield.activeSelf && shield)
        {
            shield.SetActive(false);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Activates the spellBook
    public void AddVisibleSpellBookRpc()
    {
        if (!spellBook.activeSelf && spellBook)
        {
            spellBook.SetActive(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Deactivates the spellBook
    public void RemoveVisibleSpellBookRpc()
    {
        if (spellBook.activeSelf && spellBook)
        {
            spellBook.SetActive(false);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Activates the potion
    public void AddVisiblePotionsRpc()
    {
        if (!potion.activeSelf && potion)
        {
            potion.SetActive(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Deactivates the potion
    public void RemoveVisiblePotionsRpc()
    {
        if (potion.activeSelf && potion)
        {
            potion.SetActive(false);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Activates the next armor in order
    public void AddVisibleArmorRpc()
    {
        if (armorIndex < armors.Count && !armors[armorIndex].activeSelf && armors[armorIndex])
        {
            armors[armorIndex].SetActive(true);
            armorIndex++;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Deactivates the previous armor in order
    public void RemoveVisibleArmorRpc()
    {
        if (armorIndex > 0)
        {
            armorIndex--;
            if (armors[armorIndex].activeSelf && armors[armorIndex])
            {
                armors[armorIndex].SetActive(false);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Equips a specified weapon
    public void EquipRightHandRpc(string itemName)
    {
        // Replace spaces with underscores
        string formattedItemName = itemName.Replace(" ", "_");

        if (rightHandFolderParent != null)
        {
            GameObject weaponToEquip = rightHandFolderParent.transform.Find("SM_Wep_" + formattedItemName)?.gameObject;
            if (weaponToEquip != null)
            {
                if (equippedWeapon != null)
                {
                    UnequipRightHandRpc();
                }

                equippedWeapon = weaponToEquip;
                equippedWeapon.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Weapon parent is null.");
        }
    }



    [Rpc(SendTo.ClientsAndHost)]
    // Equips a specified shield
    public void EquipLeftHandRpc(string itemName)
    {
        // Replace spaces with underscores
        string formattedItemName = itemName.Replace(" ", "_");

        if (leftHandFolderParent != null)
        {
            GameObject weaponToEquip = leftHandFolderParent.transform.Find("SM_Wep_" + formattedItemName)?.gameObject;
            if (weaponToEquip != null)
            {
                if (equippedShield != null)
                {
                    UnequipLeftHandRpc();
                }

                equippedShield = weaponToEquip;
                equippedShield.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Weapon parent is null.");
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Unequips the currently equipped weapon
    public void UnequipRightHandRpc()
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.SetActive(false);
            equippedWeapon = null;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    // Unequips the currently equipped shield
    public void UnequipLeftHandRpc()
    {
        if (equippedShield != null)
        {
            equippedShield.SetActive(false);
            equippedShield = null;
        }
    }


    // Deactivates all items
    public void DeactivateAllItems()
    {
        foreach (GameObject scrap in scraps)
        {
            scrap.SetActive(false);
        }
        foreach (GameObject sword in swords)
        {
            sword.SetActive(false);
        }
        foreach (GameObject armor in armors)
        {
            armor.SetActive(false);
        }
        if (shield.activeSelf)
        {
            shield.SetActive(false);
        }
    }

    // Activates all items
    public void ActivateAllItems()
    {
        foreach (GameObject scrap in scraps)
        {
            scrap.SetActive(true);
        }
        foreach (GameObject sword in swords)
        {
            sword.SetActive(true);
        }
        foreach (GameObject armor in armors)
        {
            armor.SetActive(true);
        }
        if (!shield.activeSelf)
        {
            shield.SetActive(true);
        }
    }

    public void ChangeWeaponAndShieldLayer(int layer)
    {
        foreach (Transform trans in leftHandFolderParent.GetComponentsInChildren<Transform>(true)) 
        { 
            trans.gameObject.layer = layer;
        }
        
        foreach (Transform trans in rightHandFolderParent.GetComponentsInChildren<Transform>(true)) 
        { 
            trans.gameObject.layer = layer;
        }
    }

    
}
