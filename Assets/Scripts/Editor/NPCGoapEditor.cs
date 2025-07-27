using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCGoap))]
public class NPCGoapEditor : Editor
{
    private float damage = 10f;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("[Depurar Vida]", EditorStyles.boldLabel);

        NPCGoap goap = (NPCGoap)target;
        NPC npc = goap.GetComponent<NPC>();

        if (npc != null)
        {
            // Mostrar barra de vida
            float life = goap.WorldState.Life;
            float lifePercent = Mathf.Clamp01(life / 100f);
            EditorGUILayout.LabelField("Vida actual: " + life);
            Rect rect = GUILayoutUtility.GetRect(100, 20);
            EditorGUI.ProgressBar(rect, lifePercent, (lifePercent * 100f).ToString("F0") + "%");

            // Advertencia si está por morir
            if (life <= 20f)
            {
                EditorGUILayout.HelpBox("¡La vida del NPC es críticamente baja!", MessageType.Error);
            }
            else if (life <= 50f)
            {
                EditorGUILayout.HelpBox("Advertencia: la vida del NPC está por debajo del 50%", MessageType.Warning);
            }

            // Input de daño y botón
            damage = EditorGUILayout.FloatField("Cantidad de daño", damage);
            if (GUILayout.Button("Aplicar daño al NPC"))
            {
                goap.WorldState.Life -= damage;
                goap.WorldState.Life = Mathf.Max(0f, goap.WorldState.Life); // evitar negativos
                Debug.Log($"Se aplicaron {damage} puntos de daño. Vida actual: {goap.WorldState.Life}");
            }
        }
        else
        {
            EditorGUILayout.HelpBox("NPC no encontrado en este GameObject.", MessageType.Error);
        }
    }
}