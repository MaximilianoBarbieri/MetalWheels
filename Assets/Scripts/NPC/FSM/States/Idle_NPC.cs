using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using static AnimNpc;
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
        npc.animator.SetTrigger(IdleNpc);

        npcGoap.worldState.Mood = LightRest;
        npcGoap.worldState.UpdateSpeedByMood();

        _recoverStepsRoutine = StartCoroutine(RecoverStepsOverTime());
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_recoverStepsRoutine != null) StopCoroutine(_recoverStepsRoutine);

        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
    }

    private IEnumerator RecoverStepsOverTime()
    {
        Debug.Log("Entro a corrutina de Steps");

        while (npcGoap.worldState.Steps < npcGoap.worldState.MaxSteps)
        {
            npcGoap.worldState.Steps++;
            //Debug.Log("Se sumo un step, ahora tus steps son:" + $"{npcGoap.worldState.steps}");
            yield return new WaitForSeconds(2);
        }
    }
}