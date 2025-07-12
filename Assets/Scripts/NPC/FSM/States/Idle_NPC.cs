using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;

public class Idle_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    private Coroutine _recoverStepsRoutine;

    public override IState ProcessInput()
    {
        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.animator.SetTrigger(AnimNpc.IdleNpc);
        _recoverStepsRoutine = StartCoroutine(RecoverStepsOverTime());
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_recoverStepsRoutine != null)
            StopCoroutine(_recoverStepsRoutine);
        
        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
    }

    private IEnumerator RecoverStepsOverTime()
    {
        while (true)
        {
            if (npc.worldState.steps >= npc.worldState.maxsteps)
                yield break;

            npc.worldState.steps = Mathf.Min(npc.worldState.steps + 1, npc.worldState.maxsteps);
            Debug.Log("[Idle] Se recuperaron steps.");
            yield return new WaitForSeconds(1f);
        }
    }
}