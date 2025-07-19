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

    private Coroutine _escapeRoutine;
    private Coroutine _moveRoutine;

    [SerializeField] private Node _targetNode;
    private CharacterController _currentCar;

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Debug.Log("Enter [Escape]");

        npc.animator.SetTrigger(AnimNpc.EscapeNpc);
        npcGoap.worldState.mood = NotSafe;

        var currentZone = NodeGenerator.Instance.GetZoneForNode(npc.CurrentNode);

        if (currentZone == null || currentZone.IsSafe)
        {
            Debug.Log("[Escape] Zona ya segura. No hace falta escapar.");
            npcGoap.worldState.mood = Waiting;
            return;
        }

        _targetNode = FindEscapeNode(npc.CurrentNode);

        _escapeRoutine = StartCoroutine(EscapeRoutine());
    }

    public override IState ProcessInput()
    {
        return this;
    }

    private IEnumerator EscapeRoutine()
    {
        yield return _moveRoutine = StartCoroutine(npc.MoveAlongPath(npc.CurrentNode, _targetNode));
    }

    public override void UpdateLoop()
    {
        if (!npcGoap.worldState.carInRange)
        {
            npcGoap.worldState.mood = Waiting;

            Debug.Log("[Escape] Ya no hay coche cerca. Cancelando Escape.");
        }
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        StopCoroutine(_moveRoutine);
        StopCoroutine(_escapeRoutine);

        _moveRoutine = null;
        _escapeRoutine = null;

        return base.Exit(to);
    }

    private Node FindEscapeNode(Node currentNode)
    {
        var currentPos = currentNode.transform.position;

        var safeZones = NodeGenerator.Instance.zones.Where(z => z.IsSafe);

        return safeZones
            .SelectMany(z => z.nodes)
            .Where(n => n.neighbors != null && n.neighbors.Count > 0)
            .OrderByDescending(n =>
            {
                Vector3 dirToNode = (n.transform.position - currentPos).normalized;
                Vector3 opposite = -npc.transform.forward; // suponemos que el NPC mira al auto
                float dirScore = Vector3.Dot(dirToNode, opposite);
                float distScore = Vector3.Distance(n.transform.position, currentPos);
                return dirScore * 0.6f + distScore * 0.4f;
            })
            .FirstOrDefault();
    }

}