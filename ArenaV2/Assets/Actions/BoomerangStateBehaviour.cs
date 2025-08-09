using UnityEngine;

public class BoomerangStateBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // animator and IAction control script should be on root object together
        BoomerangAction action = animator.GetComponent<BoomerangAction>();
        if (action != null)
        {
            action.OnAnimationEnd();
        }
    }
}