using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using UnityEngine;
using static MoodsNpc;

public class Escape_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    private Node targetNode;
    private Coroutine escapeRoutine;

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Debug.Log("Enter [Escape]");
        npc.animator.SetTrigger(AnimNpc.EscapeNpc);

        npcGoap.worldState.Mood = NotSafe;
        npcGoap.worldState.UpdateSpeedByMood();

        targetNode = FindSafeNeighborZoneNode(npc.CurrentNode);

        if (targetNode == null)
        {
            Debug.LogWarning("[Escape] No hay zona vecina segura disponible.");
            npcGoap.worldState.Mood = Waiting;
            return;
        }

        escapeRoutine = StartCoroutine(EscapeRoutine(targetNode));
    }

    public override void UpdateLoop()
    {
        if (!npcGoap.worldState.CarInRange && escapeRoutine != null)
        {
            StopEscape();
            npcGoap.worldState.Mood = Waiting;
            Debug.Log("[Escape] Ya no hay coche cerca.");
        }
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        StopEscape();
        return base.Exit(to);
    }

    public override IState ProcessInput() => this;

    private void StopEscape()
    {
        if (escapeRoutine != null)
        {
            StopCoroutine(escapeRoutine);
            escapeRoutine = null;
        }
    }

    private Node FindSafeNeighborZoneNode(Node currentNode)
    {
        var currentZone = NodeGenerator.Instance.GetZoneForNode(currentNode);
        if (currentZone == null) return null;

        var safeZones = currentZone.neighbors.Where(z => z.IsSafe);

        var candidates = safeZones
            .SelectMany(z => z.nodes)
            .Where(n => n.neighbors.Any(nb => currentZone.nodes.Contains(nb))) // frontera
            .ToList();

        if (candidates.Count == 0) return null;

        return candidates
            .OrderByDescending(n =>
            {
                Vector3 dirToNode = (n.transform.position - currentNode.transform.position).normalized;
                Vector3 opposite = -npc.transform.forward;
                float dirScore = Vector3.Dot(dirToNode, opposite);
                float distScore = Vector3.Distance(n.transform.position, currentNode.transform.position);
                return dirScore * 0.6f + distScore * 0.4f;
            })
            .FirstOrDefault();
    }

    private IEnumerator EscapeRoutine(Node destination)
    {
        yield return npc.MoveTo(destination,
            npcGoap.worldState.Speed,
            npcGoap.worldState.SpeedRotation);

        // Reevaluar si sigue en peligro
        if (npcGoap.worldState.CarInRange)
        {
            var newTarget = FindSafeNeighborZoneNode(npc.CurrentNode);
            if (newTarget != null && newTarget != destination)
            {
                escapeRoutine = StartCoroutine(EscapeRoutine(newTarget));
                yield break;
            }
        }

        npcGoap.worldState.Mood = Waiting;
    }
}