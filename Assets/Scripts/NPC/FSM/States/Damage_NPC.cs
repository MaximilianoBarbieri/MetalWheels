using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using static AnimNpc;
using static MoodsNpc;

public class Damage_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    private Coroutine _damageRoutine;

    private const int ImpactForce = 10;

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.Animator.SetTrigger(IdleAnimNpc);
        
        npcGoap.WorldState.Mood = Injured;

        npcGoap.WorldState.UpdateSpeedByMood();
        
        _damageRoutine = StartCoroutine(DamageRoutine());
    }

    public override IState ProcessInput() => this;

    public override void UpdateLoop() { }
    
    public override Dictionary<string, object> Exit(IState to)
    {
        if (_damageRoutine != null)
            StopCoroutine(_damageRoutine);

        return base.Exit(to);
    }

    private IEnumerator DamageRoutine()
    {
        npcGoap.WorldState.Life -= 25;

        var direction = (npc.Rigidbody.transform.position - npcGoap.WorldState.DirectionToFly);
        
        direction.y = 0f;
        
        direction = direction.normalized;

        npc.Rigidbody.AddForce(direction * ImpactForce, ForceMode.Impulse);

        yield return new WaitForSeconds(0.25f);

        npc.Rigidbody.velocity = Vector3.zero;

        yield return new WaitForSeconds(2f);

        if (!(npcGoap.WorldState.Life > 0)) yield break;
        
        npcGoap.WorldState.Mood = Waiting;
            
        npcGoap.WorldState.DirectionToFly = Vector3.zero;
    }
}