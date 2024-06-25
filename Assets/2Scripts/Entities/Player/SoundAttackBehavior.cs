using _2Scripts.Entities.AI;
using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAttackBehavior : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerBehaviour playerBehaviour = GameManager.playerBehaviour;
        int ID = playerBehaviour.getCharacterId();

        switch (ID)
        {
            case 1:
            case 3:
                GameManager.GetManager<AudioManager>().PlaySfx("SwordWhoosh", playerBehaviour, 1, 5);
                break;
        }
        
    }

    private int GetActiveChildID(Transform parentTransform)
    {
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                return i;
            }
        }
        return 0;
    }
}