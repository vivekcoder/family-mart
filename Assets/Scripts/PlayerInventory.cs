using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public Transform stackRoot;
    public int Capacity { get; set; } = GameConfig.StartingCapacity;

    readonly List<StackItem> _items = new List<StackItem>();

    public bool IsFull => _items.Count >= Capacity;
    public bool IsEmpty => _items.Count == 0;

    public bool Has(Product product)
    {
        foreach (StackItem item in _items)
            if (item.product == product) return true;
        return false;
    }
    public int Count => _items.Count;

    void Awake()
    {
        Instance = this;
    }

    public void Add(StackItem item)
    {
        _items.Add(item);
        item.MoveTo(stackRoot, StackPosition(_items.Count - 1));
        SaveManager.RequestSave();
    }

    // Removes the topmost item of the given product (or returns null if none)
    public StackItem RemoveLast(Product product)
    {
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            if (_items[i].product != product) continue;

            StackItem item = _items[i];
            _items.RemoveAt(i);
            Relayout();
            SaveManager.RequestSave();
            return item;
        }
        return null;
    }

    // For saving: fills 'into' with the product id of every carried item, bottom to top
    public void GetProductIds(List<string> into)
    {
        into.Clear();
        foreach (StackItem item in _items) into.Add(item.product.id);
    }

    // For loading: recreates carried items from their product ids
    public void Restore(List<string> productIds)
    {
        foreach (string id in productIds)
        {
            if (IsFull) break;

            Product product = GameConfig.FindProduct(id);
            if (product == null) continue;

            Add(StackItem.Create(product, stackRoot.position));
        }
    }

    // Closes any gap left behind when an item is removed from the middle
    void Relayout()
    {
        for (int i = 0; i < _items.Count; i++)
            _items[i].MoveTo(stackRoot, StackPosition(i));
    }

    static Vector3 StackPosition(int index)
    {
        return new Vector3(0f, index * 0.32f, 0f);
    }
}