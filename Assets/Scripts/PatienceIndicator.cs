using UnityEngine;

// A floating face and bar showing how patient a customer still is
public class PatienceIndicator : MonoBehaviour
{
    static readonly Color Green = new Color(0.35f, 0.85f, 0.4f);
    static readonly Color Yellow = new Color(1f, 0.85f, 0.3f);
    static readonly Color Red = new Color(0.95f, 0.3f, 0.28f);

    SpriteRenderer _face;
    SpriteRenderer _barBack;
    SpriteRenderer _barFill;
    Vector3 _basePosition;
    float _targetRatio = 1f;
    float _shownRatio = 1f;
    float _visible;
    bool _show;
    bool _renderersOn;
    int _faceLevel = -1;
    float _pop;
    float _phase;

    public static PatienceIndicator Create(Transform owner, Vector3 localOffset)
    {
        var root = new GameObject("Patience Indicator");
        root.transform.SetParent(owner, false);
        root.transform.localPosition = localOffset;

        var indicator = root.AddComponent<PatienceIndicator>();
        indicator._basePosition = localOffset;
        indicator._phase = Random.value * 6f;
        indicator._face = AddSprite(root.transform, "Face", Icons.Face(0),
            new Vector3(0f, 0.3f, 0f), new Vector3(0.62f, 0.62f, 1f), 202, Color.white);
        indicator._barBack = AddSprite(root.transform, "Bar Back", Icons.WhiteCenter,
            new Vector3(0f, -0.08f, 0f), new Vector3(0.9f, 0.14f, 1f), 200, new Color(0.12f, 0.13f, 0.18f, 0.85f));
        indicator._barFill = AddSprite(root.transform, "Bar Fill", Icons.WhiteLeft,
            new Vector3(-0.42f, -0.08f, 0f), new Vector3(0.84f, 0.09f, 1f), 201, Green);
        indicator.SetRenderers(false);
        return indicator;
    }

    static SpriteRenderer AddSprite(Transform parent, string name, Sprite sprite,
        Vector3 position, Vector3 scale, int sortingOrder, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        renderer.color = color;
        return renderer;
    }

    // ratio: 1 = fully patient, 0 = out of patience
    public void Set(float ratio, bool show)
    {
        _targetRatio = Mathf.Clamp01(ratio);
        _show = show;
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        _visible = Mathf.MoveTowards(_visible, _show ? 1f : 0f, dt * 5f);

        bool on = _visible > 0.01f;
        SetRenderers(on);
        if (!on) return;

        // The bar and face follow the real value smoothly
        _shownRatio = Mathf.MoveTowards(_shownRatio, _targetRatio, dt * 0.6f);

        int level = LevelFor(_shownRatio);
        if (level != _faceLevel)
        {
            _faceLevel = level;
            _face.sprite = Icons.Face(level);
            _pop = 1f;
        }
        _pop = Mathf.MoveTowards(_pop, 0f, dt * 4f);

        _phase += dt;
        float urgency = 1f - _shownRatio;
        float bob = Mathf.Sin(_phase * (2f + urgency * 4f)) * (0.02f + urgency * 0.05f);
        float shake = urgency > 0.75f ? Mathf.Sin(_phase * 40f) * (urgency - 0.75f) * 24f : 0f;
        float pulse = 1f + Mathf.Sin(_phase * (3f + urgency * 5f)) * 0.04f * urgency + _pop * 0.2f;

        transform.localPosition = _basePosition + Vector3.up * bob;
        transform.localScale = Vector3.one * (_visible * pulse);
        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(0f, 0f, shake);

        float width = Mathf.Max(0.001f, 0.84f * _shownRatio);
        _barFill.transform.localScale = new Vector3(width, 0.09f, 1f);
        _barFill.color = BarColor(_shownRatio);
    }

    static int LevelFor(float ratio)
    {
        if (ratio > 0.66f) return 0;
        if (ratio > 0.33f) return 1;
        if (ratio > 0.12f) return 2;
        return 3;
    }

    static Color BarColor(float ratio)
    {
        return ratio > 0.5f
            ? Color.Lerp(Yellow, Green, (ratio - 0.5f) * 2f)
            : Color.Lerp(Red, Yellow, ratio * 2f);
    }

    void SetRenderers(bool on)
    {
        if (on == _renderersOn) return;
        _renderersOn = on;
        _face.enabled = on;
        _barBack.enabled = on;
        _barFill.enabled = on;
    }
}