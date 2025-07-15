using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using static MoodsNpc;

public class Idle_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    private Coroutine _recoverStepsRoutine;

    public override IState ProcessInput()
    {
        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        //npc.animator.SetTrigger(AnimNpc.IdleNpc);
        _recoverStepsRoutine = npc.StartCoroutine(RecoverStepsOverTime());
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_recoverStepsRoutine != null)
            StopCoroutine(_recoverStepsRoutine);

        Debug.Log("Sali de IDLE");


        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
        Debug.Log("Estoy en Idle");
    }

    private IEnumerator RecoverStepsOverTime()
    {
        npcGoap.worldState.mood = LightRest;

        Debug.Log("Entro a corrutina de Steps");

        while (npcGoap.worldState.steps < npcGoap.worldState.maxsteps)
        {
            npcGoap.worldState.steps++;
            Debug.Log("Se sumo un step, ahora tus steps son:" + $"{npcGoap.worldState.steps}");
            yield return new WaitForSeconds(1f);
        }
    }
}