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
        npc.Animator.SetTrigger(AnimNpc.WalkAnimNpc);

        npcGoap.worldState.Mood = MoodsNpc.Curious;
        npcGoap.worldState.UpdateSpeedByMood();

        if (npc.CurrentInteractable != null)
            _talkCoroutine = StartCoroutine(TalkRoutine(npc.CurrentInteractable));
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_talkCoroutine != null)
            StopCoroutine(_talkCoroutine);

        StopCoroutine(_movementRoutine);

        npc.CurrentInteractable = null;

        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
        Debug.Log("[Talk]");
    }

    private IEnumerator TalkRoutine(InteractableNPC interactable)
    {
        yield return _movementRoutine = StartCoroutine(npc.MoveTo(interactable.assignedNode, 
            npcGoap.worldState.Speed, 
            npcGoap.worldState.SpeedRotation));

        Vector3 dir = (interactable.transform.position - npc.transform.position);
        dir.y = 0;
        npc.transform.forward = dir.normalized;

        npc.Animator.SetTrigger(AnimNpc.TalkAnimNpc);

        yield return new WaitForSeconds(5f);

        npcGoap.worldState.Mood = MoodsNpc.Waiting;
        npc.CurrentInteractable = null;
    }
}