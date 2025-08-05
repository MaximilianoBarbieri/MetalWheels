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

    
    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.Animator.SetTrigger(IdleAnimNpc);

        npcGoap.WorldState.Mood = LightRest;
        
        npcGoap.WorldState.UpdateSpeedByMood();

        _recoverStepsRoutine = StartCoroutine(RecoverStepsOverTime());
    }
    
    public override IState ProcessInput() => this;
    
    public override void UpdateLoop() { }
    
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
            
            yield return new WaitForSeconds(2);
        }
    }
}