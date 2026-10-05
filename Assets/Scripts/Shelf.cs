using System.Collections.Generic;
using UnityEngine;

public class Shelf : InteractionZone, IItemDestination
{
    public Product product;
    public Transform slotRoot;
    public int columns = GameConfig.ShelfColumns;
    public int rows = GameConfig.ShelfRows;

    readonly List<StackItem> _items = new List<StackItem>();
    TextMesh _label;
    Product[] _accepted;
    Vector2 _spacing = new Vector2(0.9f, 0.55f);        // distance between displayed items
    float _spread = 1.2f;                               // how far customers spread along the front
    Vector3 _workerOffset = new Vector3(1.9f, 0f, -1.9f);

    public int Capacity => columns * rows;
    public int Count => _items.Count;
    public int FreeSlots => Capacity - _items.Count;
    public bool IsFull => _items.Count >= Capacity;

    public static Shelf Create(Product product, Vector3 position)
    {
        bool fridge = product.shelfLook == ShelfLook.Fridge;

        var root = new GameObject(product.name + (fridge ? " Fridge" : " Shelf"));
        root.transform.position = position;
        Transform t = root.transform;

        // One collider for the whole unit (blocks the player and customers' paths)
        var collider = root.AddComponent<BoxCollider>();
        if (fridge)
        {
            collider.center = new Vector3(0f, 1f, 0f);
            collider.size = new Vector3(2.4f, 2f, 1.2f);
            BuildFridgeModel(t, product);
        }
        else
        {
            collider.center = new Vector3(0f, 0.9f, 0f);
            collider.size = new Vector3(4f, 1.8f, 1.4f);
            BuildWoodenModel(t, product);
        }

        // Items on display sit on the upper shelf
        var slotRoot = new GameObject("Slot Root").transform;
        slotRoot.SetParent(t, false);
        slotRoot.localPosition = new Vector3(0f, 1.2f, 0f);

        var shelf = root.AddComponent<Shelf>();
        shelf.product = product;
        shelf._accepted = new[] { product };
        shelf.slotRoot = slotRoot;
        shelf.radius = fridge ? 3f : 3.5f;
        shelf.markerColor = Color.Lerp(product.color, Color.white, 0.6f);

        if (fridge)
        {
            shelf._spacing = new Vector2(0.6f, 0.4f);
            shelf._spread = 0.7f;
            shelf._workerOffset = new Vector3(-1.6f, 0f, -1.9f);
        }

        shelf._label = WorldLabel.Create(t, new Vector3(0f, fridge ? 2.9f : 2.7f, 0f));
        shelf.Refresh();
        AttentionIndicator.Attach(t, new Vector3(0f, fridge ? 3.9f : 3.7f, 0f), () => shelf.NeedsAttention());
        return shelf;
    }

    static void BuildWoodenModel(Transform t, Product product)
    {
        Color frame = new Color(0.45f, 0.3f, 0.2f);
        Color plank = new Color(0.78f, 0.58f, 0.36f);
        Color back = new Color(0.62f, 0.45f, 0.3f);

        Look.Part(t, PrimitiveType.Cube, frame, new Vector3(0f, 0.075f, 0f), new Vector3(4f, 0.15f, 1.4f));
        Look.Part(t, PrimitiveType.Cube, frame, new Vector3(-1.94f, 0.9f, 0.05f), new Vector3(0.12f, 1.8f, 1.2f));
        Look.Part(t, PrimitiveType.Cube, frame, new Vector3(1.94f, 0.9f, 0.05f), new Vector3(0.12f, 1.8f, 1.2f));
        Look.Part(t, PrimitiveType.Cube, back, new Vector3(0f, 0.95f, 0.62f), new Vector3(3.9f, 1.7f, 0.1f));
        Look.Part(t, PrimitiveType.Cube, plank, new Vector3(0f, 0.6f, 0f), new Vector3(3.8f, 0.1f, 1.2f));
        Look.Part(t, PrimitiveType.Cube, plank, new Vector3(0f, 1.15f, 0f), new Vector3(3.8f, 0.1f, 1.2f));
        Look.Part(t, PrimitiveType.Cube, product.color, new Vector3(0f, 1.95f, 0.62f), new Vector3(2.6f, 0.5f, 0.1f));

        // Spare stock on the lower plank
        Color crate = Color.Lerp(product.color, Color.black, 0.15f);
        for (int i = -1; i <= 1; i++)
            Look.Part(t, PrimitiveType.Cube, crate, new Vector3(i * 1.2f, 0.85f, 0.1f), new Vector3(0.8f, 0.4f, 0.6f));
    }

