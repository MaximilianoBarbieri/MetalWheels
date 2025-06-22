using System.Collections.Generic;
using FSM;
using UnityEngine;

public class Sitdown_NPC : MonoBaseState
{
    [SerializeField] private NPC npc;

    public override IState ProcessInput()
    {
        return this;
    }

    public override void Enter(IState from, Dictionary<string, object> transitionParameters = null)
    {
    }

    public override Dictionary<string, object> Exit(IState to)
    {
        return base.Exit(to);
    }

    public override void UpdateLoop()
    {
    }
}
