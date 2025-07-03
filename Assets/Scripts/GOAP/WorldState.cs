using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldState
{
    public float life; // La vida del NPC
    public int steps; // Cantidad de pasos disponibles, cada step es un nodo mas que puede moverse, si no tiene mas, pasa a IDLE por cansancio
    //public float stamina; //OPCIONAL -> Cuando escapa no gastara pasos, pero si stamina [Como efecto, se volvera mas lento]
    public bool carInRange; // Auto cerca, prioridad sobrevivir
    public string mood;  // "Calmado", "Asustado", "Curioso", etc.

    public InteractionType? interactionType; // Para saber como interactuar con los objetos.

    //public bool hasTalked;
    //public bool isSeated;
    //public bool isSafe;

    public WorldState Clone()
    {
        return (WorldState)this.MemberwiseClone(); // copia superficial
    }
}