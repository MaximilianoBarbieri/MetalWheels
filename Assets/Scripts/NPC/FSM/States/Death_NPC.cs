using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using static AnimNpc;
using static MoodsNpc;

public class Death_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;
    
    private Coroutine _deathRoutine;
    
    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        npc.Animator.SetTrigger(DeathAnimNpc);
        
        npcGoap.WorldState.Mood = Dying;
        
        npcGoap.WorldState.UpdateSpeedByMood();

        _deathRoutine = StartCoroutine(DeathSequence());
    }
    
    public override IState ProcessInput() => this;

    public override void UpdateLoop() { }
    
    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(3f);
        
        Destroy(npc.gameObject);
    }
}