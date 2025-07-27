using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using static MoodsNpc;
using UnityEngine;

public class Walk_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;
    private Coroutine _movementRoutine;

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.Animator.SetTrigger(AnimNpc.WalkAnimNpc);
        
        npcGoap.WorldState.Mood = Exploring;

        npcGoap.WorldState.UpdateSpeedByMood();
        
        _movementRoutine = StartCoroutine(DoWalk());
    }

    public override void UpdateLoop()
    {
    }

    private IEnumerator DoWalk()
    {
        Node startNode = npc.CurrentNode;
        Node targetNode = GetTargetNode(startNode);

        yield return npc.MoveTo(targetNode, 
                                npcGoap.WorldState.Speed, 
                                npcGoap.WorldState.SpeedRotation,
                         steps => npcGoap.WorldState.Steps -= steps);

        npc.CurrentInteractable = npc.GetClosestInteractable();
        npcGoap.WorldState.Mood = Waiting;
    }

    private Node GetTargetNode(Node start)
    {
        var zone = NodeGenerator.Instance.GetZoneForNode(start);
        if (zone == null)
            return null;

        var candidates = zone.nodes.Where(n => n != start).ToList();
        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        StopCoroutine(_movementRoutine);

        return base.Exit(to);
    }

    public override IState ProcessInput()
    {
        return this;
    }
}