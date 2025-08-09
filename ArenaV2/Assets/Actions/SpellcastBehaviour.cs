using UnityEngine;

public class SpellcastBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Find the Player component
        Player player = animator.GetComponent<Player>();
        if (player != null)
        {
            player.OnSpellcastComplete();
        }
    }
}