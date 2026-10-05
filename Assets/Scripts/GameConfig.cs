using System.Collections.Generic;
using UnityEngine;

// A crop is a production source: it makes a product over time and stores a few until harvested
public class CropDef
{
    public readonly Product product;
    public readonly float productionTime;   // seconds per cycle
    public readonly int maxStored;          // production pauses when this many are waiting
    public readonly int amountPerCycle;     // how many it makes each cycle
    public CropVisual visual;               // which 3D look to use (set with { visual = ... })

    public CropDef(Product product, float productionTime, int maxStored, int amountPerCycle)
    {
        this.product = product;
        this.productionTime = productionTime;
        this.maxStored = maxStored;
        this.amountPerCycle = amountPerCycle;
    }
}

// Every tunable number in the game lives here
public static class GameConfig
{
    // --- Player ---
    public const int StartingMoney = 5000;
    public const int StartingCapacity = 3;

    // --- Upgrades ---
    // values: the value at level 0, 1, 2, ...   costs: the price of each next level
    public static readonly UpgradeDef CarryCapacity = new UpgradeDef(
        "capacity", "CARRY", UpgradeKind.Capacity,
        new[] { StartingCapacity, 5, 8, 12, 16, 20 },
        new[] { 50, 120, 300, 700, 1500 });

    public static readonly UpgradeDef StaffSpeedUpgrade = new UpgradeDef(
        "staff_speed", "STAFF SPEED %", UpgradeKind.StaffSpeed,
        new[] { 100, 115, 130, 150, 175, 200 },
        new[] { 150, 300, 600, 1200, 2400 });

    public static readonly UpgradeDef StaffCarryUpgrade = new UpgradeDef(
        "staff_carry", "STAFF CARRY", UpgradeKind.StaffCapacity,
        new[] { 0, 1, 2, 3, 4 },
        new[] { 200, 500, 1000, 2000 });

    public static readonly UpgradeDef[] Upgrades = { CarryCapacity, StaffSpeedUpgrade, StaffCarryUpgrade };

    // --- Products (id, name, color, sell price, shape) ---
    public static readonly Product Tomato  = new Product("tomato",  "Tomato",  new Color(0.9f, 0.15f, 0.1f),  5,  ProductShape.Ball);
    public static readonly Product Ketchup = new Product("ketchup", "Ketchup", new Color(0.75f, 0.08f, 0.08f), 40, ProductShape.Bottle);
    public static readonly Product Wheat   = new Product("wheat",   "Wheat",   new Color(0.9f, 0.8f, 0.3f),   4,  ProductShape.Box);
    public static readonly Product Flour   = new Product("flour",   "Flour",   new Color(0.95f, 0.95f, 0.9f), 20, ProductShape.Box);
    public static readonly Product Egg     = new Product("egg",     "Egg",     new Color(0.97f, 0.93f, 0.82f), 12, ProductShape.Egg);
    public static readonly Product Milk    = new Product("milk",    "Milk",    new Color(0.97f, 0.97f, 0.98f), 18, ProductShape.Carton) { shelfLook = ShelfLook.Fridge };
    public static readonly Product Bread   = new Product("bread",   "Bread",   new Color(0.75f, 0.5f, 0.25f), 50, ProductShape.Box);

    public static readonly Product[] AllProducts = { Tomato, Ketchup, Wheat, Flour, Egg, Milk, Bread };

    // --- Crops (product, seconds per cycle, max stored, amount per cycle) ---
    public static readonly CropDef TomatoPlant = new CropDef(Tomato, 1f, 3, 1);
    public static readonly CropDef WheatField = new CropDef(Wheat, 2f, 4, 1) { visual = CropVisual.Stalks };

    public static readonly CropDef[] AllCrops = { TomatoPlant, WheatField };

    // --- Recipes (id, inputs, output, output amount, seconds) ---
    public static readonly RecipeDef KetchupRecipe = new RecipeDef(
        "ketchup", new[] { new RecipeInput(Tomato, 5) }, Ketchup, 1, 4f);

    public static readonly RecipeDef FlourRecipe = new RecipeDef(
        "flour", new[] { new RecipeInput(Wheat, 3) }, Flour, 1, 5f);

    public static readonly RecipeDef EggRecipe = new RecipeDef(
        "egg", new[] { new RecipeInput(Tomato, 1) }, Egg, 1, 8f);

    public static readonly RecipeDef MilkRecipe = new RecipeDef(
        "milk", new[] { new RecipeInput(Wheat, 1) }, Milk, 1, 10f);

