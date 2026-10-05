using UnityEngine;

public class UnlockPad : InteractionZone
{
    public string Id { get; set; }
    public int Paid => _paid;

    string _title;
    int _cost;
    int _paid;
    bool _done;
    System.Action _onUnlocked;
    Color _baseColor;
    TextMesh _label;
    Transform _sign;
    float _phase;
    bool _affordable;
    bool _stateSet;

    public static UnlockPad Create(string title, Vector3 position, int cost, Color color, System.Action onUnlocked)
    {
        var go = new GameObject("Pad: " + title);
        go.transform.position = position;

        var sign = new GameObject("Sign").transform;
        sign.SetParent(go.transform, false);
        sign.localPosition = new Vector3(0f, 1.5f, 0f);

        var pad = go.AddComponent<UnlockPad>();
        pad._title = title;
        pad._cost = cost;
        pad._onUnlocked = onUnlocked;
        pad._baseColor = color;
        pad._sign = sign;
        pad.markerColor = Color.Lerp(color, Color.white, 0.35f);
        pad.radius = 1.6f;
        pad.interval = 0.05f;
        pad._phase = Random.value * 6f;

        // The sign: text on a dark plate that always faces the camera
        pad._label = WorldLabel.Create(sign, Vector3.zero);
        Look.Part(pad._label.transform, PrimitiveType.Cube, new Color(0.1f, 0.12f, 0.2f),
            new Vector3(0f, 0f, 0.08f), new Vector3(2.6f, 0.9f, 0.08f));

        pad.Refresh();
        AttentionIndicator.Attach(go.transform, new Vector3(0f, 3.2f, 0f),
            () => !pad._done && Wallet.Money >= pad._cost - pad._paid ? AttentionLevel.Low : AttentionLevel.None,
            true);
        return pad;
    }

    // For loading: money already paid into this pad
    public void Restore(int paid)
    {
        _paid = Mathf.Clamp(paid, 0, _cost);
        Refresh();
    }

    protected override string Prompt => _done ? null : "BUY";

    protected override void Interact(PlayerInventory player)
    {
        if (_done) return;

        int step = Mathf.Max(1, _cost / 20);
        int amount = Mathf.Min(_cost - _paid, step, Wallet.Money);
        if (amount <= 0) return;

        Wallet.TrySpend(amount);
        _paid += amount;
        Refresh();

        if (_paid >= _cost)
        {
            _done = true;
            DustPuff.Burst(transform.position);
            _onUnlocked?.Invoke();
            Destroy(gameObject);
        }
    }

    protected override void Tick()
    {
        if (_done || MarkerRenderer == null) return;

        _phase += Time.deltaTime;
        bool affordable = Wallet.Money >= _cost - _paid;

        if (!_stateSet || affordable != _affordable)
        {
            _affordable = affordable;
            _stateSet = true;
            MarkerRenderer.sharedMaterial = Look.Mat(affordable
                ? Color.Lerp(_baseColor, Color.white, 0.35f)
                : Color.Lerp(_baseColor, new Color(0.35f, 0.37f, 0.42f), 0.75f));
            _label.color = affordable ? Color.white : new Color(0.7f, 0.72f, 0.78f);
        }

        float pulse = affordable ? 1f + Mathf.Sin(_phase * 4f) * 0.05f : 1f;
        float size = radius * 2f * pulse;
        MarkerRenderer.transform.localScale = new Vector3(size, 0.01f, size);
        _sign.localPosition = new Vector3(0f, 1.5f + Mathf.Sin(_phase * 2.5f) * (affordable ? 0.12f : 0.05f), 0f);
    }

    void Refresh()
    {
        _label.text = _title + "\n$" + (_cost - _paid);
    }
}