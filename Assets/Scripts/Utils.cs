using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    // Transformaciones

    //Clamp básico
    public static int Clamp(int v, int min, int max)
    {
        return v < min ? min : v > max ? max : v;
    }

    //Generator genérico, lo vamos a ver más adelante.
    public static IEnumerable<Src> Generate<Src>(Src seed, Func<Src, Src> generator)
    {
        while (true)
        {
            yield return seed;
            seed = generator(seed);
        }
    }
}

public static class MoodsNpc
{
    public const string Relaxed = "RelaxedNPC";
    public const string Safe = "SafeNPC";
    public const string Curious = "CuriousNPC";
}

public static class StatesNpc
{
    public const string ToIdleNpc = "ToIdleNPC";
    public const string ToWalkNpc = "ToWalkNPC";
    public const string ToEscapeNpc = "ToEscapeNPC";
    public const string ToTalkNpc = "ToTalkNPC";
    public const string ToSitdownNpc = "ToSitdownNPC";
    public const string ToDeathNpc = "ToDeathNPC";
}

public static class AnimNpc
{
    public const string IdleNpc = "IdleNPC";
    public const string WalkNpc = "WalkNPC";
    public const string EscapeNpc = "EscapeNPC";
    public const string TalkNpc = "TalkNPC";
    public const string SitdownNpc = "SitdownNPC";
    public const string DeathNpc = "DeathNPC";
}

public enum InteractionType
{
    Talk,
    Sit,
    NoInteractable
}