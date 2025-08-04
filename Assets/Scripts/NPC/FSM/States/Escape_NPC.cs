using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using static AnimNpc;
using static MoodsNpc;

public class Escape_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    private Coroutine _escapeRoutine;

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.Animator.SetTrigger(EscapeAnimNpc);

        npcGoap.WorldState.Mood = NotSafe;
     
        npcGoap.WorldState.UpdateSpeedByMood();

        _escapeRoutine = npc.StartCoroutine(EscapeLoop());
    }
    
    public override IState ProcessInput() => this;

    public override void UpdateLoop() { }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_escapeRoutine != null)
            npc.StopCoroutine(_escapeRoutine);
        
        return base.Exit(to);
    }
    
    private IEnumerator EscapeLoop()
    {
        while (true)
        {
            if (npcGoap.WorldState.Impacted)
            {
                Debug.Log("[Escape] NPC fue impactado durante el escape. Replanificando...");
                yield break;
            }

            if (!npcGoap.WorldState.CarInRange)
            {
                Debug.Log("[Escape] Ya no hay coche cerca. Terminando escape.");
                npcGoap.WorldState.Mood = Waiting;
                yield break;
            }

            var direction = npc.transform.position - npcGoap.WorldState.DirectionToFly;
            
            direction.y = 0f;
            
            direction.Normalize();

            npc.transform.position += direction * npcGoap.WorldState.Speed * Time.deltaTime;
            
            npc.transform.forward = Vector3.Lerp(npc.transform.forward, 
                                                 direction, 
                                                 npcGoap.WorldState.SpeedRotation * Time.deltaTime);
            yield return null;
        }
    }
}