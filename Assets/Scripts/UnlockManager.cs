using System.Collections.Generic;
using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    public static UnlockManager Instance { get; private set; }

    readonly HashSet<string> _unlocked = new HashSet<string>();
    readonly Dictionary<string, UnlockPad> _pads = new Dictionary<string, UnlockPad>();
    readonly Dictionary<string, int> _savedProgress = new Dictionary<string, int>();
    float _timer;
    float _holdPadsUntil;
    float _rebakeAt = -1f;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static bool IsUnlocked(string id)
    {
        return Instance != null && Instance._unlocked.Contains(id);
    }

    public void Restore(List<string> unlockedIds, List<PadSave> progress)
    {
        foreach (string id in unlockedIds) _unlocked.Add(id);
        foreach (PadSave pad in progress) _savedProgress[pad.id] = pad.paid;
    }

    public void Capture(List<string> unlockedIds, List<PadSave> progress)
    {
        foreach (string id in _unlocked) unlockedIds.Add(id);
        foreach (var pair in _pads)
        {
            if (pair.Value != null && pair.Value.Paid > 0)
                progress.Add(new PadSave { id = pair.Key, paid = pair.Value.Paid });
        }
    }

    void Update()
    {
        // Refresh customer pathing once a new building has finished its build-in animation
        if (_rebakeAt > 0f && Time.time >= _rebakeAt)
        {
            _rebakeAt = -1f;
            StoreNav.Build();
        }

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = 0.5f;

        // After an expansion, wait for the animation before showing the new pads
        if (Time.time < _holdPadsUntil) return;

        foreach (UnlockDef def in GameConfig.Unlocks)
        {
            if (_unlocked.Contains(def.id) || _pads.ContainsKey(def.id)) continue;
            if (RequirementsMet(def)) SpawnPad(def);
        }
    }

    // A purchase is only offered when everything it depends on is owned
    static bool RequirementsMet(UnlockDef def)
    {
        if (def.requiresUnlocked != null)
        {
            foreach (string id in def.requiresUnlocked)
                if (!IsUnlocked(id)) return false;
        }
        if (def.requiresProduced != null && !Shop.IsProduced(def.requiresProduced)) return false;
        return true;
    }

    void SpawnPad(UnlockDef def)
    {
        Color color = Color.white;
        switch (def.kind)
        {
            case UnlockKind.Machine: color = def.building.color; break;
            case UnlockKind.Staff: color = def.staff.shirt; break;
            case UnlockKind.Zone: color = new Color(1f, 0.85f, 0.3f); break;
            case UnlockKind.Crop: color = def.crop.product.color; break;
            default: if (def.product != null) color = def.product.color; break;
        }

        UnlockPad pad = UnlockPad.Create(def.title, def.position, def.cost, color, () => OnPurchased(def));
        pad.Id = def.id;
        if (_savedProgress.TryGetValue(def.id, out int paid)) pad.Restore(paid);
        _pads[def.id] = pad;
    }

    void OnPurchased(UnlockDef def)
    {
        _unlocked.Add(def.id);
        _pads.Remove(def.id);
        _savedProgress.Remove(def.id);

        switch (def.kind)
        {
            case UnlockKind.Shelf:
                ConstructionPop.Play(Shelf.Create(def.product, def.position).gameObject);
                break;
            case UnlockKind.Machine:
                ConstructionPop.Play(ProductionBuilding.Create(def.building, def.position).gameObject);
                break;
            case UnlockKind.Crop:
                ConstructionPop.Play(CropPlant.Create(def.crop, def.position).gameObject);
                break;
            case UnlockKind.Staff:
                StaffManager.Hire(def.staff, def.position);
                break;
            case UnlockKind.Zone:
                Zones.Build(def.zone, true);
                _holdPadsUntil = Time.time + 1.6f;
                break;
        }

        _rebakeAt = Time.time + 1.4f;
        SaveManager.RequestSave();
    }
}