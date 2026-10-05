using System.Collections.Generic;
using UnityEngine;

public class ProductionBuilding : InteractionZone, IItemSource, IItemDestination
{
    public enum State { Idle, CheckingInput, Producing, OutputReady, WaitingForPlayer }

    public BuildingDef def;
    public Transform inputRoot;
    public Transform outputRoot;

    readonly Dictionary<Product, int> _inputs = new Dictionary<Product, int>();
    readonly List<StackItem> _outputs = new List<StackItem>();
    State _state = State.Idle;
    float _progress;
    float _idleTimer;
    TextMesh _label;
    Transform _barFill;
    string _lastText;

    public static ProductionBuilding Create(BuildingDef def, Vector3 position)
    {
        var root = new GameObject(def.name);
        root.transform.position = position;
        Transform t = root.transform;

        var box = root.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, 0.9f, 0f);
        box.size = new Vector3(2.2f, 1.8f, 1.8f);

        // The mill and the oven have their own models
        if (def.look != BuildingLook.Tank) return CreateCustom(def, root);

        Color steel = new Color(0.42f, 0.46f, 0.52f);
        Color dark = new Color(0.22f, 0.25f, 0.3f);

        Look.Part(t, PrimitiveType.Cube, steel, new Vector3(0f, 0.45f, 0f), new Vector3(2.2f, 0.9f, 1.8f));
        Look.Part(t, PrimitiveType.Cylinder, def.color, new Vector3(0f, 1.45f, 0f), new Vector3(1.2f, 0.55f, 1.2f));
        Look.Part(t, PrimitiveType.Cylinder, dark, new Vector3(0f, 2.02f, 0f), new Vector3(1.3f, 0.05f, 1.3f));
        Look.Part(t, PrimitiveType.Cylinder, steel, new Vector3(0f, 2.2f, 0f), new Vector3(0.6f, 0.15f, 0.6f));
        Look.Part(t, PrimitiveType.Cylinder, dark, new Vector3(0f, 2.36f, 0f), new Vector3(0.4f, 0.02f, 0.4f));
        Look.Part(t, PrimitiveType.Sphere, Color.white, new Vector3(-0.6f, 0.55f, -0.92f), Vector3.one * 0.22f);
        Look.Part(t, PrimitiveType.Sphere, Color.white, new Vector3(0.6f, 0.55f, -0.92f), Vector3.one * 0.22f);
        Look.Part(t, PrimitiveType.Cube, new Color(0.78f, 0.58f, 0.36f), new Vector3(0f, 0.5f, -1.15f), new Vector3(1.5f, 0.1f, 0.7f));

        // Progress bar above the tank
        Look.Part(t, PrimitiveType.Cube, dark, new Vector3(0f, 2.75f, 0f), new Vector3(1.6f, 0.14f, 0.08f));
        Transform fill = Look.Part(t, PrimitiveType.Cube, new Color(0.4f, 0.9f, 0.4f), new Vector3(-0.75f, 2.75f, -0.02f), new Vector3(0.001f, 0.1f, 0.08f));

        var inputRoot = new GameObject("Input Root").transform;
        inputRoot.SetParent(t, false);
        inputRoot.localPosition = new Vector3(0f, 2.35f, 0f);

        var outputRoot = new GameObject("Output Root").transform;
        outputRoot.SetParent(t, false);
        outputRoot.localPosition = new Vector3(0f, 0.55f, -1.15f);

