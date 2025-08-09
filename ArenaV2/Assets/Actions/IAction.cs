using UnityEngine;
using System.Collections;

public enum ActionResult
{
    Success,
    Failure,
    InProgress
}

public interface IAction
{
    ActionResult GetStatus();
    bool CanExecute();
    IEnumerator Execute();
    void Interrupt();
    float GetCooldownRemaining();
    bool IsOnCooldown();
    float GetRange();
}