using System.Collections.Generic;
using UnityEngine;

public enum AttentionLevel { None = 0, Low = 1, Medium = 2, High = 3, Critical = 4 }

// A small floating badge above an object. The AttentionSystem decides which badges are shown.
public class AttentionIndicator : MonoBehaviour
{
    public static readonly List<AttentionIndicator> All = new List<AttentionIndicator>();

    System.Func<AttentionLevel> _source;
    SpriteRenderer _renderer;
    Vector3 _basePosition;
    AttentionLevel _displayLevel = AttentionLevel.Medium;
    bool _upgradeIcon;
    bool _wanted;
    float _show;
    float _phase;

    public AttentionLevel Level { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        All.Clear();
    }

    public static AttentionIndicator Attach(Transform owner, Vector3 localOffset,
        System.Func<AttentionLevel> source, bool upgradeIcon = false)
    {
        var go = new GameObject("Attention Indicator");
        go.transform.SetParent(owner, false);
        go.transform.localPosition = localOffset;
        go.transform.localScale = Vector3.zero;

        var indicator = go.AddComponent<AttentionIndicator>();
        indicator._source = source;
        indicator._basePosition = localOffset;
        indicator._upgradeIcon = upgradeIcon;
        indicator._phase = Random.value * 6f;
        indicator._renderer = go.AddComponent<SpriteRenderer>();
        indicator._renderer.sprite = upgradeIcon ? Icons.UpArrow : Icons.Exclamation;
        indicator._renderer.sortingOrder = 200;
        indicator._renderer.enabled = false;
        return indicator;
    }

    void OnEnable()
    {
        All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }

    // Called a few times per second by the AttentionSystem
    public void Poll()
    {
        Level = _source != null ? _source() : AttentionLevel.None;
        if (Level != AttentionLevel.None) _displayLevel = Level;
    }

    public void SetWanted(bool wanted)
    {
        _wanted = wanted;
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        _show = Mathf.MoveTowards(_show, _wanted ? 1f : 0f, dt * 6f);

        if (_show <= 0.001f)
        {
            if (_renderer.enabled) _renderer.enabled = false;
            return;
        }
        if (!_renderer.enabled) _renderer.enabled = true;

        _phase += dt;
        float pulse = _displayLevel == AttentionLevel.Critical ? 1f + Mathf.Sin(_phase * 9f) * 0.12f : 1f;
        transform.localScale = Vector3.one * (SizeFor(_displayLevel) * pulse * _show);
        transform.localPosition = _basePosition + Vector3.up * (Mathf.Sin(_phase * 3f) * 0.08f);

        if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;

        _renderer.color = _upgradeIcon ? new Color(0.45f, 0.9f, 0.5f) : ColorFor(_displayLevel);
    }

    static float SizeFor(AttentionLevel level)
    {
        switch (level)
        {
            case AttentionLevel.Critical: return 0.85f;
            case AttentionLevel.High: return 0.7f;
            case AttentionLevel.Medium: return 0.6f;
            default: return 0.5f;
        }
    }

    static Color ColorFor(AttentionLevel level)
    {
        switch (level)
        {
            case AttentionLevel.Critical: return new Color(0.95f, 0.25f, 0.25f);
            case AttentionLevel.High: return new Color(1f, 0.55f, 0.15f);
            case AttentionLevel.Medium: return new Color(1f, 0.85f, 0.25f);
            default: return new Color(0.5f, 0.78f, 1f);
        }
    }
}