        var building = root.AddComponent<ProductionBuilding>();
        building.def = def;
        building.inputRoot = inputRoot;
        building.outputRoot = outputRoot;
        building._barFill = fill;
        building.radius = 3f;
        building.markerColor = Color.Lerp(def.color, Color.white, 0.6f);
        building._label = WorldLabel.Create(t, new Vector3(0f, 3.4f, 0f));
        building.Refresh();
        return building;
    }

    // ---- Staff support: a machine accepts ingredients and gives out finished products ----

    Product[] _accepted;

    public Vector3 WorkerSpot => transform.position + new Vector3(0f, 0f, -2.5f);

    public Product[] AcceptedProducts
    {
        get
        {
            if (_accepted == null)
            {
                _accepted = new Product[def.recipe.inputs.Length];
                for (int i = 0; i < _accepted.Length; i++) _accepted[i] = def.recipe.inputs[i].product;
            }
            return _accepted;
        }
    }

    int InputLimit(Product product)
    {
        foreach (RecipeInput input in def.recipe.inputs)
            if (input.product == product) return input.quantity * def.inputBatches;
        return 0;
    }

    public int FreeSpace(Product product)
    {
        _inputs.TryGetValue(product, out int stored);
        return Mathf.Max(0, InputLimit(product) - stored);
    }

    public float FillRatio(Product product)
    {
        int limit = InputLimit(product);
        if (limit == 0) return 1f;

        _inputs.TryGetValue(product, out int stored);
        return (float)stored / limit;
    }

    public bool Accept(StackItem item)
    {
        if (FreeSpace(item.product) <= 0) return false;

        _inputs.TryGetValue(item.product, out int stored);
        _inputs[item.product] = stored + 1;
        item.MoveTo(inputRoot, Vector3.zero);
        Destroy(item.gameObject, 0.3f);
        Refresh();
        SaveManager.RequestSave();
        return true;
    }

    public int AvailableCount(Product product)
    {
        return product == def.recipe.output ? _outputs.Count : 0;
    }

    public StackItem TakeItem(Product product)
    {
        if (product != def.recipe.output || _outputs.Count == 0) return null;

        StackItem item = _outputs[_outputs.Count - 1];
        _outputs.RemoveAt(_outputs.Count - 1);
        Refresh();
        SaveManager.RequestSave();
        return item;
    }

    // Machines with their own look (mill, oven): same behavior, different shape
    static ProductionBuilding CreateCustom(BuildingDef def, GameObject root)
    {
        Transform t = root.transform;
        Transform fill;
        Vector3 inputPos, outputPos;
        Props.SizeCollider(root, def.look);

        if (def.look == BuildingLook.Mill) Props.BuildMill(t, def, out fill, out inputPos, out outputPos);
        else if (def.look == BuildingLook.Chicken) Props.BuildChicken(t, def, out fill, out inputPos, out outputPos);
        else if (def.look == BuildingLook.Cow) Props.BuildCow(t, def, out fill, out inputPos, out outputPos);
        else Props.BuildOven(t, def, out fill, out inputPos, out outputPos);

        var inputRoot = new GameObject("Input Root").transform;
        inputRoot.SetParent(t, false);
        inputRoot.localPosition = inputPos;

        var outputRoot = new GameObject("Output Root").transform;
        outputRoot.SetParent(t, false);
        outputRoot.localPosition = outputPos;

        var building = root.AddComponent<ProductionBuilding>();
        building.def = def;
        building.inputRoot = inputRoot;
        building.outputRoot = outputRoot;
        building._barFill = fill;
        building.radius = Props.ZoneRadius(def.look);
        building.markerColor = Color.Lerp(def.color, Color.white, 0.6f);
        building._label = WorldLabel.Create(t, new Vector3(0f, 3.4f, 0f));
        building.Refresh();
        return building;
    }

    void Awake()
    {
        Shop.Machines.Add(this);
    }

    void OnDestroy()
    {
        Shop.Machines.Remove(this);
    }

    // ---- State machine (runs every frame) ----

    protected override void Tick()
    {
        EnsureIndicator();
        RunStateMachine();
    }

    AttentionIndicator _attention;

    void EnsureIndicator()
    {
        if (_attention != null) return;
        _attention = AttentionIndicator.Attach(transform, new Vector3(0f, 4.2f, 0f), CurrentAttention);
    }

    AttentionLevel CurrentAttention()
    {
        if (_outputs.Count > 0) return AttentionLevel.High;                       // product ready to collect
        if (_state == State.Idle && !CanStart()) return AttentionLevel.Low;       // waiting for ingredients
        return AttentionLevel.None;
    }

    void RunStateMachine()
    {
        switch (_state)
        {
            case State.Idle:
                _idleTimer -= Time.deltaTime;
                if (_idleTimer <= 0f)
                {
                    _idleTimer = 0.25f;
                    _state = State.CheckingInput;
                }
                break;

            case State.CheckingInput:
                if (CanStart())
                {
                    ConsumeInputs();
                    _progress = 0f;
                    _state = State.Producing;
                    Refresh();
                }
                else
                {
                    _state = State.Idle;
                }
                break;

            case State.Producing:
                _progress += Time.deltaTime;
                if (_progress >= def.recipe.productionTime)
                {
                    Produce();
                    _state = State.OutputReady;
                }
                UpdateBar();
                break;

            case State.OutputReady:
                _state = _outputs.Count >= def.outputCapacity ? State.WaitingForPlayer : State.Idle;
                break;

            case State.WaitingForPlayer:
                if (_outputs.Count < def.outputCapacity) _state = State.Idle;
                break;
        }
    }

    // Resources are only used when production can really happen
    bool CanStart()
    {
        foreach (RecipeInput input in def.recipe.inputs)
        {
            _inputs.TryGetValue(input.product, out int stored);
            if (stored < input.quantity) return false;
        }
        return _outputs.Count + def.recipe.outputQuantity <= def.outputCapacity;
    }

    void ConsumeInputs()
    {
        foreach (RecipeInput input in def.recipe.inputs)
            _inputs[input.product] -= input.quantity;
    }

    void Produce()
    {
        for (int i = 0; i < def.recipe.outputQuantity; i++) SpawnOutput();
        _progress = 0f;
        UpdateBar();
        Refresh();
        SaveManager.RequestSave();
    }

    void SpawnOutput()
    {
        StackItem item = StackItem.Create(def.recipe.output, outputRoot.position + Vector3.up * 0.5f);
        item.MoveTo(outputRoot, OutputSlot(_outputs.Count));
        _outputs.Add(item);
    }

    Vector3 OutputSlot(int index)
    {
        return new Vector3((index - (def.outputCapacity - 1) / 2f) * 0.5f, 0.22f, 0f);
    }

    // ---- Player interaction ----

    protected override string Prompt
    {
        get
        {
            PlayerInventory player = PlayerInventory.Instance;
            if (player == null) return null;

            foreach (RecipeInput input in def.recipe.inputs)
                if (player.Has(input.product) && FreeSpace(input.product) > 0) return "FEED";

            return _outputs.Count > 0 && !player.IsFull ? "COLLECT" : null;
        }
    }

    protected override void Interact(PlayerInventory player)
    {
        // 1) Deposit ingredients (one per tick)
        foreach (RecipeInput input in def.recipe.inputs)
        {
            _inputs.TryGetValue(input.product, out int stored);
            if (stored >= input.quantity * def.inputBatches) continue;

            StackItem item = player.RemoveLast(input.product);
            if (item == null) continue;

            _inputs[input.product] = stored + 1;
            item.MoveTo(inputRoot, Vector3.zero);
            Destroy(item.gameObject, 0.3f);
            Refresh();
            SaveManager.RequestSave();
            return;
        }

        // 2) Collect finished products
        if (_outputs.Count > 0 && !player.IsFull)
        {
            StackItem output = _outputs[_outputs.Count - 1];
            _outputs.RemoveAt(_outputs.Count - 1);
            player.Add(output);
            Refresh();
            SaveManager.RequestSave();
        }
    }

    // ---- Saving / loading ----

    public MachineSave Capture()
    {
        var save = new MachineSave
        {
            buildingId = def.id,
            position = transform.position,
            outputs = _outputs.Count,
            producing = _state == State.Producing,
            progress = _progress
        };
        foreach (var pair in _inputs)
            save.inputs.Add(new InputSave { productId = pair.Key.id, count = pair.Value });
        return save;
    }

    public void Restore(MachineSave save)
    {
        foreach (InputSave input in save.inputs)
        {
            Product product = GameConfig.FindProduct(input.productId);
            if (product != null) _inputs[product] = Mathf.Max(0, input.count);
        }

        int outputs = Mathf.Clamp(save.outputs, 0, def.outputCapacity);
        for (int i = 0; i < outputs; i++) SpawnOutput();

        if (save.producing)
        {
            _state = State.Producing;
            _progress = Mathf.Max(0f, save.progress);
        }
        Refresh();
        UpdateBar();
    }

    // Lets the machine work through time that passed while the game wasn't running
    public void FastForward(double seconds)
    {
        if (!GameConfig.OfflineProduction || seconds <= 0.0) return;

        double remaining = seconds;
        for (int guard = 0; remaining > 0.0 && guard < 1000; guard++)
        {
            if (_state != State.Producing)
            {
                if (!CanStart()) break;
                ConsumeInputs();
                _progress = 0f;
                _state = State.Producing;
            }

            double needed = def.recipe.productionTime - _progress;
            if (remaining < needed)
            {
                _progress += (float)remaining;
                break;
            }

            remaining -= needed;
            Produce();
            _state = State.OutputReady;
        }
        Refresh();
        UpdateBar();
    }

    // ---- Visuals ----

    void UpdateBar()
    {
        float p = _state == State.Producing ? Mathf.Clamp01(_progress / def.recipe.productionTime) : 0f;
        float width = Mathf.Max(0.001f, 1.5f * p);
        _barFill.localScale = new Vector3(width, 0.1f, 0.08f);
        _barFill.localPosition = new Vector3(-0.75f + width / 2f, 2.75f, -0.02f);
    }

    void Refresh()
    {
        string text = def.name;
        foreach (RecipeInput input in def.recipe.inputs)
        {
            _inputs.TryGetValue(input.product, out int stored);
            text += "\n" + input.product.name + " " + stored + "/" + input.quantity * def.inputBatches;
        }
        text += "\n" + def.recipe.output.name + " " + _outputs.Count + "/" + def.outputCapacity;

        if (text == _lastText) return;
        _lastText = text;
        _label.text = text;
    }
}