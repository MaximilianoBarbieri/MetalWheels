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
}


//public bool hasTalked;
//public bool isSeated;
//public bool isSafe;

//public float stamina; //OPCIONAL -> Cuando escapa no gastara pasos, pero si stamina [Como efecto, se volvera mas lento]