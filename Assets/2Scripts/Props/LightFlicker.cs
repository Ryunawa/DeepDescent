using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _2Scripts.Helpers;
using _2Scripts.Interfaces;
using _2Scripts.Manager;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Light))]
public class LightFlicker : GameManagerSync<LightFlicker>
{
    [SerializeField] private int flickerIntervalsMS = 100;
    [SerializeField] private float baseLightIntensity = 2.5f;
    [SerializeField] private float intensityOffsetMultiplier = 0.2f;

    private Light _light;

    void Start()
    {
        if (GameManager.GameState != GameState.InLevel) return;
                                                    
        TryGetComponent(out _light);
                                                    
        GameManager.GetManager<LightManager>().AddToLightsList(gameObject);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Flicker();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    private void OnEnable()
    {
        Start();
    }

    private async Task Flicker()
    {
        if (!gameObject.activeSelf)
        { 
            return;
        }
        
        _light.intensity = baseLightIntensity + (Random.value * 2 - 1 ) * intensityOffsetMultiplier;
        
        await Task.Delay(flickerIntervalsMS);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Flicker();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }
}
