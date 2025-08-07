using System;
using System.Collections.Generic;
using Fusion;
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
    
    /// <summary>
    /// Devuelve una función que configura el WorldState con un nuevo mood, velocidad y FX.
    /// </summary>
    public static Action<NPCGoap, NPC> SetupStateWithAnimation(string mood, string animationTrigger, Texture2D moodImage)
    {
        return (goap, npc) =>
        {
            npc.Animator.SetTrigger(animationTrigger);
            goap.WorldState.Mood = mood;
            goap.WorldState.UpdateSpeedByMood();
            goap.WorldState.UpdateMoodsFX(npc, moodImage);
        };
    }
    
    public static IEnumerable<NetworkObject> GenerateNodes(
        Vector2Int gridSize,
        float nodeSpacing,
        Vector3 origin,
        GameObject nodePrefab,
        NetworkRunner runner)
    {
        float y = origin.y;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                Vector3 pos = new Vector3(origin.x + x * nodeSpacing, y, origin.z + z * nodeSpacing);
                NetworkObject node = runner.Spawn(nodePrefab, pos, Quaternion.identity, runner.LocalPlayer, null);
                node.name = $"Node_{x}_{z}";
                yield return node;
            }
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

public static class NPCStats
{
    public const float DISTANCE_TO_IMPACT = 1f;
}

public enum InteractionType
{
    Talk,
    Sit,
    OnlyForPath
}