using UnityEngine;
using System.Collections;

public class AttackBehavior : IBehavior
{
    private IAgent agent;
    private IAction action;
    private MovementController movement;

    public AttackBehavior(IAgent agent, MovementController movement, IAction action)
    {
        this.agent = agent;
        this.movement = movement;
        this.action = action;
    }

    public BehaviorResult Execute()
    {
        if (agent.GetTarget() == null)
            return BehaviorResult.Failure;

        // stop movement during attack
        movement.Stop();

        // Check if attack is already in progress
        if (action.GetStatus() == ActionResult.InProgress)
        {
            // Attack is in progress, keep control
            return BehaviorResult.Continue;
        }

        // Start attack if ready
        if (action.CanExecute())
        {
            // Set facing toward target when starting attack, then leave it
            agent.SetFacing(agent.GetTarget().position);
            
            agent.StartCoroutine(action.Execute());
            return BehaviorResult.Continue; // Take control during attack
        }

        return BehaviorResult.Failure;
    }
}