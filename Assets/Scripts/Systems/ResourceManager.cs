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
    [SerializeField] private int maxBalance = 9999;
    [SerializeField] private int maxHealthPoints = 200;

    [Header("Cheat/Debug Settings")]
    [Tooltip("Enable to ignore all balance/HP deductions and maintain infinite resources.")]
    [SerializeField] private bool infiniteResourcesAndHP = false;

    public int Balance { get; private set; }
    public int HealthPoints { get; private set; }

    // Notifies listeners of the new balance after any change
    public event Action<int> OnBalanceChanged;
    public event Action<int> OnHealthPointsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddHealthPoints(int amount)
    {
        if (amount <= 0) return;
        if (infiniteResourcesAndHP)
        {
            HealthPoints = maxHealthPoints; // keep at max
        }
        else
        {
            HealthPoints = Mathf.Clamp(HealthPoints + amount, 0, maxHealthPoints);
        }
        OnHealthPointsChanged?.Invoke(HealthPoints);
    }

    public void RemoveHealthPoints(int amount)
    {
        if (infiniteResourcesAndHP) return; // ignore damage
        if (amount <= 0) return;
        HealthPoints = Mathf.Clamp(HealthPoints - amount, 0, maxHealthPoints);
        OnHealthPointsChanged?.Invoke(HealthPoints);
    }

    public void AddBalance(int amount)
    {
        if (amount <= 0) return;
        if (infiniteResourcesAndHP)
        {
            Balance = maxBalance; // keep at max
        }
        else
        {
            Balance = Mathf.Clamp(Balance + amount, 0, maxBalance);
        }
        OnBalanceChanged?.Invoke(Balance);
    }

    // Tries to remove 'amount' from balance. Returns true if successful.
    public bool TrySpend(int amount)
    {
        if (infiniteResourcesAndHP) return true; // always succeed, no deduction
        if (amount <= 0) return true; // nothing to spend
        if (amount > Balance) return false;
        Balance -= amount;
        OnBalanceChanged?.Invoke(Balance);
        return true;
    }

    public void SetBalance(int amount)
    {
        if (infiniteResourcesAndHP)
        {
            Balance = maxBalance; // override to max
        }
        else
        {
            Balance = Mathf.Clamp(amount, 0, maxBalance);
        }
        OnBalanceChanged?.Invoke(Balance);
    }

    public void SetHealthPoints(int amount)
    {
        if (infiniteResourcesAndHP)
        {
            HealthPoints = maxHealthPoints; // override to max
        }
        else
        {
            HealthPoints = Mathf.Clamp(amount, 0, maxHealthPoints);
        }
        OnHealthPointsChanged?.Invoke(HealthPoints);
    }

    public int GetMaxHealthPoints()
    {
        return maxHealthPoints;
    }

    // Editor/Debug helper to add funds from inspector context menu
        [ContextMenu("Add 100")] private void DebugAdd100() { AddBalance(100); }
    }
}
