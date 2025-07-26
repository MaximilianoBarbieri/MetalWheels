using System.Collections.Generic;
using System.Linq;

public class WorldState
{
    public float Life;
    
    public string Mood;
    
    public bool CarInRange;
    public bool Impacted;
    
    public int Steps;
    public int MaxSteps;
    
    public float Speed;
    public float SpeedRotation;
    
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

    public WorldState Clone()
    {
        return (WorldState)this.MemberwiseClone(); // copia superficial
    }

    public bool IsMoodOneOf(params string[] validMoods)
    {
        return validMoods.Contains(Mood);
    }

    public bool IsMoodNot(params string[] moods)
    {
        return !moods.Contains(Mood);
    }
}