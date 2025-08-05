using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using static AnimNpc;
using static MoodsNpc;

public class Talk_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    private Coroutine _talkCoroutine;
    private Coroutine _movementRoutine;
    
    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.Animator.SetTrigger(WalkAnimNpc);

        npcGoap.WorldState.Mood = Curious;
        
        npcGoap.WorldState.UpdateSpeedByMood();

        if (npc.currentInteractable != null)
            _talkCoroutine = StartCoroutine(TalkRoutine(npc.currentInteractable));
    }
    
    public override IState ProcessInput() => this;

    public override void UpdateLoop() { }
    
    public override Dictionary<string, object> Exit(IState to)
    {
        if(_talkCoroutine != null)
            StopCoroutine(_talkCoroutine);
        
        if( _movementRoutine != null)
            StopCoroutine(_movementRoutine);

        npc.currentInteractable = null;

        return base.Exit(to);
    }


    private IEnumerator TalkRoutine(InteractableNPC interactable)
    {
        yield return _movementRoutine = StartCoroutine(npc.MoveTo(interactable.assignedNode, 
                                                                        npcGoap.WorldState.Speed, 
                                                                        npcGoap.WorldState.SpeedRotation));
        
        Vector3 dir = (interactable.transform.position - npc.transform.position);
        
        dir.y = 0;
        
        npc.transform.forward = dir.normalized;

        npc.Animator.SetTrigger(TalkAnimNpc);

        yield return new WaitForSeconds(5f);

        npcGoap.WorldState.Mood = Waiting;
        
        npc.currentInteractable = null;
    }
}