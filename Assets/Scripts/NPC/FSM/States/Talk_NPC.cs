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

    public override IState ProcessInput()
    {
        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        //npc.animator.SetTrigger(AnimNpc.TalkNpc);

        if (npc.currentInteractable != null)
            _talkCoroutine = StartCoroutine(TalkRoutine(npc.currentInteractable));
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_talkCoroutine != null)
            StopCoroutine(_talkCoroutine);

        npcGoap.worldState.mood = MoodsNpc.Curious;

        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
    }

    private IEnumerator TalkRoutine(InteractableNPC interactable)
    {
        yield return npc.StartCoroutine(npc.MoveAlongPath(npc.currentNode, interactable.assignedNode));

        Vector3 dir = (interactable.transform.position - npc.transform.position);
        dir.y = 0;
        npc.transform.forward = dir.normalized;

        yield return new WaitForSeconds(5f);

        npc.currentInteractable = null;
    }
}