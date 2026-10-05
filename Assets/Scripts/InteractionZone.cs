using UnityEngine;

public abstract class InteractionZone : MonoBehaviour
{
    public float radius = 3f;
    public float interval = 0.12f;
    public Color markerColor = new Color(1f, 1f, 1f);

    float _timer;

    protected abstract void Interact(PlayerInventory player);

    // Text shown on screen while the player stands here and can do something (null = nothing to show)
    protected virtual string Prompt => null;

    // The flat disc on the floor (pads change its color to show if they're affordable)
    protected Renderer MarkerRenderer { get; private set; }

    // Optional: runs every frame, whether or not the player is nearby
    protected virtual void Tick() { }

    void Start()
    {
        var marker = GameBootstrap.MakePrimitive(PrimitiveType.Cylinder, "Zone Marker", markerColor, false);
        MarkerRenderer = marker.GetComponent<Renderer>();
        marker.transform.SetParent(transform, false);
        marker.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        marker.transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
    }

    void Update()
    {
        Tick();

        PlayerInventory player = PlayerInventory.Instance;
        if (player == null) return;

        Vector3 offset = player.transform.position - transform.position;
        offset.y = 0f;

        if (offset.sqrMagnitude > radius * radius)
        {
            _timer = 0f;
            return;
        }

        string prompt = Prompt;
        if (prompt != null) InteractionHint.Request(prompt, offset.sqrMagnitude);

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = interval;
            Interact(player);
        }
    }
}