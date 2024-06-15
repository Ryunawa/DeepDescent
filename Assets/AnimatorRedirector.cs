using System;
using System.Collections;
using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using UnityEngine;

public class AnimatorRedirector : MonoBehaviour
{
    public void ResetIsAttacking()
    {
        GameManager.playerBehaviour.ResetIsAttacking();
    }
}