    public static readonly RecipeDef BreadRecipe = new RecipeDef(
        "bread", new[] { new RecipeInput(Flour, 1), new RecipeInput(Egg, 1) }, Bread, 1, 6f);

    // --- Production buildings (id, name, recipe, batches of ingredients held, output tray size, color) ---
    public static readonly BuildingDef KetchupMachine = new BuildingDef(
        "ketchup_machine", "Ketchup Machine", KetchupRecipe, 2, 3, new Color(0.8f, 0.15f, 0.12f));

    public static readonly BuildingDef FlourMill = new BuildingDef(
        "flour_mill", "Flour Mill", FlourRecipe, 2, 3, new Color(0.9f, 0.85f, 0.7f)) { look = BuildingLook.Mill };

    public static readonly BuildingDef BreadOven = new BuildingDef(
        "bread_oven", "Bread Oven", BreadRecipe, 2, 3, new Color(0.65f, 0.3f, 0.22f)) { look = BuildingLook.Oven };

    // Animals are production buildings too: feed in, product out (max 4 waiting on the tray)
    public static readonly BuildingDef ChickenBuilding = new BuildingDef(
        "chicken", "Chicken", EggRecipe, 2, 4, new Color(0.97f, 0.95f, 0.9f)) { look = BuildingLook.Chicken };

    public static readonly BuildingDef CowBuilding = new BuildingDef(
        "cow", "Cow", MilkRecipe, 2, 4, new Color(0.96f, 0.96f, 0.94f)) { look = BuildingLook.Cow };

    public static readonly BuildingDef[] AllBuildings = { KetchupMachine, FlourMill, BreadOven, ChickenBuilding, CowBuilding };

    // --- Staff (id, title, role, move speed, carry capacity, work speed, shirt color) ---
    public static readonly StaffDef ShelverStaff = new StaffDef(
        "shelver", "Shelver", StaffRole.Shelver, 4f, 4, 1f, new Color(0.3f, 0.75f, 0.4f));

    public static readonly StaffDef BakerStaff = new StaffDef(
        "baker", "Baker", StaffRole.Operator, 3.5f, 5, 1f, new Color(0.95f, 0.95f, 0.95f));

    public static readonly StaffDef CashierStaff = new StaffDef(
        "cashier", "Cashier", StaffRole.Cashier, 3f, 0, 0.6f, new Color(0.95f, 0.8f, 0.25f));

    public static readonly StaffDef[] AllStaff = { ShelverStaff, BakerStaff, CashierStaff };

    // How long a worker waits at a source for more items before leaving with what it has
    public const float StaffMaxLoadWait = 6f;

    // --- Zones: the world grows in a fixed order (cell on the 20 x 20 grid; the mart is 0,0) ---
    public static readonly ZoneDef FarmTomatoZone = new ZoneDef("zone_farm_tomato", "TOMATO FARM", new Vector2Int(1, 0), ZoneFloor.Dirt);
    public static readonly ZoneDef FarmChickenZone = new ZoneDef("zone_farm_chicken", "CHICKEN AREA", new Vector2Int(2, 0), ZoneFloor.Dirt);
    public static readonly ZoneDef FarmWheatZone = new ZoneDef("zone_farm_wheat", "WHEAT FARM", new Vector2Int(3, 0), ZoneFloor.Dirt);
    public static readonly ZoneDef FarmCowZone = new ZoneDef("zone_farm_cow", "COW AREA", new Vector2Int(4, 0), ZoneFloor.Dirt);
    public static readonly ZoneDef ProductionZone = new ZoneDef("zone_production", "PRODUCTION", new Vector2Int(0, 1), ZoneFloor.Tile);
    public static readonly ZoneDef Mart2Zone = new ZoneDef("zone_mart_2", "MORE MART", new Vector2Int(-1, 0), ZoneFloor.Tile);

    public static readonly ZoneDef[] AllZones =
        { FarmTomatoZone, FarmChickenZone, FarmWheatZone, FarmCowZone, ProductionZone, Mart2Zone };

    // --- Things the player can buy ---
    public static readonly UnlockDef[] Unlocks = BuildUnlocks();

    static string[] Needs(params string[] ids)
    {
        return ids.Length > 0 ? ids : null;
    }

    static UnlockDef ZonePad(ZoneDef zone, int cost, Vector3 position, params string[] requires)
    {
        return new UnlockDef
        {
            id = zone.id, title = "EXPAND: " + zone.title, cost = cost,
            position = position, kind = UnlockKind.Zone, zone = zone,
            requiresUnlocked = Needs(requires)
        };
    }

