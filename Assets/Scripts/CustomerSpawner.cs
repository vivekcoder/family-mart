using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public float interval = GameConfig.CustomerSpawnInterval;
    public int maxCustomers = GameConfig.CustomerMaxActive;

    readonly List<Customer> _customers = new List<Customer>();
    float _timer = GameConfig.CustomerFirstSpawnDelay;

    void Update()
    {
        _customers.RemoveAll(c => c == null);

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = interval;

        if (_customers.Count >= maxCustomers) return;

        // Shopping lists only ever contain products that have a shelf in the mart
        List<Product> available = Shop.GetAvailableProducts();
        if (available.Count == 0) return;

        ShoppingList list = ShoppingList.Generate(available);
        _customers.Add(Customer.Create(list));
    }
}