using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InteractableNPC))]
public class InteractableNPCEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Obtener la referencia al componente que se está editando
        InteractableNPC npc = (InteractableNPC)target;

        // Mostrar el campo 'type' como dropdown
        npc.type = (InteractionType)EditorGUILayout.EnumPopup("Type", npc.type);

        // Solo mostrar 'sitTarget' si el tipo es 'Sit'
        if (npc.type == InteractionType.Sit)
        {
            npc.sitTarget = (Transform)EditorGUILayout.ObjectField("Sit Target", npc.sitTarget, typeof(Transform), true);
        }

        // Mostrar siempre el nodo asignado
        npc.assignedNode = (Node)EditorGUILayout.ObjectField("Assigned Node", npc.assignedNode, typeof(Node), true);

        // Marcar el objeto como modificado si hubo cambios
        if (GUI.changed)
        {
            EditorUtility.SetDirty(npc);
        }
    }
}