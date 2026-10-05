using UnityEngine;

public class UpgradePad : InteractionZone
{
    public UpgradeDef def;

    TextMesh _label;
    Transform _sign;
    Color _baseColor;
    float _phase;
    float _hold;
    float _lastPresent = -10f;
    int _lastLevel = -1;
    bool _affordable;
    bool _stateSet;
    bool _maxed;

    public static UpgradePad Create(UpgradeDef def, Vector3 position)
    {
        var go = new GameObject("Upgrade Pad: " + def.title);
        go.transform.position = position;

        Color color = def.kind == UpgradeKind.StaffSpeed ? new Color(0.35f, 0.75f, 1f)
                    : def.kind == UpgradeKind.StaffCapacity ? new Color(0.55f, 0.9f, 0.4f)
                    : new Color(1f, 0.8f, 0.25f);

        // Floating sign with an icon above it
        var sign = new GameObject("Sign").transform;
        sign.SetParent(go.transform, false);
        sign.localPosition = new Vector3(0f, 1.5f, 0f);
        BuildIcon(sign, def.kind, color);

        var pad = go.AddComponent<UpgradePad>();
        pad.def = def;
        pad._sign = sign;
        pad._baseColor = color;
        pad.markerColor = Color.Lerp(color, Color.white, 0.35f);
        pad.radius = 1.6f;
        pad.interval = 0.1f;
        pad._phase = Random.value * 6f;

        pad._label = WorldLabel.Create(sign, new Vector3(0f, 0f, 0f));
        Look.Part(pad._label.transform, PrimitiveType.Cube, new Color(0.1f, 0.12f, 0.2f),
            new Vector3(0f, 0f, 0.08f), new Vector3(2.6f, 0.9f, 0.08f));

        AttentionIndicator.Attach(go.transform, new Vector3(0f, 3.4f, 0f),
            () => !pad._maxed && UpgradeSystem.CanAfford(def) ? AttentionLevel.Low : AttentionLevel.None, true);
        return pad;
    }

    static void BuildIcon(Transform sign, UpgradeKind kind, Color accent)
    {
        Vector3 at = new Vector3(0f, 0.95f, 0f);
        if (kind == UpgradeKind.StaffSpeed)
        {
            Transform rim = Look.Part(sign, PrimitiveType.Cylinder, accent, at, new Vector3(0.6f, 0.05f, 0.6f));
            rim.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Transform face = Look.Part(sign, PrimitiveType.Cylinder, Color.white, at + new Vector3(0f, 0f, -0.03f), new Vector3(0.5f, 0.05f, 0.5f));
            face.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Look.Part(sign, PrimitiveType.Cube, new Color(0.9f, 0.25f, 0.25f), at + new Vector3(0.05f, 0.08f, -0.08f), new Vector3(0.05f, 0.22f, 0.03f));
        }
        else if (kind == UpgradeKind.StaffCapacity)
        {
            Look.Part(sign, PrimitiveType.Cube, new Color(0.8f, 0.6f, 0.35f), at + new Vector3(-0.15f, -0.08f, 0f), new Vector3(0.4f, 0.28f, 0.3f));
            Look.Part(sign, PrimitiveType.Cube, new Color(0.85f, 0.65f, 0.4f), at + new Vector3(0.12f, 0.1f, 0f), new Vector3(0.35f, 0.25f, 0.28f));
        }
        else
        {
            Look.Part(sign, PrimitiveType.Cube, new Color(0.9f, 0.45f, 0.2f), at, new Vector3(0.42f, 0.5f, 0.24f));
            Look.Part(sign, PrimitiveType.Cube, new Color(0.7f, 0.32f, 0.14f), at + new Vector3(0f, 0.2f, 0f), new Vector3(0.44f, 0.16f, 0.26f));
            Look.Part(sign, PrimitiveType.Cube, accent, at + new Vector3(0f, -0.1f, -0.13f), new Vector3(0.28f, 0.2f, 0.04f));
        }
    }

    protected override string Prompt =>
        !_maxed && UpgradeSystem.CanAfford(def) ? "HOLD TO UPGRADE" : null;

    // Standing on the pad while you can afford the upgrade buys it after a short hold
    protected override void Interact(PlayerInventory player)
    {
        _lastPresent = Time.time;
        if (_maxed) return;

        if (!UpgradeSystem.CanAfford(def))
        {
            _hold = 0f;
            return;
        }

        _hold += interval;
        if (_hold < 0.6f) return;
        _hold = 0f;

        if (UpgradeSystem.TryUpgrade(def))
        {
            DustPuff.Burst(transform.position);
            FloatingText.Spawn(transform.position + Vector3.up * 2.6f,
                "Level " + UpgradeSystem.GetLevel(def), new Color(1f, 0.9f, 0.3f));
        }
    }

    protected override void Tick()
    {
        if (_maxed || MarkerRenderer == null) return;

        _phase += Time.deltaTime;
        if (Time.time - _lastPresent > 0.3f) _hold = 0f;

        int level = UpgradeSystem.GetLevel(def);
        bool affordable = UpgradeSystem.CanAfford(def);

        if (level != _lastLevel)
        {
            _lastLevel = level;
            if (UpgradeSystem.IsMax(def))
            {
                _maxed = true;
                _sign.gameObject.SetActive(false);
                MarkerRenderer.gameObject.SetActive(false);
                return;
            }
            _label.text = def.title + "  Lv " + level + "/" + def.MaxLevel + "\n$" + UpgradeSystem.NextCost(def);
        }

        if (!_stateSet || affordable != _affordable)
        {
            _affordable = affordable;
            _stateSet = true;
            MarkerRenderer.sharedMaterial = Look.Mat(affordable
                ? Color.Lerp(_baseColor, Color.white, 0.35f)
                : Color.Lerp(_baseColor, new Color(0.35f, 0.37f, 0.42f), 0.75f));
            _label.color = affordable ? Color.white : new Color(0.7f, 0.72f, 0.78f);
        }

        // Glow = a gentle pulse; the sign floats higher when it's affordable
        float pulse = affordable ? 1f + Mathf.Sin(_phase * 4f) * 0.05f : 1f;
        float size = radius * 2f * pulse;
        MarkerRenderer.transform.localScale = new Vector3(size, 0.01f, size);
        _sign.localPosition = new Vector3(0f, 1.5f + Mathf.Sin(_phase * 2.5f) * (affordable ? 0.12f : 0.05f), 0f);
    }
}