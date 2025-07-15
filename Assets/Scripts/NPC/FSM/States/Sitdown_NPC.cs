using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;

public class Sitdown_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    private Coroutine _sitCoroutine;
    private Coroutine _movementCoroutine;

    public override IState ProcessInput()
    {
        Debug.Log("[Sitdown]");

        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        //npc.animator.SetTrigger(AnimNpc.SitdownNpc);

        if (npc.currentInteractable != null)
            _sitCoroutine = StartCoroutine(SitRoutine(npc.currentInteractable));
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_sitCoroutine != null || _movementCoroutine != null)
        {
            StopCoroutine(_sitCoroutine);
            StopCoroutine(_movementCoroutine);
        }

        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
    }

    private IEnumerator SitRoutine(InteractableNPC interactable)
    {
        npc.currentInteractable = null;

        yield return _movementCoroutine = StartCoroutine(npc.MoveAlongPath(npc.currentNode, interactable.assignedNode));

        Vector3 target = interactable.sitTarget.position;
        target.y = npc.transform.position.y;

        while (Vector3.Distance(npc.transform.position, target) > 0.05f)
        {
            Vector3 dir = (target - npc.transform.position).normalized;
            npc.transform.position += dir * npc.speed * Time.deltaTime;
            npc.transform.forward = Vector3.Lerp(npc.transform.forward, dir, npc.speedRotation * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        npcGoap.worldState.steps = npcGoap.worldState.maxsteps - 1;

        npcGoap.worldState.mood = MoodsNpc.Relaxed;

        Debug.Log("Finalizo corrutina [Sit]");
    }
}