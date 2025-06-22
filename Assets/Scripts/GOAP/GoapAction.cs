using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapAction
{
    public string Name;

    public Func<WorldState, bool> Precondition;
    public Func<WorldState, WorldState> Effect;
    public Func<IEnumerator?> Execute;

    public float Cost = 1f; // opcional, para A*

    public override string ToString() => Name;
}

