using UnityEngine;
using System.Collections;
using _2Scripts.Entities;
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
        if (_swinging && _canInflictDamage)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (other.TryGetComponent(out HealthComponent healthComponent) && TryGetComponent(out EnemyData data))
                {
                    healthComponent.TakeDamage(data.damageInflicted);
                }
                Debug.Log("DAMAGE");

                // Start the cooldown coroutine
                StartCoroutine(DamageCooldown());
            }
            else
            {
                if (other.TryGetComponent(out HealthComponent healthComponent) && TryGetComponent(out Inventory inventory))
                {
                    healthComponent.TakeDamage(inventory.MainHandItem.AttackValue);
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