    // THE RULE: a production source can only be bought after the shelf/fridge it feeds.
    // Every source goes through here, so the rule can't be forgotten.
    static UnlockDef Sourced(UnlockDef def, string storageId)
    {
        var required = new List<string>();
        if (def.requiresUnlocked != null) required.AddRange(def.requiresUnlocked);
        if (!required.Contains(storageId)) required.Add(storageId);
        def.requiresUnlocked = required.ToArray();
        return def;
    }

    static UnlockDef[] BuildUnlocks()
    {
        var list = new List<UnlockDef>();

        // Zones, in the order the world grows
        list.Add(ZonePad(FarmTomatoZone, 300, new Vector3(8.5f, 0f, 0f)));
        list.Add(ZonePad(FarmChickenZone, 600, new Vector3(28.5f, 0f, 0f), FarmTomatoZone.id));
        list.Add(ZonePad(FarmWheatZone, 900, new Vector3(48.5f, 0f, 0f), FarmChickenZone.id));
        list.Add(ZonePad(FarmCowZone, 1400, new Vector3(68.5f, 0f, 0f), FarmWheatZone.id));
        list.Add(ZonePad(ProductionZone, 500, new Vector3(-8f, 0f, 8.5f), FarmTomatoZone.id));
        list.Add(ZonePad(Mart2Zone, 700, new Vector3(-8.5f, 0f, 0f), ProductionZone.id));

        // Storage: shelves and fridges
        list.Add(new UnlockDef
        {
            id = "shelf_ketchup", title = "KETCHUP SHELF", cost = 150,
            position = new Vector3(6f, 0f, -3f), kind = UnlockKind.Shelf, product = Ketchup
        });
        list.Add(new UnlockDef
        {
            id = "shelf_egg", title = "EGG SHELF", cost = 200,
            position = new Vector3(6f, 0f, 8f), kind = UnlockKind.Shelf, product = Egg
        });
        list.Add(new UnlockDef
        {
            id = "fridge_milk", title = "MILK FRIDGE", cost = 250,
            position = new Vector3(-3.2f, 0f, 8f), kind = UnlockKind.Shelf, product = Milk
        });
        list.Add(new UnlockDef
        {
            id = "shelf_bread", title = "BREAD SHELF", cost = 300,
            position = new Vector3(-26f, 0f, 6.5f), kind = UnlockKind.Shelf, product = Bread,
            requiresUnlocked = Needs(Mart2Zone.id)
        });

        // Production Area machines (each needs the shelf it feeds)
        list.Add(Sourced(new UnlockDef
        {
            id = "machine_ketchup", title = "KETCHUP MACHINE", cost = 100,
            position = new Vector3(-7f, 0f, 16f), kind = UnlockKind.Machine, building = KetchupMachine,
            requiresUnlocked = Needs(ProductionZone.id)
        }, "shelf_ketchup"));
        list.Add(new UnlockDef
        {
            id = "machine_mill", title = "FLOUR MILL", cost = 200,
            position = new Vector3(-2.5f, 0f, 16f), kind = UnlockKind.Machine, building = FlourMill,
            requiresUnlocked = Needs(ProductionZone.id), requiresProduced = Wheat
        });
        list.Add(Sourced(new UnlockDef
        {
            id = "machine_oven", title = "BREAD OVEN", cost = 350,
            position = new Vector3(2f, 0f, 16f), kind = UnlockKind.Machine, building = BreadOven,
            requiresUnlocked = Needs(ProductionZone.id)
        }, "shelf_bread"));

        // Tomato Farm: extra plants
        for (int i = 0; i < 6; i++)
        {
            list.Add(new UnlockDef
            {
                id = "plant_tomato_" + i, title = "TOMATO PLANT", cost = 200 + 60 * i,
                position = new Vector3(14f + 6f * (i % 3), 0f, i < 3 ? 6f : -6f),
                kind = UnlockKind.Crop, crop = TomatoPlant, requiresUnlocked = Needs(FarmTomatoZone.id)
            });
        }

        // Chicken Area: each chicken needs the Egg Shelf
        for (int i = 0; i < 8; i++)
        {
            list.Add(Sourced(new UnlockDef
            {
                id = "chicken_" + i, title = "BUY CHICKEN", cost = 150 + 30 * i,
                position = new Vector3(34f + 4f * (i % 4), 0f, i < 4 ? 6.5f : -4f),
                kind = UnlockKind.Machine, building = ChickenBuilding,
                requiresUnlocked = Needs(FarmChickenZone.id)
            }, "shelf_egg"));
        }

        // Wheat Farm: one field for now (the 4 x 4 plot grid is the next step)
        list.Add(new UnlockDef
        {
            id = "crop_wheat", title = "WHEAT FIELD", cost = 150,
            position = new Vector3(60f, 0f, 0f), kind = UnlockKind.Crop, crop = WheatField,
            requiresUnlocked = Needs(FarmWheatZone.id)
        });

        // Cow Area: each cow needs the Milk Fridge
        for (int i = 0; i < 6; i++)
        {
            list.Add(Sourced(new UnlockDef
            {
                id = "cow_" + i, title = "BUY COW", cost = 300 + 60 * i,
                position = new Vector3(74f + 6f * (i % 3), 0f, i < 3 ? 5f : -5.5f),
                kind = UnlockKind.Machine, building = CowBuilding,
                requiresUnlocked = Needs(FarmCowZone.id)
            }, "fridge_milk"));
        }

        // Staff
        list.Add(new UnlockDef
        {
            id = "hire_cashier_1", title = "HIRE CASHIER", cost = 120,
            position = new Vector3(-8f, 0f, -8.5f), kind = UnlockKind.Staff, staff = CashierStaff
        });
        list.Add(new UnlockDef
        {
            id = "hire_shelver_1", title = "HIRE SHELVER", cost = 150,
            position = new Vector3(-4.5f, 0f, -8.5f), kind = UnlockKind.Staff, staff = ShelverStaff
        });
        list.Add(new UnlockDef
        {
            id = "hire_baker_1", title = "HIRE BAKER", cost = 250,
            position = new Vector3(6f, 0f, 12.5f), kind = UnlockKind.Staff, staff = BakerStaff,
            requiresUnlocked = Needs(ProductionZone.id)
        });
        list.Add(new UnlockDef
        {
            id = "hire_shelver_2", title = "HIRE SHELVER 2", cost = 400,
            position = new Vector3(-6f, 0f, 12.5f), kind = UnlockKind.Staff, staff = ShelverStaff,
            requiresUnlocked = Needs(ProductionZone.id, "hire_shelver_1")
        });
        list.Add(new UnlockDef
        {
            id = "hire_baker_2", title = "HIRE BAKER 2", cost = 600,
            position = new Vector3(0f, 0f, 12.5f), kind = UnlockKind.Staff, staff = BakerStaff,
            requiresUnlocked = Needs("hire_baker_1")
        });

        return list.ToArray();
    }

