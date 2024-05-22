using UnityEngine;
using System.Collections;

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
        if (other.gameObject.CompareTag("Player") && _swinging && _canInflictDamage)
        {
            // TODO: Inflict damage
            Debug.Log("DAMAGE");

            // Start the cooldown coroutine
            StartCoroutine(DamageCooldown());
        }
    }

    private IEnumerator DamageCooldown()
    {
        _canInflictDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        _canInflictDamage = true;
    }
}
