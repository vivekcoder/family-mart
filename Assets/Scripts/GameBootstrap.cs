using UnityEngine;
using UnityEngine.AI;

public static class GameBootstrap
{
    static readonly Vector3 DefaultPlayerStart = new Vector3(-3.5f, 1f, 0.5f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SaveData save = SaveSystem.Load();

        double offlineSeconds = 0.0;
        if (save != null)
        {
            long ticks = System.DateTime.UtcNow.Ticks - save.savedAtUtcTicks;
            offlineSeconds = System.Math.Max(0.0, ticks / (double)System.TimeSpan.TicksPerSecond);
        }

        Look.SetupScene();
        BuildFloor();
        Look.BuildSurroundings();

        // The starting mart, plus every zone the player has bought
        Zones.Begin();
        if (save != null)
        {
            foreach (ZoneDef zone in GameConfig.AllZones)
                if (save.unlocked.Contains(zone.id)) Zones.Build(zone, false);
        }

        // Build the mart: from the save if there is one, otherwise the starting setup
        bool hasBuildings = save != null && (save.crops.Count > 0 || save.shelves.Count > 0 || save.machines.Count > 0);
        if (hasBuildings) BuildMartFromSave(save, offlineSeconds);
        else BuildStartingMart();

        CheckoutCounter.Create(new Vector3(-2.2f, 0f, -3f), new Vector3(0f, 0f, -3f));

        // Bake walkable area now: the floors and all obstacles exist, the player doesn't yet
        StoreNav.Build();

        Vector3 start = save != null ? save.playerPosition : DefaultPlayerStart;
        start.y = 1f;
        float yaw = save != null ? save.playerYaw : 0f;
        GameObject player = BuildPlayer(start, yaw);

        if (save != null)
        {
            Wallet.Set(save.money);

            UpgradeSystem.Restore(save.upgrades);
            UpgradeSystem.ApplyAll();

            var inventory = player.GetComponent<PlayerInventory>();
            inventory.Restore(save.inventory);
        }

        var unlocks = new GameObject("Unlock Manager").AddComponent<UnlockManager>();
        if (save != null) unlocks.Restore(save.unlocked, save.pads);

        if (save != null)
        {
            foreach (StaffSave hired in save.staff)
            {
                StaffDef staffDef = GameConfig.FindStaff(hired.defId);
                if (staffDef != null) StaffManager.Hire(staffDef, hired.position);
            }
        }

        // Upgrades are pads too
        UpgradePad.Create(GameConfig.CarryCapacity, new Vector3(-8.5f, 0f, -5f));
        UpgradePad.Create(GameConfig.StaffSpeedUpgrade, new Vector3(8.5f, 0f, -8f));
        UpgradePad.Create(GameConfig.StaffCarryUpgrade, new Vector3(4.5f, 0f, -8.5f));

        new GameObject("Customer Spawner").AddComponent<CustomerSpawner>();
        new GameObject("Save Manager").AddComponent<SaveManager>();

        MoneyHud.Create();
        InteractionHintHud.Create();
        new GameObject("Attention System").AddComponent<AttentionSystem>();
        SetupCamera(player.transform);
    }

    static void BuildFloor()
    {
        // The starting mart: 20 x 20 units
        var floor = MakePrimitive(PrimitiveType.Plane, "Floor", Color.white, true);
        floor.GetComponent<Renderer>().sharedMaterial = Look.FloorMaterial();
        floor.transform.localScale = new Vector3(2f, 1f, 2f);
    }

    // What a brand-new player owns: one tomato plant and one tomato shelf
    static void BuildStartingMart()
    {
        CropPlant.Create(GameConfig.TomatoPlant, new Vector3(-6f, 0f, 3f));
        Shelf.Create(GameConfig.Tomato, new Vector3(6f, 0f, 3f));
    }

    static void BuildMartFromSave(SaveData save, double offlineSeconds)
    {
        foreach (CropSave saved in save.crops)
        {
            CropDef def = GameConfig.FindCrop(saved.productId);
            if (def == null) continue;

            CropPlant plant = CropPlant.Create(def, saved.position);
            plant.Restore(saved.ready, saved.timer);
            plant.FastForward(offlineSeconds);
        }

        foreach (MachineSave saved in save.machines)
        {
            BuildingDef def = GameConfig.FindBuilding(saved.buildingId);
            if (def == null) continue;

            ProductionBuilding machine = ProductionBuilding.Create(def, saved.position);
            machine.Restore(saved);
            machine.FastForward(offlineSeconds);
        }

        foreach (ShelfSave saved in save.shelves)
        {
            Product product = GameConfig.FindProduct(saved.productId);
            if (product == null) continue;

            Shelf.Create(product, saved.position).Restore(saved.count);
        }
    }

    static GameObject BuildPlayer(Vector3 position, float yaw)
    {
        var player = new GameObject("Player");
        player.transform.position = position;
        player.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        player.layer = 2;   // "Ignore Raycast": the NavMesh ignores it
        player.AddComponent<CharacterController>();

        // Customers steer around the player
        var obstacle = player.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Capsule;
        obstacle.radius = 0.5f;
        obstacle.height = 2f;
        obstacle.carving = false;

        // The player's root sits at capsule height (1.0), so the character's feet are 1.0 below
        var holder = new GameObject("Character Holder").transform;
        holder.SetParent(player.transform, false);
        holder.localPosition = new Vector3(0f, -1f, 0f);
        CharacterBuilder.Build(holder, CharacterStyle.Player());

        // Where carried items stack up (in front of the player)
        var stackRoot = new GameObject("Stack Root").transform;
        stackRoot.SetParent(player.transform, false);
        stackRoot.localPosition = new Vector3(0f, 0.2f, 1.1f);

        var inventory = player.AddComponent<PlayerInventory>();
        inventory.stackRoot = stackRoot;

        var controller = player.AddComponent<PlayerController>();
        controller.joystick = VirtualJoystick.Create();

        return player;
    }

    static void SetupCamera(Transform target)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGO.tag = "MainCamera";
            cam = camGO.GetComponent<Camera>();
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Look.Sky;
        cam.transform.rotation = Quaternion.Euler(53f, 0f, 0f);
        var follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = target;
        cam.transform.position = target.position + follow.offset;
    }

    public static GameObject MakePrimitive(PrimitiveType type, string name, Color color, bool keepCollider)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.GetComponent<Renderer>().sharedMaterial = Look.Mat(color);
        if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }
}