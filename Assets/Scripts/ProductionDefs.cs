using UnityEngine;

public class RecipeInput
{
    public readonly Product product;
    public readonly int quantity;

    public RecipeInput(Product product, int quantity)
    {
        this.product = product;
        this.quantity = quantity;
    }
}

public class RecipeDef
{
    public readonly string id;
    public readonly RecipeInput[] inputs;
    public readonly Product output;
    public readonly int outputQuantity;
    public readonly float productionTime;

    public RecipeDef(string id, RecipeInput[] inputs, Product output, int outputQuantity, float productionTime)
    {
        this.id = id;
        this.inputs = inputs;
        this.output = output;
        this.outputQuantity = outputQuantity;
        this.productionTime = productionTime;
    }
}

public class BuildingDef
{
    public readonly string id;
    public readonly string name;
    public readonly RecipeDef recipe;
    public readonly int inputBatches;     // how many batches of ingredients it can hold
    public readonly int outputCapacity;   // how many finished items wait on its tray
    public readonly Color color;
    public BuildingLook look;   // which 3D model to use (set with { look = ... })

    public BuildingDef(string id, string name, RecipeDef recipe, int inputBatches, int outputCapacity, Color color)
    {
        this.id = id;
        this.name = name;
        this.recipe = recipe;
        this.inputBatches = inputBatches;
        this.outputCapacity = outputCapacity;
        this.color = color;
    }
}

public enum UnlockKind { Crop, Shelf, Machine, Staff, Zone }

// Something the player can buy with a pad on the floor
public class UnlockDef
{
    public string id;
    public string title;
    public int cost;
    public Vector3 position;
    public UnlockKind kind;

    public Product product;        // for Shelf
    public CropDef crop;           // for Crop
    public BuildingDef building;   // for Machine
    public StaffDef staff;         // for Staff
    public ZoneDef zone;           // for Zone

    public string[] requiresUnlocked;   // other unlock ids that must be bought first
    public Product requiresProduced;    // something must already produce this product
}