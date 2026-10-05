public enum UpgradeKind { Capacity, StaffSpeed, StaffCapacity }

public class UpgradeDef
{
    public readonly string id;
    public readonly string title;
    public readonly UpgradeKind kind;
    public readonly int[] values;   // the value at level 0, 1, 2, ...
    public readonly int[] costs;    // the price of going from level i to i+1

    public int MaxLevel => costs.Length;

    public UpgradeDef(string id, string title, UpgradeKind kind, int[] values, int[] costs)
    {
        this.id = id;
        this.title = title;
        this.kind = kind;
        this.values = values;
        this.costs = costs;
    }
}

[System.Serializable]
public class UpgradeSave
{
    public string id;
    public int level;
}