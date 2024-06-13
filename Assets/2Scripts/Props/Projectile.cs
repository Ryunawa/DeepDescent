using _2Scripts.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public float projectileMovementSpeed = 1.0f;
    public float projectileDamage = 1.0f;
    [DoNotSerialize] public Vector3 projectileDirection = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        transform.position =  (projectileMovementSpeed * projectileDirection) + transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            return;
        if (other.TryGetComponent(out HealthComponent healthComponent))
        {
            healthComponent.TakeDamage(projectileDamage);
            if (TryGetComponent(out HealthComponent ownHealthComponent))
            {
                ownHealthComponent.TakeDamage(1.0f);
            }
        }
    }
}
