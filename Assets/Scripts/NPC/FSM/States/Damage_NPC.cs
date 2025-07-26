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

    private const int ImpactForce = 10;

    private Coroutine _damageRoutine;

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.Animator.SetTrigger(IdleAnimNpc);

        Debug.Log("El npc " + $"${npc.name}" + " acaba de ser atropellado");

        npcGoap.worldState.Mood = Injured;

        _damageRoutine = StartCoroutine(DamageRoutine());
    }

    public override void UpdateLoop()
    {
    }

    public override IState ProcessInput()
    {
        return this;
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_damageRoutine != null)
            StopCoroutine(_damageRoutine);

        return base.Exit(to);
    }

    private IEnumerator DamageRoutine()
    {
        npc.ModifyLife(npcGoap.worldState.Life, 25);

        var direction = (npc.Rigidbody.transform.position - npcGoap.worldState.CurrentCar.transform.position);
        direction.y = 0f; // anulamos componente Y
        direction = direction.normalized;

        npc.Rigidbody.AddForce(direction * ImpactForce, ForceMode.Impulse);

        yield return new WaitForSeconds(0.25f); // reposo adicional

        npc.Rigidbody.velocity = Vector3.zero;

        yield return new WaitForSeconds(2f); // reposo adicional
        Debug.Log($"Terminó el daño del NPC {npc.name}");

        npcGoap.worldState.Mood = Waiting;
        npcGoap.worldState.CurrentCar = null;
    }
}