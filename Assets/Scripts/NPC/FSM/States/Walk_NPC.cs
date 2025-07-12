using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;

public class Walk_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;
    private Coroutine _movementRoutine;

    public override IState ProcessInput() => this;

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
       // npc.animator.SetTrigger(AnimNpc.WalkNpc);

        if (npc.currentNode == null)
        {
            Debug.LogWarning("[WALK] El NPC no tiene un nodo actual.");
            npc.fsm.TransitionTo(npc.idleNpc);
            return;
        }

        int valueSteps = Random.Range(1, npcGoap.worldState.steps + 1);
        _movementRoutine = npc.StartCoroutine(WalkFreely(npc.currentNode, valueSteps));
    }

    private IEnumerator WalkFreely(Node startNode, int valueSteps)
    {
        Node current = startNode;
        int walkedSteps = 0;

        while (walkedSteps < valueSteps)
        {
            var neighbors = current.neighbors;
            if (neighbors == null || neighbors.Count == 0)
                break;

            Node next = neighbors[Random.Range(0, neighbors.Count)];

            while (Vector3.Distance(npc.transform.position, next.transform.position) > 0.1f)
            {
                Vector3 dir = (next.transform.position - npc.transform.position).normalized;
                npc.transform.position += dir * npc.speed * Time.deltaTime;
                npc.transform.forward = Vector3.Lerp(npc.transform.forward, dir, npc.speedRotation * Time.deltaTime);
                yield return null;
            }

            walkedSteps++;
            npcGoap.worldState.steps--;
            current = next;
            npc.currentNode = current;

            if (npcGoap.worldState.steps <= 0)
                break;
        }

        npc.fsm.TransitionTo(npc.idleNpc);
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_movementRoutine != null)
            npc.StopCoroutine(_movementRoutine);
        return base.Exit(to);
    }

    public override void UpdateLoop() { Debug.Log("Estoy en Walk");}
}