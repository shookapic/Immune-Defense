using GameSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;

public class PlayScreenUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI healthPointsText;
    [SerializeField] private Slider healthBar;

    void Start()
    {
        int initialBalance = ResourceManager.Instance.Balance;
        HandleBalanceChanged(initialBalance);
        int initialHealthPoints = ResourceManager.Instance.HealthPoints;
        HandleHealthPointsChanged(initialHealthPoints);
    }

    void OnEnable()
    {
        ResourceManager.Instance.OnBalanceChanged += HandleBalanceChanged;
        ResourceManager.Instance.OnHealthPointsChanged += HandleHealthPointsChanged;
    }

    void OnDisable()
    {
        ResourceManager.Instance.OnBalanceChanged -= HandleBalanceChanged;
        ResourceManager.Instance.OnHealthPointsChanged -= HandleHealthPointsChanged;
    }

    void HandleBalanceChanged(int newBalance)
    {
        // Update balance UI here
        balanceText.text = "E" + newBalance.ToString();
    }

    void HandleHealthPointsChanged(int newHealthPoints)
    {
        int maxHealthPoints = ResourceManager.Instance.GetMaxHealthPoints();

        // Update health points UI here
        healthPointsText.text = $"{newHealthPoints}/{maxHealthPoints}";
        healthBar.value = Mathf.Clamp((float)newHealthPoints / maxHealthPoints, 0, 1);
    }
}
