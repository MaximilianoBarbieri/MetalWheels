using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSM;
using Fusion;
using UnityEngine;
using static MoodsNpc;

public class StaySafe_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;
    [SerializeField] private NPCGoap npcGoap;

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
    //    npcGoap.worldState.mood = Safe;
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
    }

    public override IState ProcessInput()
    {
        return this;
    }
}