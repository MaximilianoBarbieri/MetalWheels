using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using static AnimNpc;
using static MoodsNpc;

public class Sitdown_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    private Coroutine _sitCoroutine;
    private Coroutine _movementRoutine;
    
    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.Animator.SetTrigger(SitdownAnimNpc);

        npcGoap.WorldState.Mood = Relaxed;
    
        npcGoap.WorldState.UpdateSpeedByMood();

        if (npc.currentInteractable != null)
            _sitCoroutine = StartCoroutine(SitRoutine(npc.currentInteractable));
    }
    
    public override IState ProcessInput() => this;
    
    public override void UpdateLoop() { }
    
    public override Dictionary<string, object> Exit(IState to)
    {
        StopCoroutine(_sitCoroutine);
        StopCoroutine(_movementRoutine);

        npc.currentInteractable = null;

        return base.Exit(to);
    }
    
    private IEnumerator SitRoutine(InteractableNPC interactable)
    {
        npc.currentInteractable = null;

        yield return _movementRoutine = StartCoroutine(npc.MoveTo(interactable.assignedNode, 
                                                                        npcGoap.WorldState.Speed, 
                                                                        npcGoap.WorldState.SpeedRotation));

        Vector3 target = interactable.sitTarget.position;
        
        target.y = npc.transform.position.y;

        while (Vector3.Distance(npc.transform.position, target) > 0.05f)
        {
            Vector3 dir = (target - npc.transform.position).normalized;
            npc.transform.position += dir * npcGoap.WorldState.Speed * Time.deltaTime;
            npc.transform.forward = Vector3.Lerp(npc.transform.forward, dir, npcGoap.WorldState.SpeedRotation * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        npcGoap.WorldState.Steps = npcGoap.WorldState.MaxSteps - 1;

        npcGoap.WorldState.Mood = Waiting;
    }
}