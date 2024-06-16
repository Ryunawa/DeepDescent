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
    public float projectileLife = 1.0f;
    public bool despawnOnDeath = false;
    public ParticleSystem vfx;
    public GameObject mesh;
    [DoNotSerialize] public Vector3 projectileDirection = Vector3.zero;
    private void Start()
    {
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
        StartCoroutine(ShowVFXOrMesh());
    }

    private void FixedUpdate()
    {
        if (!IsServer)
            return;
        transform.position = projectileDirection * projectileMovementSpeed + transform.position ;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || !IsServer || projectileLife <= 0)
            return;
        if (other.TryGetComponent(out HealthComponent healthComponent))
        {
            healthComponent.TakeDamage(projectileDamage);
        }

        projectileLife--;

        if (projectileLife <= 0)
            if (TryGetComponent(out NetworkObject networkObject))
                    networkObject.Despawn(true);
    }

    private IEnumerator ShowVFXOrMesh()
    {
        yield return new WaitForEndOfFrame();
        if (vfx)
            vfx.Play(true);
        if (mesh)
            mesh.SetActive(true);
    }
}
