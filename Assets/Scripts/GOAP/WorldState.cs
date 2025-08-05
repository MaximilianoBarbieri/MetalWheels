using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldState
{
    public float Life;
    public float Speed;
    public float SpeedRotation;
    
    public int Steps;
    public int MaxSteps;
    
    public bool CarInRange;
    public bool Impacted;
    
    public string Mood;
    
    public Vector3 DirectionToFly;
    
    public InteractionType? InteractionType;

    private readonly Dictionary<string, float> _moodSpeeds = new()
    {
        { MoodsNpc.Waiting,         0f },
        { MoodsNpc.LightRest,       0f },
        { MoodsNpc.Exploring,       1.5f },
        { MoodsNpc.Relaxed,         2f },
        { MoodsNpc.Curious,         0.5f },
        { MoodsNpc.NotSafe,         6f },
        { MoodsNpc.Injured,         1f },
        { MoodsNpc.Dying,           0f }
    };


    public void UpdateSpeedByMood()
    {
        if (_moodSpeeds.TryGetValue(Mood, out float newSpeed))
            Speed = newSpeed;
    }

    public WorldState Clone() => (WorldState)this.MemberwiseClone();
    
    public bool IsMoodOneOf(params string[] validMoods) => validMoods.Contains(Mood);
    
    public bool IsMoodNot(params string[] moods) => !moods.Contains(Mood);
}