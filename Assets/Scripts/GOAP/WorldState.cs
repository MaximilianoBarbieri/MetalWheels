using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldState
{
    public bool isAlive;
    public float life;
    public bool carInRange;
    public InteractionType? interactionType;

    public bool hasTalked;
    public bool isSeated;
    public bool isSafe;

    public WorldState Clone()
    {
        return (WorldState)this.MemberwiseClone(); // copia superficial
    }
}