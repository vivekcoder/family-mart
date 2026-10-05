using System.Collections.Generic;
using UnityEngine;

public class CheckoutCounter : InteractionZone
{
    public Vector3 queueHead;        // where the first customer stands
    public float spacing = 1.2f;     // gap between queued customers
    public int maxQueue = GameConfig.CheckoutMaxQueue;
    public float packTime = GameConfig.CheckoutPackTime;
    public StaffWorker cashier;      // set when a cashier is hired for this counter

    readonly List<Customer> _queue = new List<Customer>();
    Customer _current;
    float _progress;
    float _refreshTimer;
    TextMesh _label;
    string _lastText;

    public int QueueLength => _queue.Count;
    public bool IsFull => _queue.Count >= maxQueue;
    public bool HasCashier => cashier != null;

    // Where a cashier stands (on the left side of the counter)
    public Vector3 StaffSpot => transform.position + new Vector3(-1.7f, 0f, 0f);

    public static CheckoutCounter Create(Vector3 position, Vector3 queueHead)
    {
        var root = new GameObject("Checkout Counter");
        root.transform.position = position;

        var body = GameBootstrap.MakePrimitive(PrimitiveType.Cube, "Counter Body", new Color(0.35f, 0.38f, 0.42f), true);
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        body.transform.localScale = new Vector3(1.6f, 1f, 1.6f);

        var screen = GameBootstrap.MakePrimitive(PrimitiveType.Cube, "Counter Screen", new Color(0.2f, 0.9f, 0.5f), false);
        screen.transform.SetParent(root.transform, false);
        screen.transform.localPosition = new Vector3(0f, 1.15f, 0f);
        screen.transform.localScale = new Vector3(0.7f, 0.3f, 0.5f);

        Props.DecorateCounter(root.transform);

        var counter = root.AddComponent<CheckoutCounter>();
        counter.queueHead = queueHead;
        counter.radius = 3f;
        counter.interval = 0.1f;
        counter.markerColor = new Color(1f, 0.95f, 0.5f);
        counter._label = WorldLabel.Create(root.transform, new Vector3(0f, 2.6f, 0f));
        counter.Refresh();
        AttentionIndicator.Attach(root.transform, new Vector3(0f, 3.6f, 0f),
            () => counter.cashier == null && counter._queue.Count > 0 && counter._queue[0].CanBeServed
                ? AttentionLevel.High : AttentionLevel.None);
        return counter;
    }

    void Awake()
    {
        Shop.Counters.Add(this);
    }

    void OnDestroy()
    {
        Shop.Counters.Remove(this);
    }

    // The queue extends away from the entrance, toward the back of the store
    Vector3 SlotPosition(int index)
    {
        return queueHead + Vector3.forward * (index * spacing);
    }

    public Vector3 JoinQueue(Customer customer)
    {
        _queue.Add(customer);
        Refresh();
        return SlotPosition(_queue.Count - 1);
    }

    protected override void Tick()
    {
        // A cashier at their post packs orders on their own
        if (cashier != null && cashier.IsAtPost)
            Serve(Time.deltaTime * cashier.def.workSpeed * StaffManager.SpeedMultiplier);

        _refreshTimer -= Time.deltaTime;
        if (_refreshTimer > 0f) return;
        _refreshTimer = 0.25f;
        Refresh();
    }

    // The player is standing at the counter
    protected override string Prompt => _queue.Count > 0 && _queue[0].CanBeServed ? "CHECKOUT" : null;

    protected override void Interact(PlayerInventory player)
    {
        Serve(interval);
    }

    // Spends 'amount' seconds of packing work on the first customer in the queue
    void Serve(float amount)
    {
        if (_queue.Count == 0) return;

        Customer head = _queue[0];
        if (!head.CanBeServed) return;

        if (head != _current)
        {
            _current = head;
            _progress = 0f;
            head.BeginCheckout();
        }

        _progress += amount;
        if (_progress >= packTime) Complete(head);
    }

    void Complete(Customer customer)
    {
        Wallet.Add(customer.Payment);
        FloatingText.Spawn(customer.transform.position + Vector3.up * 2.9f,
            "+$" + customer.Payment, new Color(1f, 0.9f, 0.3f));

        _queue.RemoveAt(0);
        _current = null;
        _progress = 0f;
        customer.PayAndLeave();

        for (int i = 0; i < _queue.Count; i++)
            _queue[i].SetQueueTarget(SlotPosition(i));

        Refresh();
    }

    void Refresh()
    {
        string text = "Checkout  " + _queue.Count + "/" + maxQueue;
        if (cashier != null) text += "\nCashier on duty";

        if (_queue.Count > 0 && _queue[0].CanBeServed)
        {
            Customer head = _queue[0];
            text += "\n$" + head.Payment + " (" + Mathf.RoundToInt(head.Satisfaction * 100f) + "%)";
            if (_current != null)
                text += "\nPacking " + Mathf.RoundToInt(Mathf.Clamp01(_progress / packTime) * 100f) + "%";
        }

        if (text == _lastText) return;
        _lastText = text;
        _label.text = text;
    }
}