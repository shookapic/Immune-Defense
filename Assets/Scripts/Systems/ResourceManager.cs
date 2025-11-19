using System;
using UnityEngine;

namespace GameSystems
{
    // Simple player resource manager (singleton). Keeps an integer balance and
    // provides methods to add/spend resources with an event to notify UI.
    public class ResourceManager : MonoBehaviour
    {
    public static ResourceManager Instance { get; private set; }

    [Header("Starting values")]
    [SerializeField] private int startingBalance = 200;
    [SerializeField] private int maxBalance = 9999;

    public int Balance { get; private set; }

    // Notifies listeners of the new balance after any change
    public event Action<int> OnBalanceChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Balance = Mathf.Clamp(startingBalance, 0, maxBalance);
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Balance = Mathf.Clamp(Balance + amount, 0, maxBalance);
        OnBalanceChanged?.Invoke(Balance);
    }

    // Tries to remove 'amount' from balance. Returns true if successful.
    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true; // nothing to spend
        if (amount > Balance) return false;
        Balance -= amount;
        OnBalanceChanged?.Invoke(Balance);
        return true;
    }

    public void SetBalance(int amount)
    {
        Balance = Mathf.Clamp(amount, 0, maxBalance);
        OnBalanceChanged?.Invoke(Balance);
    }

    // Editor/Debug helper to add funds from inspector context menu
        [ContextMenu("Add 100")] private void DebugAdd100() { Add(100); }
    }
}