    // An open-front chiller: you can see the cartons inside
    static void BuildFridgeModel(Transform t, Product product)
    {
        Color steel = new Color(0.85f, 0.88f, 0.92f);
        Color darkSteel = new Color(0.5f, 0.55f, 0.6f);
        Color teal = new Color(0.25f, 0.6f, 0.7f);
        Color blue = new Color(0.3f, 0.55f, 0.95f);

        Look.Part(t, PrimitiveType.Cube, darkSteel, new Vector3(0f, 0.1f, 0f), new Vector3(2.4f, 0.2f, 1.2f));
        Look.Part(t, PrimitiveType.Cube, steel, new Vector3(-1.15f, 1f, 0f), new Vector3(0.1f, 1.8f, 1.1f));
        Look.Part(t, PrimitiveType.Cube, steel, new Vector3(1.15f, 1f, 0f), new Vector3(0.1f, 1.8f, 1.1f));
        Look.Part(t, PrimitiveType.Cube, new Color(0.7f, 0.85f, 0.95f), new Vector3(0f, 1f, 0.5f), new Vector3(2.2f, 1.7f, 0.1f));
        Look.Part(t, PrimitiveType.Cube, steel, new Vector3(0f, 0.55f, 0f), new Vector3(2.2f, 0.06f, 1f));
        Look.Part(t, PrimitiveType.Cube, steel, new Vector3(0f, 1.15f, 0f), new Vector3(2.2f, 0.06f, 1f));
        Look.Part(t, PrimitiveType.Cube, teal, new Vector3(0f, 2f, 0f), new Vector3(2.4f, 0.3f, 1.2f));
        Look.Part(t, PrimitiveType.Cube, new Color(1f, 0.98f, 0.85f), new Vector3(0f, 1.82f, -0.05f), new Vector3(2f, 0.05f, 0.8f));
        Look.Part(t, PrimitiveType.Cube, blue, new Vector3(0f, 2.35f, 0.5f), new Vector3(1.6f, 0.3f, 0.08f));

        // Spare cartons on the lower shelf
        for (int i = -1; i <= 1; i++)
        {
            Look.Part(t, PrimitiveType.Cube, product.color, new Vector3(i * 0.7f, 0.75f, 0.1f), new Vector3(0.3f, 0.35f, 0.25f));
            Look.Part(t, PrimitiveType.Cube, blue, new Vector3(i * 0.7f, 0.95f, 0.1f), new Vector3(0.3f, 0.06f, 0.25f));
        }
    }

    // ---- IItemDestination: lets staff stock this shelf ----

    public Vector3 WorkerSpot => transform.position + _workerOffset;

    public Product[] AcceptedProducts => _accepted;

    public int FreeSpace(Product p) => p == product ? FreeSlots : 0;

    public float FillRatio(Product p) => (float)Count / Capacity;

    public bool Accept(StackItem item) => TryAdd(item);

    // A floating "!" when the shelf is running low and something is able to restock it
    public AttentionLevel NeedsAttention()
    {
        if (!Shop.IsProduced(product)) return AttentionLevel.None;
        return _items.Count <= Capacity / 3 ? AttentionLevel.Medium : AttentionLevel.None;
    }

    void Awake()
    {
        Shop.Shelves.Add(this);
    }

    void OnDestroy()
    {
        Shop.Shelves.Remove(this);
    }

    // For loading: recreate this many items on the shelf
    public void Restore(int count)
    {
        count = Mathf.Clamp(count, 0, Capacity);
        for (int i = 0; i < count; i++)
            TryAdd(StackItem.Create(product, slotRoot.position));
    }

    // Stocking: the player stands in the zone and matching items fly onto the shelf
    protected override string Prompt
    {
        get
        {
            PlayerInventory player = PlayerInventory.Instance;
            return player != null && !IsFull && player.Has(product) ? "STOCK" : null;
        }
    }

    protected override void Interact(PlayerInventory player)
    {
        if (IsFull) return;

        StackItem item = player.RemoveLast(product);
        if (item == null) return;

        TryAdd(item);
    }

    // Puts an item on the shelf if there's room and it's the right product
    public bool TryAdd(StackItem item)
    {
        if (IsFull || item.product != product) return false;

        _items.Add(item);
        item.MoveTo(slotRoot, SlotPosition(_items.Count - 1));
        Refresh();
        SaveManager.RequestSave();
        return true;
    }

    // Used by customers: removes and returns the last item on the shelf
    public StackItem TakeOne()
    {
        if (_items.Count == 0) return null;

        StackItem item = _items[_items.Count - 1];
        _items.RemoveAt(_items.Count - 1);
        Refresh();
        SaveManager.RequestSave();
        return item;
    }

    // A spot in front of the shelf (toward the entrance) where a customer stands
    public Vector3 GetCustomerSpot()
    {
        return transform.position + new Vector3(Random.Range(-_spread, _spread), 0f, -1.9f);
    }

    Vector3 SlotPosition(int index)
    {
        int col = index % columns;
        int row = index / columns;
        float x = (col - (columns - 1) / 2f) * _spacing.x;
        float z = (row - (rows - 1) / 2f) * _spacing.y;
        return new Vector3(x, 0.2f, z);
    }

    void Refresh()
    {
        string kind = product.shelfLook == ShelfLook.Fridge ? " Fridge" : " Shelf";
        _label.text = product.name + kind + "\n" + _items.Count + " / " + Capacity;
    }
}