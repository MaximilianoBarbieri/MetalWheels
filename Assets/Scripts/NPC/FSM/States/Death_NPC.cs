using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;

public class Death_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    private Coroutine _deathRoutine;

    public override IState ProcessInput()
    {
        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.animator.SetTrigger(AnimNpc.DeathNpc);
        _deathRoutine = StartCoroutine(DeathSequence());
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        if (_deathRoutine != null)
            StopCoroutine(_deathRoutine);

        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(2f); //Tiempo estimado de la animacion de muerte [No Loopeable]

        Debug.Log("[Death] Animación finalizada. Destruyendo NPC.");
        Destroy(npc.gameObject);
    }
}