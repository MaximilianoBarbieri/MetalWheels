using System;
using System.Collections;

public class GoapAction
{
    public string Name;

    public Func<WorldState, bool> Precondition;
    public Func<WorldState, WorldState> Effect;
    public Func<IEnumerator?> Execute;

    public int Cost;

    public override string ToString() => Name;
}

