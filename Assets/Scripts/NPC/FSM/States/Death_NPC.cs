using System.Collections;
using System.Collections.Generic;
using FSM;
using UnityEngine;
using UnityEngine.Serialization;
using static AnimNpc;
using static MoodsNpc;

public class Death_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;
    
    [SerializeField] private Texture2D moodImage;

    private Coroutine _deathRoutine;
    
    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        Generators.SetupStateWithAnimation(Dying,
                                   DeathAnimNpc, 
                                                moodImage)
            (npcGoap, npc);

        
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