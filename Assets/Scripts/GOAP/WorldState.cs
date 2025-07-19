using System.Linq;

public class WorldState
{
    // La vida del NPC
    public float life;

    // Cantidad de pasos disponibles, cada step es un nodo mas que puede moverse, si no tiene mas, pasa a IDLE por cansancio
    public int steps;
    public int maxsteps;

    // Auto cerca, prioridad sobrevivir
    public bool carInRange;

    // "Calmado", "Asustado", "Curioso", etc.
    public string mood;

    // Para saber como interactuar con los objetos.
    public InteractionType? interactionType;

    public WorldState Clone()
    {
        return (WorldState)this.MemberwiseClone(); // copia superficial
    }

    public bool IsMoodOneOf(params string[] validMoods)
    {
        return validMoods.Contains(mood);
    }

    public bool IsMoodNot(params string[] moods)
    {
        return !moods.Contains(mood);
    }
}