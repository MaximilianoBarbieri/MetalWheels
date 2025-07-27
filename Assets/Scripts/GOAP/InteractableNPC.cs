using Fusion;
using UnityEngine;

public class InteractableNPC : NetworkBehaviour
{
    public InteractionType type;
    public Transform sitTarget;
    public Node assignedNode;

    // private void Start() => NodeGenerator.Instance.Register(this);

    public void AssignClosestNode(float radius)
    {
        float minDist = float.MaxValue;
        Node closest = null;

        foreach (var node in Generators.DetectNearby<Node>(transform.position, gameObject, radius))
        {
            float dist = Vector3.Distance(transform.position, node.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        if (closest == null) return;

        closest.type = type;
        assignedNode = closest;

//        Debug.Log("El " +$"{gameObject.name}" + " ha sido asociado al nodo " + $"{assignedNode}");
    }

    private void OnDrawGizmosSelected()
    {
        if (assignedNode != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.2f,
                assignedNode.transform.position + Vector3.up * 0.2f);
        }
    }

    private void OnEnable()
    {
        NodeGenerator.OnGameReady += () => { NodeGenerator.Instance.Register(this); };
    }
}