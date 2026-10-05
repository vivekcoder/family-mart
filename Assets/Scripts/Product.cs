using UnityEngine;

public enum ProductShape { Ball, Bottle, Box, Egg, Carton }

public enum ShelfLook { Wooden, Fridge }

// A thing that can be grown, made, carried, shelved and sold
public class Product
{
    public readonly string id;
    public readonly string name;
    public readonly Color color;
    public readonly int sellPrice;
    public readonly ProductShape shape;
    public ShelfLook shelfLook;   // set with { shelfLook = ... }

    public Product(string id, string name, Color color, int sellPrice, ProductShape shape)
    {
        this.id = id;
        this.name = name;
        this.color = color;
        this.sellPrice = sellPrice;
        this.shape = shape;
    }
}