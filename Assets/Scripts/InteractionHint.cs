using UnityEngine;

public static class InteractionHint
{
    static string _text;
    static float _bestDistance = float.MaxValue;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        _text = null;
        _bestDistance = float.MaxValue;
    }

    // Zones call this every frame while the player is inside; the closest one wins
    public static void Request(string text, float distanceSquared)
    {
        if (distanceSquared >= _bestDistance) return;
        _bestDistance = distanceSquared;
        _text = text;
    }

    // Returns this frame's hint (or null) and clears it for the next frame
    public static string Consume()
    {
        string text = _text;
        _text = null;
        _bestDistance = float.MaxValue;
        return text;
    }
}