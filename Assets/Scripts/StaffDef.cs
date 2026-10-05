using UnityEngine;

public enum StaffRole { Shelver, Operator, Cashier }

public class StaffDef
{
    public readonly string id;
    public readonly string title;
    public readonly StaffRole role;
    public readonly float moveSpeed;
    public readonly int carryCapacity;   // items carried per trip (0 for cashiers)
    public readonly float workSpeed;     // 1 = as fast as the player, 0.5 = half speed
    public readonly Color shirt;

    public StaffDef(string id, string title, StaffRole role, float moveSpeed, int carryCapacity, float workSpeed, Color shirt)
    {
        this.id = id;
        this.title = title;
        this.role = role;
        this.moveSpeed = moveSpeed;
        this.carryCapacity = carryCapacity;
        this.workSpeed = workSpeed;
        this.shirt = shirt;
    }
}

[System.Serializable]
public class StaffSave
{
    public string defId;
    public Vector3 position;
}