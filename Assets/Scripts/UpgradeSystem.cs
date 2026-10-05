using System.Collections.Generic;
using UnityEngine;

public static class UpgradeSystem
{
    static readonly Dictionary<string, int> _levels = new Dictionary<string, int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        _levels.Clear();
    }

    public static int GetLevel(UpgradeDef def)
    {
        _levels.TryGetValue(def.id, out int level);
        return level;
    }

    public static bool IsMax(UpgradeDef def) => GetLevel(def) >= def.MaxLevel;

    public static int CurrentValue(UpgradeDef def) => def.values[GetLevel(def)];

    public static int NextValue(UpgradeDef def) => def.values[Mathf.Min(GetLevel(def) + 1, def.MaxLevel)];

    // Only valid when the upgrade isn't maxed out
    public static int NextCost(UpgradeDef def) => def.costs[GetLevel(def)];

    public static bool CanAfford(UpgradeDef def) => !IsMax(def) && Wallet.Money >= NextCost(def);

    public static bool TryUpgrade(UpgradeDef def)
    {
        if (IsMax(def)) return false;
        if (!Wallet.TrySpend(NextCost(def))) return false;

        _levels[def.id] = GetLevel(def) + 1;
        Apply(def);
        SaveManager.RequestSave();
        return true;
    }

    // Puts the upgrade's current value into effect
    public static void Apply(UpgradeDef def)
    {
        int value = CurrentValue(def);

        switch (def.kind)
        {
            case UpgradeKind.Capacity:
                if (PlayerInventory.Instance != null) PlayerInventory.Instance.Capacity = value;
                break;
            case UpgradeKind.StaffSpeed:
                StaffManager.SpeedMultiplier = value / 100f;
                break;
            case UpgradeKind.StaffCapacity:
                StaffManager.CarryBonus = value;
                break;
        }
    }

    public static void ApplyAll()
    {
        foreach (UpgradeDef def in GameConfig.Upgrades) Apply(def);
    }

    public static void Restore(List<UpgradeSave> saved)
    {
        _levels.Clear();
        foreach (UpgradeSave entry in saved)
        {
            UpgradeDef def = GameConfig.FindUpgrade(entry.id);
            if (def != null) _levels[def.id] = Mathf.Clamp(entry.level, 0, def.MaxLevel);
        }
    }

    public static void Capture(List<UpgradeSave> into)
    {
        foreach (var pair in _levels)
            into.Add(new UpgradeSave { id = pair.Key, level = pair.Value });
    }
}