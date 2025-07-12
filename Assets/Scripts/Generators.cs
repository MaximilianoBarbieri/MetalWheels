using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Generators
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
    
    public static IEnumerable<T> DetectNearby<T>(Vector3 position, GameObject self, float radius) where T : Component
    {
        Collider[] hits = Physics.OverlapSphere(position, radius);
        foreach (var hit in hits)
        {
            if (hit.gameObject == self) continue;

            T component = hit.GetComponent<T>();
            if (component != null)
                yield return component;
        }
    }
}

public static class MoodsNpc
{
    public const string Relaxed = "RelaxedNPC";
    public const string Safe = "SafeNPC";
    public const string NotSafe = "NotSafeNPC";
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

public static class GoapActionName
{
    public const string IdleGoapNpc = "Idle";
    public const string WalkGoapNpc = "Walk";
    public const string EscapeGoapNpc = "Escape";
    public const string TalkGoapNpc = "Talk";
    public const string SitdownGoapNpc = "Sitdown";
    public const string DeathGoapNpc = "Death";
}

public enum InteractionType
{
    Talk,
    Sit,
    OnlyForPath
}