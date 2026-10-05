using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StaffWorker : MonoBehaviour
{
    enum State { Idle, ToSource, Loading, ToDestination, Unloading }

    public StaffDef def;
    public Transform carryRoot;
    public CheckoutCounter post;   // cashiers only

    NavMeshAgent _agent;
    State _state = State.Idle;
    Vector3 _dest;
    float _timer;
    float _stuckTime;
    float _loadWaited;
    int _retries;

    // The current delivery job
    IItemSource _source;
    IItemDestination _destination;
    Product _product;
    int _amount;

    // Scratch values used while choosing the best job
    IItemDestination _planDest;
    IItemSource _planSource;
    Product _planProduct;
    int _planAmount;
    float _planRatio;
    bool _planned;

    readonly List<StackItem> _carried = new List<StackItem>();

    public bool IsAtPost => post != null && Arrived;

    int Capacity => def.carryCapacity + StaffManager.CarryBonus;

    float ActionInterval => 0.2f / Mathf.Max(0.1f, def.workSpeed * StaffManager.SpeedMultiplier);

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

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _dest = transform.position;
        StaffManager.All.Add(this);
    }

    void OnDestroy()
    {
        StaffManager.All.Remove(this);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        _agent.speed = def.moveSpeed * StaffManager.SpeedMultiplier;
        if (def.role == StaffRole.Cashier) UpdateCashier(dt);
        else UpdateHauler(dt);
    }

    // ---- Cashier ----

    void UpdateCashier(float dt)
    {
        if (post == null)
        {
            _timer -= dt;
            if (_timer > 0f) return;
            _timer = 1f;

            post = Shop.FindUnstaffedCounter();
            if (post != null)
            {
                post.cashier = this;
                MoveTo(post.StaffSpot);
            }
            return;
        }

        CheckStuck(dt);

        // Face the counter once we're at our post
        if (Arrived)
        {
            Vector3 look = post.transform.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 8f * dt);
        }
    }

    // ---- Shelver and Baker: move items from sources to destinations ----

    void UpdateHauler(float dt)
    {
        switch (_state)
        {
            case State.Idle:
                _timer -= dt;
                if (_timer > 0f) break;
                _timer = 0.4f;
                PlanJob();
                break;

            case State.ToSource:
                CheckStuck(dt);
                if (Arrived)
                {
                    _state = State.Loading;
                    _timer = 0f;
                    _loadWaited = 0f;
                }
                break;

            case State.Loading:
                _timer -= dt;
                if (_timer > 0f) break;
                _timer = ActionInterval;

                if (_carried.Count < _amount)
                {
                    if (_source.AvailableCount(_product) > 0)
                    {
                        StackItem taken = _source.TakeItem(_product);
                        if (taken != null)
                        {
                            taken.MoveTo(carryRoot, new Vector3(0f, _carried.Count * 0.32f, 0f));
                            _carried.Add(taken);
                            _loadWaited = 0f;
                        }
                        break;
                    }

                    // Nothing ready yet: wait a little for more, then go with what we have
                    _loadWaited += ActionInterval;
                    if (_loadWaited < GameConfig.StaffMaxLoadWait) break;
                }

                if (_carried.Count > 0) GoDeliver(_destination);
                else _state = State.Idle;
                break;

            case State.ToDestination:
                CheckStuck(dt);
                if (Arrived)
                {
                    _state = State.Unloading;
                    _timer = 0f;
                }
                break;

            case State.Unloading:
                _timer -= dt;
                if (_timer > 0f) break;
                _timer = ActionInterval;

                if (_carried.Count == 0)
                {
                    _state = State.Idle;
                    break;
                }

                StackItem last = _carried[_carried.Count - 1];
                if (_destination.Accept(last)) _carried.RemoveAt(_carried.Count - 1);
                else _state = State.Idle;   // it filled up: decide again (we keep the rest)
                break;
        }
    }

    void PlanJob()
    {
        // Still holding something (a destination filled up)? Deliver it first.
        if (_carried.Count > 0)
        {
            Product held = _carried[0].product;
            IItemDestination target = FindDestinationFor(held);
            if (target != null)
            {
                _product = held;
                GoDeliver(target);
            }
            return;
        }

        _planned = false;
        _planRatio = float.MaxValue;

        if (def.role == StaffRole.Shelver)
        {
            foreach (Shelf shelf in Shop.Shelves) Consider(shelf);
        }
        else if (def.role == StaffRole.Operator)
        {
            foreach (ProductionBuilding machine in Shop.Machines) Consider(machine);
        }

        if (!_planned) return;

        _destination = _planDest;
        _source = _planSource;
        _product = _planProduct;
        _amount = _planAmount;

        MoveTo(_source.WorkerSpot);
        _state = State.ToSource;
    }

    // Is this destination worth serving? Remember it if it's the neediest so far.
    void Consider(IItemDestination destination)
    {
        foreach (Product product in destination.AcceptedProducts)
        {
            int free = destination.FreeSpace(product);
            if (free <= 0) continue;

            IItemSource source = FindSource(product);
            if (source == null) continue;

            float ratio = destination.FillRatio(product);
            if (ratio >= _planRatio) continue;

            _planRatio = ratio;
            _planDest = destination;
            _planSource = source;
            _planProduct = product;
            _planAmount = Mathf.Min(free, Capacity);
            _planned = true;
        }
    }

    // The plant or machine with the most of this product ready (at least one)
    static IItemSource FindSource(Product product)
    {
        IItemSource best = null;
        int bestCount = 0;

        foreach (CropPlant crop in Shop.Crops)
        {
            int count = crop.AvailableCount(product);
            if (count > bestCount)
            {
                bestCount = count;
                best = crop;
            }
        }

        foreach (ProductionBuilding machine in Shop.Machines)
        {
            int count = machine.AvailableCount(product);
            if (count > bestCount)
            {
                bestCount = count;
                best = machine;
            }
        }

        return best;
    }

    IItemDestination FindDestinationFor(Product product)
    {
        if (def.role == StaffRole.Shelver)
        {
            foreach (Shelf shelf in Shop.Shelves)
                if (shelf.FreeSpace(product) > 0) return shelf;
        }
        else if (def.role == StaffRole.Operator)
        {
            foreach (ProductionBuilding machine in Shop.Machines)
                if (machine.FreeSpace(product) > 0) return machine;
        }
        return null;
    }

    void GoDeliver(IItemDestination destination)
    {
        _destination = destination;
        MoveTo(destination.WorkerSpot);
        _state = State.ToDestination;
    }

    // ---- Navigation ----

    void MoveTo(Vector3 destination)
    {
        _dest = destination;
        _stuckTime = 0f;
        _retries = 0;
        if (_agent.isOnNavMesh) _agent.SetDestination(destination);
    }

    // If we stop making progress: recalculate the path, and as a last resort relocate
    void CheckStuck(float dt)
    {
        if (Arrived)
        {
            _stuckTime = 0f;
            return;
        }
        if (_agent.pathPending) return;

        bool notMoving = _agent.velocity.sqrMagnitude < 0.04f
                         || _agent.pathStatus == NavMeshPathStatus.PathInvalid;
        _stuckTime = notMoving ? _stuckTime + dt : 0f;
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
}