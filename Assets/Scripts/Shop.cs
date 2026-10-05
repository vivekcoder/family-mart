using System.Collections.Generic;
using UnityEngine;

public static class Shop
{
    public static readonly List<Shelf> Shelves = new List<Shelf>();
    public static readonly List<CropPlant> Crops = new List<CropPlant>();
    public static readonly List<ProductionBuilding> Machines = new List<ProductionBuilding>();
    public static readonly List<CheckoutCounter> Counters = new List<CheckoutCounter>();

    static readonly List<Product> _available = new List<Product>();

    // The shelf that sells this product, or null if there isn't one
    public static Shelf FindShelfFor(Product product)
    {
        foreach (Shelf shelf in Shelves)
            if (shelf.product == product) return shelf;
        return null;
    }

    static int _depth;

    // A machine only counts as producing something if all of its ingredients are produced too
    static bool InputsProduced(RecipeDef recipe)
    {
        if (_depth > 8) return false;

        _depth++;
        bool all = true;
        foreach (RecipeInput input in recipe.inputs)
        {
            if (!IsProduced(input.product))
            {
                all = false;
                break;
            }
        }
        _depth--;
        return all;
    }

    // True if a plant or machine in the mart makes this product
    public static bool IsProduced(Product product)
    {
        foreach (CropPlant crop in Crops)
            if (crop.crop.product == product) return true;
        foreach (ProductionBuilding machine in Machines)
            if (machine.def.recipe.output == product && InputsProduced(machine.def.recipe)) return true;
        return false;
    }

    // Products that have a shelf AND something producing them.
    // (The same list object is reused, so don't keep it.)
    public static List<Product> GetAvailableProducts()
    {
        _available.Clear();
        foreach (Shelf shelf in Shelves)
        {
            if (_available.Contains(shelf.product)) continue;
            _available.Add(shelf.product);   // a product is in demand as soon as its shelf exists
        }
        return _available;
    }

        // A counter that doesn't have a cashier yet, or null
    public static CheckoutCounter FindUnstaffedCounter()
    {
        foreach (CheckoutCounter counter in Counters)
            if (!counter.HasCashier) return counter;
        return null;
    }

    // The counter with the shortest queue that still has room, or null if all are full
    public static CheckoutCounter FindBestCounter()
    {
        CheckoutCounter best = null;
        int bestLength = int.MaxValue;
        foreach (CheckoutCounter counter in Counters)
        {
            if (counter.IsFull) continue;
            if (counter.QueueLength < bestLength)
            {
                bestLength = counter.QueueLength;
                best = counter;
            }
        }
        return best;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        Shelves.Clear();
        Crops.Clear();
        Machines.Clear();
        Counters.Clear();
    }
}