    // --- Shelves ---
    public const int ShelfColumns = 3;
    public const int ShelfRows = 2;   // shelf capacity = columns x rows

    // --- Customers ---
    public static readonly Vector3 CustomerEntrance = new Vector3(0f, 0f, -11.5f);  // spawn and exit point (on the path)
    public static readonly Vector3 CustomerDoor = new Vector3(0f, 0f, -7f);         // just inside the mart
    public const float CustomerSpeed = 3.5f;
    public const float CustomerPatience = 30f;         // seconds of waiting before getting angry
    public const float CustomerFirstSpawnDelay = 20f;  // time to stock the shelf before customer #1
    public const float CustomerSpawnInterval = 8f;
    public const int CustomerMaxActive = 3;
    public const int CustomerMaxProducts = 2;          // different products per shopping list
    public const int CustomerMinQuantity = 1;
    public const int CustomerMaxQuantity = 3;
    public const float CustomerTakeInterval = 0.35f;   // seconds per item taken from a shelf
    public const float AngryRevenueMultiplier = 0.5f;  // customers who lost patience pay this fraction (1 = no penalty)

    // --- Checkout ---
    public const int CheckoutMaxQueue = 4;
    public const float CheckoutPackTime = 1.5f;        // seconds the player spends packing an order

    // --- Saving ---
    public const float AutosaveInterval = 15f;
    public const float SaveMinInterval = 2f;
    public const bool OfflineProduction = true;        // do plants and machines keep working while the game is closed?

    public static Product FindProduct(string id)
    {
        foreach (Product product in AllProducts)
            if (product.id == id) return product;
        return null;
    }

    public static CropDef FindCrop(string productId)
    {
        foreach (CropDef crop in AllCrops)
            if (crop.product.id == productId) return crop;
        return null;
    }

    public static BuildingDef FindBuilding(string id)
    {
        foreach (BuildingDef building in AllBuildings)
            if (building.id == id) return building;
        return null;
    }

    public static UpgradeDef FindUpgrade(string id)
    {
        foreach (UpgradeDef upgrade in Upgrades)
            if (upgrade.id == id) return upgrade;
        return null;
    }

    public static StaffDef FindStaff(string id)
    {
        foreach (StaffDef staff in AllStaff)
            if (staff.id == id) return staff;
        return null;
    }
}