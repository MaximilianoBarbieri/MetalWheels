using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using UnityEngine.Serialization;
using static AnimNpc;
using static MoodsNpc;

public class Damage_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    [SerializeField] private ParticleSystem injuredFX;

    [SerializeField] private Texture2D moodImage;

    private Coroutine _damageRoutine;

    private const int ImpactForce = 10;

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Generators.SetupStateWithAnimation(Injured,
                                   IdleAnimNpc, 
                                                moodImage) 
            (npcGoap, npc);

        
        injuredFX.Play();

        _damageRoutine = StartCoroutine(DamageRoutine());
    }

    public override IState ProcessInput() => this;

    public override void UpdateLoop()
    {
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_damageRoutine != null)
            StopCoroutine(_damageRoutine);

        injuredFX.Stop();

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

        npcGoap.WorldState.DirectionToFly = Vector3.zero;
        
        if (!(npcGoap.WorldState.Life > 0)) yield break;

        npcGoap.WorldState.Mood = Waiting;

    }
}