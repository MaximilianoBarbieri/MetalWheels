using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Node : NetworkBehaviour
{
    [Tooltip("Tipo de interacción que este nodo representa (por ejemplo, Sit o Talk, o solo para caminar)")]
    public InteractionType type;

    [Header("Visualización")] [Tooltip("Tamaño visual del nodo en la escena")]
    public float gizmoSize = 0.2f;

    [Tooltip("Color de las líneas que conectan los nodos vecinos al seleccionar este objeto")]
    public List<Node> neighbors = new();

    public Color neighborColor;

    public bool hasCar { get; private set; }
    private readonly HashSet<GameObject> carsInside = new();

    public void DetectNeighbors(float radius)
    {
        neighbors.Clear();

        foreach (var neighbor in Generators.DetectNearby<Node>(transform.position, gameObject, radius + 1))
        {
            if (!neighbors.Contains(neighbor))
                neighbors.Add(neighbor);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position + Vector3.up * 0.1f, Vector3.one * gizmoSize);

        Gizmos.color = hasCar ? Color.red : Color.green;
        Gizmos.DrawCube(transform.position + Vector3.up * 0.1f, Vector3.one * gizmoSize);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = neighborColor;

        foreach (var neighbor in neighbors)
        {
            if (neighbor != null)
                Gizmos.DrawLine(transform.position + Vector3.up * 0.1f,
                    neighbor.transform.position + Vector3.up * 0.1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterController>(out var car))
        {
            carsInside.Add(car.gameObject);
            hasCar = true;
            UpdateColor(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterController>(out var car))
        {
            carsInside.Remove(car.gameObject);
            hasCar = carsInside.Count > 0;
            UpdateColor(hasCar);
        }
    }

    private void UpdateColor(bool danger)
    {
        neighborColor = danger ? Color.red : Color.green;
    }
}