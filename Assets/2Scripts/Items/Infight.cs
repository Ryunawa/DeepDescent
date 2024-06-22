using UnityEngine;
using System.Collections;
using _2Scripts.Entities;
using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using _2Scripts.Struct;

public class Infight : MonoBehaviour
{
    private bool _swinging;
    private bool _canInflictDamage = true;
    [SerializeField] private float damageCooldown = 1f;

    [SerializeField] private bool isEnemy;

    [SerializeField]
    private MonoBehaviour controller;

    private IController swingController;

    private void OnEnable()
    {
        swingController = controller as IController;
        if (swingController != null)
        {
            swingController.OnSwingStateChanged += UpdateSwingingState;
        }
    }

    private void OnDisable()
    {
        if (swingController != null)
        {
            swingController.OnSwingStateChanged -= UpdateSwingingState;
        }
    }

    private void UpdateSwingingState(bool isSwinging)
    {
        _swinging = isSwinging;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy")) return;

        Debug.Log("enter trigger");
        other.TryGetComponent(out HealthComponent collidedHealthComponent);

        if (collidedHealthComponent == null)
        {
            Debug.LogWarning("No HealthComponent found on the collided object.");
            return;
        }

        // Is enemy attacking
        if (_swinging && _canInflictDamage && isEnemy && other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player!");
            if (TryGetComponent(out EnemyData data))
            {
                collidedHealthComponent.TakeDamage(data.damageInflicted);
                Debug.Log("DAMAGE: " + data.damageInflicted);
            }

            // Start the cooldown coroutine
            StartCoroutine(DamageCooldown());
        }
        // Is player attacking
        else
        {
            Debug.Log("else");
            PlayerBehaviour playerController = controller as PlayerBehaviour;
            if (playerController != null && playerController.IsAttacking)
            {
                if (collidedHealthComponent != playerController.Health)
                {
                    collidedHealthComponent.TakeDamage(GameManager.playerBehaviour.inventory.MainHandItem.AttackValue);
                }
            }
        }
    }

    private IEnumerator DamageCooldown()
    {
        _canInflictDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        _canInflictDamage = true;
    }
}
