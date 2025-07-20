using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class AStar
{
    public static IEnumerator CalculatePath<T>(
        T start,
        Func<T, bool> isGoal,
        Func<T, IEnumerable<WeightedNode<T>>> explode,
        Func<T, float> getHeuristic,
        Action<List<T>> onComplete,
        Action onFail = null,
        Func<bool> cancelCondition = null)
    {
        var queue = new PriorityQueue<T>();
        var distances = new Dictionary<T, float>();
        var parents = new Dictionary<T, T>();
        var visited = new HashSet<T>();

        distances[start] = 0;
        queue.Enqueue(new WeightedNode<T>(start, 0));

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        while (!queue.IsEmpty)
        {
            if (cancelCondition != null && cancelCondition())
            {
                UnityEngine.Debug.LogWarning("A* cancelado externamente.");
                onFail?.Invoke();
                yield break;
            }

            var dequeued = queue.Dequeue();
            visited.Add(dequeued.Element);

            if (isGoal(dequeued.Element))
            {
                var path = CommonUtils.CreatePath(parents, dequeued.Element).ToList();
                onComplete?.Invoke(path);
                yield break;
            }

            foreach (var transition in explode(dequeued.Element))
            {
                var neighbor = transition.Element;
                var cost = transition.Weight;
                var newDistance = distances[dequeued.Element] + cost;

                if (!visited.Contains(neighbor) && (!distances.ContainsKey(neighbor) || distances[neighbor] > newDistance))
                {
                    distances[neighbor] = newDistance;
                    parents[neighbor] = dequeued.Element;
                    float priority = newDistance + getHeuristic(neighbor);
                    queue.Enqueue(new WeightedNode<T>(neighbor, priority));
                }

                if (stopwatch.ElapsedMilliseconds < 1f / 60f)
                {
                    yield return null;
                    stopwatch.Restart();
                }
            }
        }

        onFail?.Invoke();
    }
}
