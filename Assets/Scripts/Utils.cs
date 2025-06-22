using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
    //NPC
    
    //STATES
    
    public const string ToIdleNpc = "ToIdleNPC";
    public const string ToWalkNpc = "ToWalkNPC";
    public const string ToEscapeNpc = "ToEscapeNPC";
    public const string ToTalkNpc = "ToTalkNPC";
    public const string ToSitdownNpc = "ToSitdownNPC";
    public const string ToDeathNpc = "ToDeathNPC";
    
    //ANIM
    public const string IdleNpc = "IdleNPC";
    public const string WalkNpc = "WalkNPC";
    public const string EscapeNpc = "EscapeNPC";
    public const string TalkNpc = "TalkNPC";
    public const string SitdownNpc = "SitdownNPC";
    public const string DeathNpc = "DeathNPC";
    
    // Transformaciones

    //Clamp básico
    public static int Clamp(int v, int min, int max)
    {
        return v < min ? min : v > max ? max : v;
    }

    //Generator genérico, lo vamos a ver más adelante.
    public static IEnumerable<Src> Generate<Src>(Src seed, Func<Src, Src> generator) {
        while (true) {
            yield return seed;
            seed = generator(seed);
        }
    }
}

public enum InteractionType
{
    Talk,
    Sit
}