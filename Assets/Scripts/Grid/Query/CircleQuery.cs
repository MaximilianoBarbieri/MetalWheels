using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircleQuery : MonoBehaviour, IQuery
{
    public SpatialGrid targetGrid;
    public float radius = 10f;

    public IEnumerable<IGridEntity> Query()
    {
        float r = radius;
        Vector3 centroCirculo = transform.position;

        return targetGrid.Query(
            transform.position + new Vector3(-r, 0, -r),
            transform.position + new Vector3(r, 0, r),
            x => Vector3.Distance(x, centroCirculo) < radius);
    }

    void OnDrawGizmos()
    {
        if (targetGrid == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}