using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    public enum State
    {
        Entering, Shopping, MovingToShelf, TakingProduct, Waiting, NextProduct,
        UnableToFindProduct, Angry, CheckoutQueue, Checkout, Leaving
    }

    NavMeshAgent _agent;
    Transform _cartSlots;
    PatienceIndicator _indicator;
    TextMesh _label;
    ShoppingList _list;
    ShoppingLine _line;
    Shelf _shelf;
    CheckoutCounter _counter;
    Vector3 _dest;
    State _state = State.Entering;
    float _patience = GameConfig.CustomerPatience;
    float _timer;
    float _stuckTime;
    int _retries;
    bool _angry;       // out of patience right now
    bool _gotAngry;    // ran out of patience at some point (affects what they pay)
    bool _inQueue;
    readonly List<StackItem> _carried = new List<StackItem>();

    public State CurrentState => _state;

    // What the customer pays: the value of what they collected (reduced if they lost patience)
    public int Payment
    {
        get
        {
            float multiplier = _gotAngry ? GameConfig.AngryRevenueMultiplier : 1f;
            return Mathf.RoundToInt(_list.CollectedValue * multiplier);
        }
    }

    // 0..1: how much of the shopping list they managed to get
    public float Satisfaction
    {
        get
        {
            int requested = _list.TotalRequested;
            return requested == 0 ? 1f : (float)_list.TotalCollected / requested;
        }
    }

    public bool CanBeServed =>
        _inQueue && (_state == State.CheckoutQueue || _state == State.Checkout) && Arrived;

    bool Arrived
    {
        get
        {
            if (_agent.pathPending) return false;
            Vector3 d = _dest - transform.position;
            d.y = 0f;
            return d.sqrMagnitude < 0.3f;
        }
    }

    public static Customer Create(ShoppingList list)
    {
        var root = new GameObject("Customer");
        root.transform.position = GameConfig.CustomerEntrance;

        CharacterBuilder.Build(root.transform, CharacterStyle.RandomCustomer());
        Transform cartSlots = Props.BuildCart(root.transform);

        var agent = root.AddComponent<NavMeshAgent>();
        agent.radius = 0.4f;
        agent.height = 1.8f;
        agent.speed = GameConfig.CustomerSpeed;
        agent.acceleration = 14f;
        agent.stoppingDistance = 0.15f;
        agent.avoidancePriority = Random.Range(30, 70);

        var customer = root.AddComponent<Customer>();
        customer._agent = agent;
        customer._cartSlots = cartSlots;
        customer._list = list;
        customer._indicator = PatienceIndicator.Create(root.transform, new Vector3(0f, 2.55f, 0f));
        customer._label = WorldLabel.Create(root.transform, new Vector3(0f, 3.4f, 0f));
        customer.UpdateLabel();

        customer.MoveTo(GameConfig.CustomerDoor);
        return customer;
    }

    // ---- Called by the checkout counter ----

    public void SetQueueTarget(Vector3 position)
    {
        MoveTo(position);
    }

    public void BeginCheckout()
    {
        _state = State.Checkout;   // being served: patience stops draining
        _indicator.Set(1f, false);
    }

    public void PayAndLeave()
    {
        foreach (StackItem item in _carried)
            if (item != null) Destroy(item.gameObject);
        _carried.Clear();
        Leave();
    }

    // ---- State machine ----

    void Update()
    {
        float dt = Time.deltaTime;

        switch (_state)
        {
            case State.Entering:
                CheckStuck();
                if (Arrived) _state = State.Shopping;
                break;

            case State.Shopping:
                BeginNextLine();
                break;

            case State.MovingToShelf:
                CheckStuck();
                if (Arrived)
                {
                    _state = State.TakingProduct;
                    _timer = 0f;
                    _patience = GameConfig.CustomerPatience;   // a fresh allowance at each shelf
                    _indicator.Set(1f, false);
                }
                break;

            case State.TakingProduct:
                TakeFromShelf(dt);
                break;

            case State.Waiting:
                if (_shelf == null)
                {
                    _state = State.UnableToFindProduct;
                    break;
                }
                if (_shelf.Count > 0)
                {
                    _state = State.TakingProduct;   // stock arrived
                    _timer = 0f;
                    break;
                }
                DrainPatience(dt);
                break;

            case State.NextProduct:
                _state = State.Shopping;
                break;

            case State.UnableToFindProduct:
                _line.abandoned = true;
                UpdateLabel();
                _state = State.Shopping;
                break;

            case State.Angry:
                StartCheckout();   // stop shopping, keep what we have
                break;

            case State.CheckoutQueue:
                UpdateCheckoutQueue(dt);
                break;

            case State.Checkout:
                break;   // the player or cashier is packing our order

            case State.Leaving:
                CheckStuck();
                if (Arrived) Destroy(gameObject);
                break;
        }
    }

    void BeginNextLine()
    {
        _line = _list.NextIncomplete();
        if (_line == null)
        {
            StartCheckout();
            return;
        }

        _shelf = Shop.FindShelfFor(_line.product);
        if (_shelf == null)
        {
            _state = State.UnableToFindProduct;
            return;
        }

        MoveTo(_shelf.GetCustomerSpot());
        _state = State.MovingToShelf;
    }

    void TakeFromShelf(float dt)
    {
        _timer -= dt;
        if (_timer > 0f) return;
        _timer = GameConfig.CustomerTakeInterval;

        if (_shelf == null)
        {
            _state = State.UnableToFindProduct;
            return;
        }

        StackItem item = _shelf.TakeOne();
        if (item == null)
        {
            _state = State.Waiting;   // shelf is empty: wait for restocking
            return;
        }

        // The product hops into the cart
        item.MoveTo(_cartSlots, CartSlot(_carried.Count));
        _carried.Add(item);
        _line.collected++;
        _patience = GameConfig.CustomerPatience;
        _indicator.Set(1f, false);
        UpdateLabel();

        if (_line.IsDone) _state = State.NextProduct;
    }

    void StartCheckout()
    {
        if (_carried.Count == 0)
        {
            Leave();   // nothing to pay for
            return;
        }

        _inQueue = false;
        _angry = false;
        _patience = GameConfig.CustomerPatience;   // a separate allowance for the checkout line
        _indicator.Set(1f, false);
        _timer = 0f;
        _state = State.CheckoutQueue;
    }

    void UpdateCheckoutQueue(float dt)
    {
        if (!_inQueue)
        {
            // Try to join a queue. If every queue is full, wait (and lose patience).
            _timer -= dt;
            if (_timer <= 0f)
            {
                _timer = 0.4f;
                _counter = Shop.FindBestCounter();
                if (_counter != null)
                {
                    _inQueue = true;
                    MoveTo(_counter.JoinQueue(this));
                }
            }
            DrainPatience(dt);
            return;
        }

        CheckStuck();
        DrainPatience(dt);
    }

    void Leave()
    {
        _inQueue = false;
        _indicator.Set(1f, false);
        _state = State.Leaving;
        MoveTo(GameConfig.CustomerEntrance);
    }

    // ---- Patience ----

    void DrainPatience(float dt)
    {
        if (_angry) return;

        _patience -= dt;
        _indicator.Set(_patience / GameConfig.CustomerPatience, true);
        if (_patience <= 0f) BecomeAngry();
    }

    void BecomeAngry()
    {
        _angry = true;
        _gotAngry = true;
        _indicator.Set(0f, true);

        // At a shelf: give up and head to checkout with whatever we have
        if (_state == State.Waiting) _state = State.Angry;
    }

    // ---- Cart ----

    static Vector3 CartSlot(int index)
    {
        int column = index % 3;
        int row = (index / 3) % 2;
        int layer = index / 6;
        return new Vector3((column - 1) * 0.25f, 0.14f + layer * 0.22f, (row - 0.5f) * 0.25f);
    }

    // ---- Navigation helpers ----

    void MoveTo(Vector3 destination)
    {
        _dest = destination;
        _stuckTime = 0f;
        _retries = 0;
        if (_agent.isOnNavMesh) _agent.SetDestination(destination);
    }

    // If we stop making progress: recalculate the path, and as a last resort relocate
    void CheckStuck()
    {
        if (Arrived)
        {
            _stuckTime = 0f;
            return;
        }
        if (_agent.pathPending) return;

        bool notMoving = _agent.velocity.sqrMagnitude < 0.04f
                         || _agent.pathStatus == NavMeshPathStatus.PathInvalid;
        _stuckTime = notMoving ? _stuckTime + Time.deltaTime : 0f;
        if (_stuckTime < 2f) return;

        _stuckTime = 0f;
        _retries++;

        if (_retries < 3)
        {
            _agent.SetDestination(_dest);
            return;
        }

        _retries = 0;
        if (NavMesh.SamplePosition(_dest, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            _agent.Warp(hit.position);
    }

    void UpdateLabel()
    {
        string text = "";
        for (int i = 0; i < _list.lines.Count; i++)
        {
            ShoppingLine line = _list.lines[i];
            text += line.product.name + " " + line.collected + "/" + line.quantity;
            if (line.abandoned) text += " (n/a)";
            if (i < _list.lines.Count - 1) text += "\n";
        }
        _label.text = text;
    }
}