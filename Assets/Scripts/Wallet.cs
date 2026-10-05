using UnityEngine;

public static class Wallet
{
    public static int Money { get; private set; } = GameConfig.StartingMoney;

    // Used when loading a saved game
    public static void Set(int amount)
    {
        Money = Mathf.Max(0, amount);
    }

    public static void Add(int amount)
    {
        Money += amount;
        SaveManager.RequestSave();
    }

    public static bool TrySpend(int amount)
    {
        if (amount > Money) return false;
        Money -= amount;
        SaveManager.RequestSave();
        return true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        Money = GameConfig.StartingMoney;
    }
}