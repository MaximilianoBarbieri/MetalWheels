using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using static MoodsNpc;

public class Death_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;
    private Coroutine _deathRoutine;

    public override IState ProcessInput()
    {
        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.animator.SetTrigger(AnimNpc.DeathNpc);
        
        npcGoap.worldState.Mood = Dying;
        npcGoap.worldState.UpdateSpeedByMood();

        _deathRoutine = StartCoroutine(DeathSequence());
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(3f); //Tiempo estimado de la animacion de muerte [No Loopeable]

        Debug.Log("[Death] Animación finalizada. Destruyendo NPC.");
        Destroy(npc.gameObject);
    }
}