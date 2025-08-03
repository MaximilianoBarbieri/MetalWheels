using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using JetBrains.Annotations;
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

        escapeRoutine = npc.StartCoroutine(EscapeLoop());
    }


    public override void UpdateLoop()
    {
    }

    public override Dictionary<string, object> Exit(IState to)
    {

        return base.Exit(to);
    }

    private void StopEscape()
    {
        StopCoroutine(escapeRoutine);
    }
    
    public override IState ProcessInput() => this;

    private IEnumerator EscapeLoop()
    {
        while (true)
        {
            if (npcGoap.WorldState.Impacted)
            {
                Debug.Log("[Escape] Impactado durante escape. Deteniendo y replanteando...");
                npcGoap.ForceReplan(); // ⚠️ Asegurate que este método cancele el plan actual
                yield break;
            }

            if (!npcGoap.WorldState.CarInRange)
            {
                Debug.Log("[Escape] Ya no hay coche cerca. Terminando escape.");
                npcGoap.WorldState.Mood = Waiting;
                yield break;
            }

            Vector3 direction = npc.transform.position - npcGoap.WorldState.DirectionToFly;
            direction.y = 0f;
            direction.Normalize();

            npc.transform.position += direction * npcGoap.WorldState.Speed * Time.deltaTime;
            npc.transform.forward = Vector3.Lerp(npc.transform.forward, direction, npcGoap.WorldState.SpeedRotation * Time.deltaTime);

            yield return null;
        }
        
    }


}