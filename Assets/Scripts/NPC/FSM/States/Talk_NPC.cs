using System.Collections;
using System.Collections.Generic;
using FSM;
using JetBrains.Annotations;
using UnityEngine;

public class Talk_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    private Coroutine _talkCoroutine;
    private Coroutine _movementRoutine;

    public override IState ProcessInput()
    {
        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.animator.SetTrigger(AnimNpc.WalkNpc);

        npcGoap.worldState.mood = MoodsNpc.Curious;

        if (npc.currentInteractable != null)
            _talkCoroutine = StartCoroutine(TalkRoutine(npc.currentInteractable));
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_talkCoroutine != null)
            StopCoroutine(_talkCoroutine);

        if (_movementRoutine != null)
            StopCoroutine(_movementRoutine);

        npc.currentInteractable = null;

        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
        Debug.Log("[Talk]");
    }

    private IEnumerator TalkRoutine(InteractableNPC interactable)
    {        
        yield return _movementRoutine = StartCoroutine(npc.MoveAlongPath(npc.CurrentNode, interactable.assignedNode));

        Vector3 dir = (interactable.transform.position - npc.transform.position);
        dir.y = 0;
        npc.transform.forward = dir.normalized;

        npc.animator.SetTrigger(AnimNpc.TalkNpc);

        yield return new WaitForSeconds(5f);

        npcGoap.worldState.mood = MoodsNpc.Waiting;
        npc.currentInteractable = null;
    }
}