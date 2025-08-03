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

    private Coroutine dangerTimeoutRoutine; // NUEVO

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Debug.Log("Enter [Escape]");
        npc.Animator.SetTrigger(AnimNpc.EscapeAnimNpc);

        npcGoap.WorldState.Mood = NotSafe;
        npcGoap.WorldState.UpdateSpeedByMood();

        dangerTimeoutRoutine = npc.StartCoroutine(DangerCooldown(3f)); // NUEVO

        if (targetNode == null)
        {
            Debug.LogWarning("[Escape] No hay zona vecina segura disponible.");
            npcGoap.WorldState.Mood = Waiting;
            return;
        }

        // escapeRoutine = StartCoroutine(EscapeRoutine(targetNode));
    }

    public override void UpdateLoop()
    {
        //if (!npcGoap.WorldState.CarInRange && escapeRoutine != null)
        //{
        //    StopEscape();
        //    npcGoap.WorldState.Mood = Waiting;
        //    Debug.Log("[Escape] Ya no hay coche cerca.");
        //}
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

        if (dangerTimeoutRoutine != null) // NUEVO
        {
            StopCoroutine(dangerTimeoutRoutine);
            dangerTimeoutRoutine = null;
        }
    }

    private IEnumerator DangerCooldown(float duration) // NUEVO
    {
        yield return new WaitForSeconds(duration);
        //npcGoap.WorldState.CarInRange = false;
        Debug.Log($"[Escape] Timeout: peligro despejado → CarInRange = false");
    }
}
