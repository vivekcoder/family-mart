using System.Collections.Generic;
using UnityEngine;

public class ShoppingLine
{
    public readonly Product product;
    public readonly int quantity;
    public int collected;
    public bool abandoned;   // true if the product turned out to be unavailable

    public ShoppingLine(Product product, int quantity)
    {
        this.product = product;
        this.quantity = quantity;
    }

    public bool IsDone => abandoned || collected >= quantity;
}

public class ShoppingList
{
    public readonly List<ShoppingLine> lines = new List<ShoppingLine>();

    public int TotalRequested
    {
        get
        {
            int n = 0;
            foreach (ShoppingLine line in lines) n += line.quantity;
            return n;
        }
    }

    public int TotalCollected
    {
        get
        {
            int n = 0;
            foreach (ShoppingLine line in lines) n += line.collected;
            return n;
        }
    }

    // Combined sell price of everything the customer actually picked up
    public int CollectedValue
    {
        get
        {
            int value = 0;
            foreach (ShoppingLine line in lines) value += line.product.sellPrice * line.collected;
            return value;
        }
    }

    public ShoppingLine NextIncomplete()
    {
        foreach (ShoppingLine line in lines)
            if (!line.IsDone) return line;
        return null;
    }

    // Builds a random list using ONLY the products in 'available'
    public static ShoppingList Generate(List<Product> available)
    {
        var list = new ShoppingList();
        if (available.Count == 0) return list;

        int maxLines = Mathf.Min(GameConfig.CustomerMaxProducts, available.Count);
        int lineCount = Random.Range(1, maxLines + 1);

        var pool = new List<Product>(available);
        for (int i = 0; i < lineCount; i++)
        {
            int pick = Random.Range(0, pool.Count);
            int quantity = Random.Range(GameConfig.CustomerMinQuantity, GameConfig.CustomerMaxQuantity + 1);
            list.lines.Add(new ShoppingLine(pool[pick], quantity));
            pool.RemoveAt(pick);
        }
        return list;
    }
}