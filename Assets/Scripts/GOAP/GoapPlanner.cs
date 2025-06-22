using System;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlanner : MonoBehaviour
{
    public static IEnumerable<List<GoapAction>> Plan(
        WorldState start,
        Func<WorldState, bool> goal,
        IEnumerable<GoapAction> actions)
    {
        var frontier = new Queue<(List<GoapAction> path, WorldState state)>();
        frontier.Enqueue((new List<GoapAction>(), start));

        while (frontier.Count > 0)
        {
            var (path, state) = frontier.Dequeue();

            if (goal(state))
            {
                yield return path;
                continue;
            }

            foreach (var action in actions)
            {
                if (action.Precondition(state))
                {
                    var newState = action.Effect(state.Clone());
                    var newPath = new List<GoapAction>(path) { action };
                    frontier.Enqueue((newPath, newState));
                }
            }
        }
    }
}
