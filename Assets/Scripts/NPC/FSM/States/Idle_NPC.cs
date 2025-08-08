using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using UnityEngine.Serialization;
using static AnimNpc;
using static MoodsNpc;

public class Idle_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    [SerializeField] private Texture2D moodImage;

    private Coroutine _recoverStepsRoutine;


    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Generators.SetupStateWithAnimation(LightRest,
                                   IdleAnimNpc, 
                                                moodImage)
            (npcGoap, npc);

        _recoverStepsRoutine = StartCoroutine(RecoverStepsOverTime());
    }

    public override IState ProcessInput() => this;

    public override void UpdateLoop()
    {
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_recoverStepsRoutine != null) StopCoroutine(_recoverStepsRoutine);

        return base.Exit(to);
    }

    private IEnumerator RecoverStepsOverTime()
    {
        while (npcGoap.WorldState.Steps < npcGoap.WorldState.MaxSteps)
        {
            npcGoap.WorldState.Steps++;

            yield return new WaitForSeconds(1);
        }
    }
}