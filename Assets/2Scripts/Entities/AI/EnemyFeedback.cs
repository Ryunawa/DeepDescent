using UnityEngine;
using System.Collections;
using System.Linq;

public class EnemyFeedback : MonoBehaviour
{
    public Renderer activeRenderer;
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    private Color originalColor;
    private Coroutine flashCoroutine;

    void Start()
    {
        FindActiveRenderer();
    }

    private void FindActiveRenderer()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        activeRenderer = renderers.FirstOrDefault(r => r.gameObject.activeSelf);

        if (activeRenderer != null)
        {
            originalColor = activeRenderer.material.color;
        }
        else
        {
            Debug.LogWarning("No active renderer found!");
        }
    }

    public void TakeHit()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashColor());
    }

    IEnumerator FlashColor()
    {
        activeRenderer.material.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        activeRenderer.material.color = originalColor;
    }
}
