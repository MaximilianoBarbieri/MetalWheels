using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fusion;
using Unity.VisualScripting;

public class PlayerQuery : NetworkBehaviour, IQuery
{
    public SpatialGrid targetGrid;
    public float radius = 25f;
    public float dangerDuration = 1f;

    private Dictionary<NPC, float> activeNpcTimers = new();
    public List<NPC> CurrentlyInDanger = new();

    private void Start()
    {
        targetGrid = FindObjectOfType<SpatialGrid>();

        if (targetGrid == null)
        {
            Debug.LogError("[PlayerQuery] No se encontró SpatialGrid en la escena.");
            return;
        }

        targetGrid.RegisterQuerys(this);
        
        targetGrid.Initialize();
    }


    private void FixedUpdate()
    {
        float now = Time.time;
        var npcsInRange = Query().OfType<NPC>().ToList();

        foreach (var npc in npcsInRange)
        {
            if (!activeNpcTimers.ContainsKey(npc))
            {
                CurrentlyInDanger.Add(npc);
               // Debug.Log($"[PlayerQuery] Añadido: {npc.name}");
            }

            activeNpcTimers[npc] = now + dangerDuration;
        }

        // Remover los que ya expiraron
        var expired = activeNpcTimers
            .Where(pair => pair.Value <= now)
            .Select(pair => pair.Key)
            .ToList();

        foreach (var npc in expired)
        {
            activeNpcTimers.Remove(npc);
            CurrentlyInDanger.Remove(npc);
//            Debug.Log($"[PlayerQuery] Removido: {npc.name}");
        }
    }

    public IEnumerable<IGridEntity> Query()
    {
        Vector3 center = transform.position;
        float r = radius;

        return targetGrid.Query(
            center + new Vector3(-r, 0, -r),
            center + new Vector3(r, 0, r),
            x => Vector3.Distance(x, center) < r
        );
    }
}

