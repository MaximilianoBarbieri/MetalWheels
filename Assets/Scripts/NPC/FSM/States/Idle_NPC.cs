using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;

public class Idle_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    private Coroutine _recoverStepsRoutine;

    public override IState ProcessInput()
    {
        Debug.Log("Estoy en Idle");
        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Debug.Log("Estoy en Idle");
        //npc.animator.SetTrigger(AnimNpc.IdleNpc);
        _recoverStepsRoutine = npc.StartCoroutine(RecoverStepsOverTime());
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_recoverStepsRoutine != null)
            StopCoroutine(_recoverStepsRoutine);

        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
        Debug.Log("Estoy en Idle");
    }

    private IEnumerator RecoverStepsOverTime()
    {
        Debug.Log("Entro a corrutina de Steps");

        while (npcGoap.worldState.steps < npcGoap.worldState.maxsteps)
        {
            npcGoap.worldState.steps++;
            Debug.Log("Se sumo un step, ahora tus steps son:" + $"{npcGoap.worldState.steps}");
            yield return new WaitForSeconds(1f);
        }
    }
}