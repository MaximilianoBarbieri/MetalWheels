using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class AStar<T>
{
    public event Action<IEnumerable<T>> OnPathCompleted;
    public event Action OnCantCalculate;

    public IEnumerator Run(
        T start,
        Func<T, bool> isGoal,
        Func<T, IEnumerable<WeightedNode<T>>> explode,
        Func<T, float> getHeuristic,
        Func<bool> cancelCondition = null // <- Nuevo parámetro opcional
    )
    {
        var queue = new PriorityQueue<T>();
        var distances = new Dictionary<T, float>();
        var parents = new Dictionary<T, T>();
        var visited = new HashSet<T>();

        distances[start] = 0;
        queue.Enqueue(new WeightedNode<T>(start, 0));

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        while (!queue.IsEmpty)
        {
            if (cancelCondition != null && cancelCondition())
            {
                UnityEngine.Debug.LogWarning("A* cancelado externamente.");
                yield break;
            }

            var dequeued = queue.Dequeue();
            visited.Add(dequeued.Element);

            if (isGoal(dequeued.Element))
            {
                var path = CommonUtils.CreatePath(parents, dequeued.Element);
                OnPathCompleted?.Invoke(path);
                yield break;
            }

            var toEnqueue = explode(dequeued.Element);

            foreach (var transition in toEnqueue)
            {
                if (cancelCondition != null && cancelCondition())
                {
                    UnityEngine.Debug.LogWarning("A* cancelado durante expansión de nodos.");
                    yield break;
                }

                var neighbour = transition.Element;
                var neighbourToDequeuedDistance = transition.Weight;

                var startToNeighbourDistance =
                    distances.ContainsKey(neighbour) ? distances[neighbour] : float.MaxValue;
                var startToDequeuedDistance = distances[dequeued.Element];

                var newDistance = startToDequeuedDistance + neighbourToDequeuedDistance;

                if (!visited.Contains(neighbour) && startToNeighbourDistance > newDistance)
                {
                    distances[neighbour] = newDistance;
                    parents[neighbour] = dequeued.Element;

                    queue.Enqueue(new WeightedNode<T>(neighbour, newDistance + getHeuristic(neighbour)));
                }

                if (stopwatch.ElapsedMilliseconds < 1f / 60f)
                {
                    yield return null;
                    stopwatch.Restart();
                }
            }
        }

        OnCantCalculate?.Invoke();
    }
}
