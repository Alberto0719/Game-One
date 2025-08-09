using UnityEngine;

public class SlashAttackBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Find the SlashAction component (should be on same GameObject)
        SlashAction slashAction = animator.GetComponent<SlashAction>();
        if (slashAction == null)
        {
            slashAction = animator.GetComponentInParent<SlashAction>();
        }

        if (slashAction != null)
        {
            slashAction.OnAnimationEnd();
        }
        else
        {
            // Fallback: try to find Player component for backward compatibility
            Player player = animator.GetComponentInParent<Player>();
            if (player != null)
            {
                player.OnAttackComplete();
            }
        }
    }
}