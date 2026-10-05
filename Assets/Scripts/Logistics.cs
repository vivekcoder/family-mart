using UnityEngine;

// Something workers can take finished items from (a plant's ripe produce, a machine's output tray)
public interface IItemSource
{
    Vector3 WorkerSpot { get; }
    int AvailableCount(Product product);
    StackItem TakeItem(Product product);
}

// Something workers can deliver items to (a shelf, a machine's ingredient hopper)
public interface IItemDestination
{
    Vector3 WorkerSpot { get; }
    Product[] AcceptedProducts { get; }
    int FreeSpace(Product product);      // how many more of this product it can take right now
    float FillRatio(Product product);    // 0 = empty (most urgent) .. 1 = full
    bool Accept(StackItem item);
}