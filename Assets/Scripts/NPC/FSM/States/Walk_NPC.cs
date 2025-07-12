using System.Collections.Generic;
using FSM;
using UnityEngine;

public class Walk_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;

    public override IState ProcessInput()
    {
        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
        
        npc.animator.SetTrigger(AnimNpc.WalkNpc);
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
    }
}