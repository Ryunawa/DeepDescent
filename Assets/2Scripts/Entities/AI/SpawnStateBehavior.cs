using _2Scripts.Entities.AI;
using UnityEngine;

public class SpawnStateBehavior : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AIController aiController = animator.GetComponent<AIController>();
        if (aiController != null)
        {
            aiController.OnSpawnAnimationComplete();
        }
    }
}