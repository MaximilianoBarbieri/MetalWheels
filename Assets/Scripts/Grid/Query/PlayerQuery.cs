using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;

public class PlayerQuery : NetworkBehaviour, IQuery
{
    public SpatialGrid targetGrid;
    //public float                   width    = 15f;
    //public float                   height   = 30f;

    public float radius = 25f;

    public IEnumerable<IGridEntity> Selected = new List<IGridEntity>();
    
    public override void Spawned()
    {
        if (targetGrid != null)
        {
            targetGrid.Initialize(); // ← asegurás que esté listo
        }
    }

    public IEnumerable<IGridEntity> Query()
    {
        float r = radius;
        Vector3 centerCircle = transform.position;
        return targetGrid.Query(
            transform.position + new Vector3(-r, 0, -r),
            transform.position + new Vector3(r, 0, r),
            x => Vector3.Distance(x, centerCircle) < radius);
    }

    private void FixedUpdate()
    {
        var result1 = Query().ToList();

        Debug.Log($"Hay {result1.Count()} entidades en peligro!");

        var result2 = Query().Select(x => (NPCGoap)x)
            .Where(x => x != null);


        Debug.Log($"Hay {result2.Count()} NPC-S en peligro!");

        foreach (var entity in result2)
        {
            //entity.WorldState.CarInRange = true;
        }
    }

    void OnDrawGizmos()
    {
        if (targetGrid == null) return;

        //Flatten the sphere we're going to draw
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}