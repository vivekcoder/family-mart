using System.Collections.Generic;
using UnityEngine;

// Polls every indicator a few times per second and shows only the most important ones
public class AttentionSystem : MonoBehaviour
{
    const int MaxVisible = 8;

    static readonly List<AttentionIndicator> _candidates = new List<AttentionIndicator>();
    static readonly System.Comparison<AttentionIndicator> _byPriority = Compare;
    static Vector3 _playerPosition;

    float _timer;

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = 0.25f;

        PlayerInventory player = PlayerInventory.Instance;
        _playerPosition = player != null ? player.transform.position : Vector3.zero;

        _candidates.Clear();
        foreach (AttentionIndicator indicator in AttentionIndicator.All)
        {
            indicator.Poll();
            if (indicator.Level != AttentionLevel.None) _candidates.Add(indicator);
            else indicator.SetWanted(false);
        }

        _candidates.Sort(_byPriority);
        for (int i = 0; i < _candidates.Count; i++)
            _candidates[i].SetWanted(i < MaxVisible);
    }

    // Higher priority first; ties go to whatever is closer to the player
    static int Compare(AttentionIndicator a, AttentionIndicator b)
    {
        int byLevel = ((int)b.Level).CompareTo((int)a.Level);
        if (byLevel != 0) return byLevel;

        float distanceA = (a.transform.position - _playerPosition).sqrMagnitude;
        float distanceB = (b.transform.position - _playerPosition).sqrMagnitude;
        return distanceA.CompareTo(distanceB);
    }
}