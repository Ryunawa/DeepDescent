using System;
using System.Collections;
using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using UnityEngine;

public class AnimatorRedirector : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform boltOrigin;
    private SpellCasterComponent _spellCasterComponent;

    private void Start()
    {
        _spellCasterComponent = GameManager.GetPlayerComponent<SpellCasterComponent>();
    }

    public void ResetIsAttacking()
    {
        GameManager.playerBehaviour.ResetIsAttacking();
    }

    public void CastSpell()
    {
        if (boltOrigin)
            GameManager.GetPlayerComponent<SpellCasterComponent>().positionToCastFrom = boltOrigin.position;
        
        PlayerBehaviour playerBehaviour = GameManager.playerBehaviour;
        _spellCasterComponent.SpawnSpellRpc(playerBehaviour.inventory.MainHandItem.ID, true);
    }

    public void AnimateBow()
    {
        _animator.Play("Take 001");
    }
}
