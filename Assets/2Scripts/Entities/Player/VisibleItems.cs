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

    // Activates a random non-visible scrap
    public void AddVisibleScrap()
    {
        List<GameObject> invisibleScraps = scraps.FindAll(scrap => !scrap.activeSelf);
        if (invisibleScraps.Count > 0)
        {
            GameObject randomScrap = invisibleScraps[Random.Range(0, invisibleScraps.Count)];
            randomScrap.SetActive(true);
        }
    }

    // Deactivates a random visible scrap
    public void RemoveVisibleScrap()
    {
        List<GameObject> visibleScraps = scraps.FindAll(scrap => scrap.activeSelf);
        if (visibleScraps.Count > 0)
        {
            GameObject randomScrap = visibleScraps[Random.Range(0, visibleScraps.Count)];
            randomScrap.SetActive(false);
        }
    }

    // Activates a random non-visible sword
    public void AddVisibleSword()
    {
        List<GameObject> invisibleSwords = swords.FindAll(sword => !sword.activeSelf);
        if (invisibleSwords.Count > 0)
        {
            GameObject randomSword = invisibleSwords[Random.Range(0, invisibleSwords.Count)];
            randomSword.SetActive(true);
        }
    }

    // Deactivates a random visible sword
    public void RemoveVisibleSword()
    {
        List<GameObject> visibleSwords = swords.FindAll(sword => sword.activeSelf);
        if (visibleSwords.Count > 0)
        {
            GameObject randomSword = visibleSwords[Random.Range(0, visibleSwords.Count)];
            randomSword.SetActive(false);
        }
    }

    // Activates the shield
    public void AddVisibleShield()
    {
        if (!shield.activeSelf)
        {
            shield.SetActive(true);
        }
    }

    // Deactivates the shield
    public void RemoveVisibleShield()
    {
        if (shield.activeSelf)
        {
            shield.SetActive(false);
        }
    }

    // Activates the spellBook
    public void AddVisibleSpellBook()
    {
        if (!spellBook.activeSelf)
        {
            spellBook.SetActive(true);
        }
    }

    // Deactivates the spellBook
    public void RemoveVisibleSpellBook()
    {
        if (spellBook.activeSelf)
        {
            spellBook.SetActive(false);
        }
    }

    // Activates the potion
    public void AddVisiblePotions()
    {
        if (!potion.activeSelf)
        {
            potion.SetActive(true);
        }
    }

    // Deactivates the potion
    public void RemoveVisiblePotions()
    {
        if (potion.activeSelf)
        {
            potion.SetActive(false);
        }
    }

    // Activates the next armor in order
    public void AddVisibleArmor()
    {
        if (armorIndex < armors.Count && !armors[armorIndex].activeSelf)
        {
            armors[armorIndex].SetActive(true);
            armorIndex++;
        }
    }

    // Deactivates the previous armor in order
    public void RemoveVisibleArmor()
    {
        if (armorIndex > 0)
        {
            armorIndex--;
            if (armors[armorIndex].activeSelf)
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
                    UnequipRightHand();
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



    // Equips a specified shield
    public void EquipLeftHand(string itemName)
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
                    UnequipLeftHand();
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

    // Unequips the currently equipped weapon
    public void UnequipRightHand()
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.SetActive(false);
            equippedWeapon = null;
        }
    }

    // Unequips the currently equipped shield
    public void UnequipLeftHand()
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

    
}
