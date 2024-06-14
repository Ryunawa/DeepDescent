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
    public ParticleSystem vfx;
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
                        NetworkObject.Despawn(true);
                    }
                }
            );
        }
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
        StartCoroutine(ShowVFX());
    }

    private void FixedUpdate()
    {
        if (!IsServer)
            return;
        transform.position = projectileDirection * projectileMovementSpeed + transform.position ;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || !IsOwner)
            return;
        if (other.TryGetComponent(out HealthComponent healthComponent))
        {
            healthComponent.TakeDamage(projectileDamage);
        }

        if (TryGetComponent(out HealthComponent ownHealthComponent))
        {
            Debug.Log($"Collider = {gameObject.name}");
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
            Debug.Log($"Collider = {gameObject.name}");
            ownHealthComponent.TakeDamage(1.0f);
        }
    }
    
    private IEnumerator ShowVFX()
    {
        yield return new WaitForEndOfFrame();
        vfx.Play(true);
    }
}
