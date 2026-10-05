using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShelfSave
{
    public string productId;
    public Vector3 position;
    public int count;
}

[System.Serializable]
public class CropSave
{
    public string productId;
    public Vector3 position;
    public int ready;
    public float timer;
}

[System.Serializable]
public class InputSave
{
    public string productId;
    public int count;
}

[System.Serializable]
public class MachineSave
{
    public string buildingId;
    public Vector3 position;
    public List<InputSave> inputs = new List<InputSave>();
    public int outputs;
    public bool producing;
    public float progress;
}

[System.Serializable]
public class PadSave
{
    public string id;
    public int paid;
}

[System.Serializable]
public class SaveData
{
    public const int CurrentVersion = 2;

    public int version = CurrentVersion;
    public long savedAtUtcTicks;
    public int money;
    public int capacity;
    public Vector3 playerPosition;
    public float playerYaw;
    public List<string> inventory = new List<string>();
    public List<ShelfSave> shelves = new List<ShelfSave>();
    public List<CropSave> crops = new List<CropSave>();
    public List<MachineSave> machines = new List<MachineSave>();
    public List<string> unlocked = new List<string>();
    public List<PadSave> pads = new List<PadSave>();
    public List<UpgradeSave> upgrades = new List<UpgradeSave>();
    public List<StaffSave> staff = new List<StaffSave>();
}