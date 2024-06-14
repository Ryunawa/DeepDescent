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
    public bool despawnOnDeath = false;
    [DoNotSerialize] public Vector3 projectileDirection = Vector3.zero;
    private HealthComponent ownHealthComponent;
    private void Start()
    {
        if (TryGetComponent(out ownHealthComponent))
        {
            ownHealthComponent.OnDeath.AddListener(
                () =>
                {
                    if (despawnOnDeath)
                    {
                        Debug.Log("Die");
                        NetworkObject.Despawn();
                    }
                }
            );
        }
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        transform.position = (projectileMovementSpeed * projectileDirection * Time.fixedDeltaTime) + transform.position ;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            return;
        if (other.TryGetComponent(out HealthComponent healthComponent))
        {
            healthComponent.TakeDamage(projectileDamage);
        }

        if (TryGetComponent(out HealthComponent ownHealthComponent))
        {
            ownHealthComponent.TakeDamage(1.0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player")) 
            return;
        if (collision.collider.TryGetComponent(out HealthComponent healthComponent))
        {
            healthComponent.TakeDamage(projectileDamage);
        }

        if (ownHealthComponent)
        {
            ownHealthComponent.TakeDamage(1.0f);
        }
    }
    

}
