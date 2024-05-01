using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private DoorTriggerZone triggerZone;
    private float animationTime = 0f;

    private void Start()
    {
        animator = GetComponent<Animator>();

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "DoorClose")
            {
                animationTime = clip.length;
                break;
            }
        }
    }

    public void OpenDoor()
    {
        animator.Play("DoorOpen", 0, 0.0f);
    }

    public void CloseDoor()
    {
        animator.Play("DoorClose", 0, 0.0f);
        StartCoroutine(CloseDoorDelayed());
    }

    private IEnumerator CloseDoorDelayed()
    {
        yield return new WaitForSeconds(animationTime);
        triggerZone.setIsOpen(false);
    }
}
