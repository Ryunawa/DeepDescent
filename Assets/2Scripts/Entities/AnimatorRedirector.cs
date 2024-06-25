using System;
using System.Collections;
using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Helpers;
using _2Scripts.Manager;
using UnityEngine;

public class AnimatorRedirector : GameManagerSync<AnimatorRedirector>
{
    [SerializeField] private List<Animator> _animators;
    [SerializeField] private Transform boltOrigin;
    [SerializeField] private SpellCasterComponent _spellCasterComponent;

    protected override void OnGameManagerChangeState(GameState gameState)
    {
        //_spellCasterComponent = GameManager.GetPlayerComponent<SpellCasterComponent>();
    }

    public void ResetIsAttacking()
    {
        GameManager.playerBehaviour.ResetIsAttacking();
    }

    public void CastSpell()
    {
        PlayerBehaviour playerBehaviour = GameManager.playerBehaviour;

        bool isBow = playerBehaviour.inventory.MainHandItem.WeaponType == WeaponType.BOW;
        bool isStaff = playerBehaviour.inventory.MainHandItem.WeaponType == WeaponType.MAGIC;

        Vector3 pos = Vector3.zero;
        if (isBow)
        {
            pos = boltOrigin.position;
        }
        else if (isStaff)
        {
            pos = GameManager.playerBehaviour.HandPosition.position;
        }
        
        
        _spellCasterComponent.SpawnSpellRpc(playerBehaviour.inventory.MainHandItem.ID, pos, playerBehaviour.transform.rotation, isStaff, isBow);
    }

    public void AnimateBow()
    {
        foreach (var animator in _animators)
        {
            animator.Play("Take 001");
        }
    }
}
