using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    float _timeSinceSave;
    bool _dirty;
    long _pausedAtTicks;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Call this whenever something worth saving changes. Saves are batched.
    public static void RequestSave()
    {
        if (Instance != null) Instance._dirty = true;
    }

    void Update()
    {
        _timeSinceSave += Time.unscaledDeltaTime;

        bool changedRecently = _dirty && _timeSinceSave >= GameConfig.SaveMinInterval;
        if (changedRecently || _timeSinceSave >= GameConfig.AutosaveInterval) SaveNow();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveNow();
            _pausedAtTicks = System.DateTime.UtcNow.Ticks;
        }
        else if (_pausedAtTicks != 0)
        {
            // Coming back: let plants grow for the time we were away
            double away = (System.DateTime.UtcNow.Ticks - _pausedAtTicks) / (double)System.TimeSpan.TicksPerSecond;
            foreach (CropPlant crop in Shop.Crops) crop.FastForward(away);
            foreach (ProductionBuilding machine in Shop.Machines) machine.FastForward(away);
        }
    }

    void OnApplicationQuit()
    {
        SaveNow();
    }

    void SaveNow()
    {
        SaveSystem.Save(Capture());
        _dirty = false;
        _timeSinceSave = 0f;
    }

    static SaveData Capture()
    {
        var data = new SaveData();
        data.savedAtUtcTicks = System.DateTime.UtcNow.Ticks;
        data.money = Wallet.Money;

        PlayerInventory inventory = PlayerInventory.Instance;
        if (inventory != null)
        {
            data.capacity = inventory.Capacity;
            data.playerPosition = inventory.transform.position;
            data.playerYaw = inventory.transform.eulerAngles.y;
            inventory.GetProductIds(data.inventory);
        }

        foreach (Shelf shelf in Shop.Shelves)
        {
            data.shelves.Add(new ShelfSave
            {
                productId = shelf.product.id,
                position = shelf.transform.position,
                count = shelf.Count
            });
        }

        foreach (CropPlant crop in Shop.Crops)
        {
            data.crops.Add(new CropSave
            {
                productId = crop.crop.product.id,
                position = crop.transform.position,
                ready = crop.ReadyCount,
                timer = crop.Timer
            });
        }

        foreach (ProductionBuilding machine in Shop.Machines)
            data.machines.Add(machine.Capture());

        if (UnlockManager.Instance != null)
            UnlockManager.Instance.Capture(data.unlocked, data.pads);

        UpgradeSystem.Capture(data.upgrades);
        StaffManager.Capture(data.staff);

        return data;
    }
}