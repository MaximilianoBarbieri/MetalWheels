using System.Collections;
using System.Collections.Generic;
using FSM;
using static MoodsNpc;
using UnityEngine;

public class Sitdown_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    private Coroutine _sitCoroutine;
    private Coroutine _movementRoutine;

    public override IState ProcessInput()
    {
        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.Animator.SetTrigger(AnimNpc.SitdownAnimNpc);

        npcGoap.worldState.Mood = Relaxed;
        npcGoap.worldState.UpdateSpeedByMood();

        if (npc.CurrentInteractable != null)
            _sitCoroutine = StartCoroutine(SitRoutine(npc.CurrentInteractable));
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        StopCoroutine(_sitCoroutine);
        StopCoroutine(_movementRoutine);

        npc.CurrentInteractable = null;

        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
        Debug.Log("[Sitdown]");
    }

    private IEnumerator SitRoutine(InteractableNPC interactable)
    {
        npc.CurrentInteractable = null;

        yield return _movementRoutine = StartCoroutine(npc.MoveTo(interactable.assignedNode, 
            npcGoap.worldState.Speed, 
            npcGoap.worldState.SpeedRotation));

        Vector3 target = interactable.sitTarget.position;
        target.y = npc.transform.position.y;

        while (Vector3.Distance(npc.transform.position, target) > 0.05f)
        {
            Vector3 dir = (target - npc.transform.position).normalized;
            npc.transform.position += dir * npcGoap.worldState.Speed * Time.deltaTime;
            npc.transform.forward = Vector3.Lerp(npc.transform.forward, dir, npcGoap.worldState.SpeedRotation * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        npcGoap.worldState.Steps = npcGoap.worldState.MaxSteps - 1;

        npcGoap.worldState.Mood = Waiting;
    }
}