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
        other.TryGetComponent(out HealthComponent collidedHealthComponent);
        if (_swinging && _canInflictDamage)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (collidedHealthComponent && TryGetComponent(out EnemyData data))
                {
                    collidedHealthComponent.TakeDamage(data.damageInflicted);
                }
                Debug.Log("DAMAGE");

                // Start the cooldown coroutine
                StartCoroutine(DamageCooldown());
            }
            // else
            // {
            //     if (collidedHealthComponent && TryGetComponent(out Inventory inventory))
            //     {
            //         collidedHealthComponent.TakeDamage(inventory.MainHandItem.AttackValue);
            //     }
            // }
        }
        else if (((PlayerBehaviour)controller).IsAttacking)
        {
            collidedHealthComponent.TakeDamage(GameManager.playerBehaviour.inventory.MainHandItem.AttackValue);
        }
    }

    private IEnumerator DamageCooldown()
    {
        _canInflictDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        _canInflictDamage = true;
    }
}
