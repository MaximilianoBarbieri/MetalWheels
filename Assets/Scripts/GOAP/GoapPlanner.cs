using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapPlanner : MonoBehaviour
{
    public static IEnumerable<List<GoapAction>> Plan(
        WorldState start,
        Func<WorldState, bool> goal,
        IEnumerable<GoapAction> actions)
    {
        var frontier = new List<(List<GoapAction> path, WorldState state, float cost)>();
        frontier.Add((new List<GoapAction>(), start, 0));

        while (frontier.Count > 0)
        {
            // Ordenamos por costo acumulado
            frontier = frontier.OrderBy(f => f.cost).ToList();

            var (path, state, currentCost) = frontier[0];
            frontier.RemoveAt(0);

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
                    var newCost = currentCost + action.Cost;
                    frontier.Add((newPath, newState, newCost));
                }
            }
        }
    }

    #region Explain

/*
    GOAP Planner:
    Genera posibles planes (secuencias de acciones) para llegar a un objetivo,
    priorizando aquellos con menor costo acumulado.

    Variables clave:
    - frontier: Lista de nodos por explorar.
      Cada nodo es (camino recorrido, estado actual, costo acumulado).

    Flujo:
    1️⃣ Ordena la frontera por costo (menor primero).
    2️⃣ Toma el nodo de menor costo y lo explora.
    3️⃣ Si el estado actual cumple el objetivo, devuelve el plan.
    4️⃣ Para cada acción posible:
       - Si cumple su precondición:
         - Aplica el efecto (clonando el estado).
         - Suma la acción al camino.
         - Suma el costo de la acción al costo acumulado.
         - Agrega el nuevo nodo a la frontera.
    5️⃣ Repite hasta que no haya nodos o se encuentren planes.
*/

    #endregion

}