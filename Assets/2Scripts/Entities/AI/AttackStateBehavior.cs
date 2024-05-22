using UnityEngine;

public class AttackStateBehavior : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AIController aiController = animator.GetComponent<AIController>();
        if (aiController != null)
        {
            aiController.StartSwing();
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AIController aiController = animator.GetComponent<AIController>();
        if (aiController != null)
        {
            aiController.EndSwing();
        }
    }
}