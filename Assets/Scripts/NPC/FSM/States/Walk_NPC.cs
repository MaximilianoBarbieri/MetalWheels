using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        npc.currentNode = NodeGenerator.Instance.GetNodes()
            .Select(go => go.GetComponent<Node>())
            .OrderBy(n => Vector3.Distance(transform.position, n.transform.position))
            .FirstOrDefault(); // Asigna el nodo más cercano

        int valueSteps = Random.Range(1, npcGoap.worldState.steps);

        Debug.Log("El numero de pasos a hacer es de " + $"{valueSteps}");

        _movementRoutine = npc.StartCoroutine(WalkFreely(npc.currentNode, valueSteps));
    }

    private IEnumerator WalkFreely(Node startNode, int valueSteps)
    {
        Node current = startNode;
        int walkedSteps = 0;
        npcGoap.worldState.steps = valueSteps;

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

        yield break;
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_movementRoutine != null)
            npc.StopCoroutine(_movementRoutine);

        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
        Debug.Log("[Walk]Mi cantidad de pasos disponibles es de: " + $"{npcGoap.worldState.steps}");
    }
}