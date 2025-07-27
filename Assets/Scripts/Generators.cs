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

public class MoodsNpc
{
    public const string Relaxed = "RelaxedNPC";
    public const string Waiting = "WaitingNPC";
    public const string Curious = "CuriousNPC";
    public const string LightRest = "LightRestNPC";

    public const string Exploring = "ExploringNPC";

    public const string NotSafe = "NotSafeNPC";
    public const string Injured = "InjuredNPC";
    public const string Dying = "DyingNPC";
}

public static class AnimNpc
{
    public const string IdleAnimNpc = "IdleNPC";
    public const string WalkAnimNpc = "WalkNPC";
    public const string EscapeAnimNpc = "EscapeNPC";
    public const string TalkAnimNpc = "TalkNPC";
    public const string SitdownAnimNpc = "SitdownNPC";
    public const string DamageAnimNpc = "DamageNPC";
    public const string DeathAnimNpc = "DeathNPC";
}

public static class GoapActionName
{
    public const string IdleGoapNpc = "Idle";
    public const string WalkGoapNpc = "Walk";
    public const string EscapeGoapNpc = "Escape";
    public const string TalkGoapNpc = "Talk";
    public const string SitdownGoapNpc = "Sitdown";
    public const string DamageGoapNpc = "Damage";
    public const string DeathGoapNpc = "Death";
}

public enum InteractionType
{
    Talk,
    Sit,
    OnlyForPath
}