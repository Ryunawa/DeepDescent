using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _2Scripts.Manager;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [SerializeField] private int flickerIntervalsMS = 100;
    [SerializeField] private float baseLightIntensity = 2.5f;
    [SerializeField] private float intensityOffsetMultiplier = 0.2f;

    private Light _light;

    // Start is called before the first frame update
    void Start()
    {
        TryGetComponent(out _light);
        
        LightManager.instance.AddToLightsList(gameObject);

        Flicker();
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

        Flicker();
    }
}
