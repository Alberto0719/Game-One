using UnityEngine;

public enum BehaviorResult
{
    Success,
    Failure,
    Continue
}

public interface IBehavior
{
    BehaviorResult Execute();
}