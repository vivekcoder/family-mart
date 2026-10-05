using System.Collections.Generic;
using UnityEngine;

public class CropPlant : InteractionZone, IItemSource
{
    public CropDef crop;
    public Transform produceRoot;

    readonly List<StackItem> _ready = new List<StackItem>();
    float _timer;
    TextMesh _label;

    public int ReadyCount => _ready.Count;
    public float Timer => _timer;

    public static CropPlant Create(CropDef crop, Vector3 position)
    {
        var root = new GameObject(crop.product.name + " Plant");
        root.transform.position = position;

        var soil = GameBootstrap.MakePrimitive(PrimitiveType.Cylinder, "Soil", new Color(0.35f, 0.22f, 0.12f), false);
        soil.transform.SetParent(root.transform, false);
        soil.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        soil.transform.localScale = new Vector3(2.6f, 0.05f, 2.6f);

        // The bush keeps its collider so customers walk around it
        var bush = GameBootstrap.MakePrimitive(PrimitiveType.Sphere, "Bush", new Color(0.2f, 0.55f, 0.2f), true);
        bush.transform.SetParent(root.transform, false);
        bush.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        bush.transform.localScale = new Vector3(1.3f, 1.1f, 1.3f);

        var produceRoot = new GameObject("Produce Root").transform;
        produceRoot.SetParent(root.transform, false);

        var plant = root.AddComponent<CropPlant>();
        plant.crop = crop;
        Props.DecoratePlant(root.transform, crop);
        AttentionIndicator.Attach(root.transform, new Vector3(0f, 3.6f, 0f),
            () => plant._ready.Count > 0 ? AttentionLevel.High : AttentionLevel.None);
        plant.produceRoot = produceRoot;
        plant.radius = 3f;
        plant.markerColor = new Color(0.6f, 0.9f, 0.5f);
        plant._label = WorldLabel.Create(root.transform, new Vector3(0f, 2.8f, 0f));
        plant.Refresh();
        return plant;
    }

    // ---- IItemSource: lets staff collect ripe produce ----

    public Vector3 WorkerSpot => transform.position + new Vector3(0f, 0f, -1.9f);

    public int AvailableCount(Product product)
    {
        return product == crop.product ? _ready.Count : 0;
    }

    public StackItem TakeItem(Product product)
    {
        if (product != crop.product || _ready.Count == 0) return null;

        StackItem item = _ready[_ready.Count - 1];
        _ready.RemoveAt(_ready.Count - 1);
        Refresh();
        SaveManager.RequestSave();
        return item;
    }

    void Awake()
    {
        Shop.Crops.Add(this);
    }

    void OnDestroy()
    {
        Shop.Crops.Remove(this);
    }

    // For loading: recreate the ripe produce and the growth progress
    public void Restore(int ready, float timer)
    {
        ready = Mathf.Clamp(ready, 0, crop.maxStored);
        for (int i = 0; i < ready; i++) Grow();

        _timer = ready >= crop.maxStored ? 0f : Mathf.Max(0f, timer);
        Refresh();
    }

    // Lets the plant grow for time that passed while the game wasn't running
    public void FastForward(double seconds)
    {
        if (!GameConfig.OfflineProduction || seconds <= 0.0 || crop.productionTime <= 0f) return;
        if (_ready.Count >= crop.maxStored) return;

        double total = _timer + seconds;
        int cycles = (int)System.Math.Min(total / crop.productionTime, 100000.0);
        int amount = Mathf.Min(cycles * crop.amountPerCycle, crop.maxStored - _ready.Count);
        for (int i = 0; i < amount; i++) Grow();

        _timer = _ready.Count >= crop.maxStored ? 0f : (float)(total % crop.productionTime);
        Refresh();
    }

    protected override string Prompt => _ready.Count > 0 ? "HARVEST" : null;

    // Growth runs every frame (called by InteractionZone)
    protected override void Tick()
    {
        // Full: production is paused and the timer restarts after the next harvest
        if (_ready.Count >= crop.maxStored)
        {
            _timer = 0f;
            return;
        }

        _timer += Time.deltaTime;
        if (_timer < crop.productionTime) return;
        _timer -= crop.productionTime;

        int amount = Mathf.Min(crop.amountPerCycle, crop.maxStored - _ready.Count);
        for (int i = 0; i < amount; i++) Grow();
        Refresh();
    }

    void Grow()
    {
        StackItem item = StackItem.Create(crop.product, produceRoot.position + Vector3.up * 0.9f);
        item.MoveTo(produceRoot, Slot(_ready.Count));
        _ready.Add(item);
    }

    // Harvesting: the player stands in the zone and ripe produce flies to their stack
    protected override void Interact(PlayerInventory player)
    {
        if (_ready.Count == 0 || player.IsFull) return;

        StackItem item = _ready[_ready.Count - 1];
        _ready.RemoveAt(_ready.Count - 1);
        player.Add(item);
        Refresh();
        SaveManager.RequestSave();
    }

    Vector3 Slot(int index)
    {
        float angle = index * Mathf.PI * 2f / Mathf.Max(1, crop.maxStored);
        return new Vector3(Mathf.Cos(angle) * 0.85f, 1.15f, Mathf.Sin(angle) * 0.85f);
    }

    void Refresh()
    {
        _label.text = crop.product.name + " Plant\n" + _ready.Count + " / " + crop.maxStored;
    }
}