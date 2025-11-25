using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameSystems;

/// <summary>
/// Manages the upgrade menu UI for selected towers.
/// Shows tower stats, upgrade options, and handles upgrade button clicks.
/// </summary>
public class TowerUpgradeMenuUI : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject upgradeMenuPanel;
    [SerializeField] private RectTransform upgradeMenuRect; // for animation

    [Header("Tower Info Display")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI currentStatsText;
    [SerializeField] private TextMeshProUGUI currentTierText;

    [Header("Upgrade Info Display")]
    [SerializeField] private TextMeshProUGUI nextTierNameText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI upgradeStatsPreviewText;

    [Header("Buttons")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button closeButton;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.25f;
    [SerializeField] private Vector2 shownAnchoredPos = Vector2.zero;
    private Vector2 hiddenAnchoredPos;
    private bool isAnimating = false;
    private int originalSiblingIndex = -1;

    [Header("Settings")]
    [SerializeField] private bool hideWhenNoSelection = true;

    void Start()
    {
        // Auto-assign RectTransform if not set
        if (upgradeMenuRect == null && upgradeMenuPanel != null)
            upgradeMenuRect = upgradeMenuPanel.GetComponent<RectTransform>();

        // Cache hidden position and sibling index
        if (upgradeMenuRect != null)
            hiddenAnchoredPos = upgradeMenuRect.anchoredPosition;

        if (upgradeMenuPanel != null)
        {
            originalSiblingIndex = upgradeMenuPanel.transform.GetSiblingIndex();
            upgradeMenuPanel.SetActive(false);
        }

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        if (sellButton != null)
            sellButton.onClick.AddListener(OnSellButtonClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);

        // Subscribe to selection events
        if (TowerSelectionManager.Instance != null)
        {
            TowerSelectionManager.Instance.OnTowerSelected += OnTowerSelected;
            TowerSelectionManager.Instance.OnTowerDeselected += OnTowerDeselected;
        }
    }

    void OnDestroy()
    {
        if (TowerSelectionManager.Instance != null)
        {
            TowerSelectionManager.Instance.OnTowerSelected -= OnTowerSelected;
            TowerSelectionManager.Instance.OnTowerDeselected -= OnTowerDeselected;
        }
    }

    private void OnTowerSelected(GameObject tower)
    {
        RefreshUI();
        ShowMenu();
    }

    private void OnTowerDeselected()
    {
        if (hideWhenNoSelection)
            HideMenu();
    }

    private void ShowMenu()
    {
        if (upgradeMenuPanel == null || upgradeMenuRect == null) return;
        if (isAnimating) return;

        upgradeMenuPanel.SetActive(true);
        upgradeMenuPanel.transform.SetAsLastSibling(); // Move to top of hierarchy
        StartCoroutine(SlideMenu(hiddenAnchoredPos, shownAnchoredPos));
    }

    private void HideMenu()
    {
        if (upgradeMenuPanel == null || upgradeMenuRect == null) return;
        if (isAnimating) return;

        StartCoroutine(SlideMenu(shownAnchoredPos, hiddenAnchoredPos, () => {
            if (originalSiblingIndex >= 0)
                upgradeMenuPanel.transform.SetSiblingIndex(originalSiblingIndex);
            upgradeMenuPanel.SetActive(false);
        }));
    }

    private IEnumerator SlideMenu(Vector2 from, Vector2 to, System.Action onComplete = null)
    {
        isAnimating = true;
        float t = 0f;

        while (t < animationDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / animationDuration);
            upgradeMenuRect.anchoredPosition = Vector2.Lerp(from, to, lerp);
            yield return null;
        }

        upgradeMenuRect.anchoredPosition = to;
        isAnimating = false;
        onComplete?.Invoke();
    }

    public void RefreshUI()
    {
        var info = TowerSelectionManager.Instance?.GetSelectedTowerInfo();
        var upgradeProgress = TowerSelectionManager.Instance?.GetSelectedTowerUpgradeProgress();

        if (info == null)
        {
            if (hideWhenNoSelection && upgradeMenuPanel != null)
                upgradeMenuPanel.SetActive(false);
            return;
        }

        // Display tower name
        if (towerNameText != null)
            towerNameText.text = info.towerName;

        // Display current tier
        if (currentTierText != null)
        {
            int tier = upgradeProgress != null ? upgradeProgress.CurrentTierIndex + 1 : 0;
            currentTierText.text = tier == 0 ? "Base Tier" : $"Tier {tier}";
        }

        // Display current stats
        if (currentStatsText != null)
        {
            var tower = info.GetComponent<Tower>();
            if (tower != null)
            {
                currentStatsText.text = $"Range: {tower.range:F1}\nAttack Speed: {tower.attackSpeed:F2}";
                if (upgradeProgress != null && upgradeProgress.accumulatedDamageBonus > 0)
                    currentStatsText.text += $"\nDamage Bonus: +{upgradeProgress.accumulatedDamageBonus:F1}";
            }
        }

        // Check if upgrade available
        bool canUpgrade = info.CanUpgrade();
        
        if (canUpgrade)
        {
            var nextTier = upgradeProgress?.GetNextTier();
            int cost = info.GetNextUpgradeCost();

            if (nextTierNameText != null && nextTier != null)
                nextTierNameText.text = $"Next: {nextTier.tierName}";

            if (upgradeCostText != null)
                upgradeCostText.text = $"Cost: {cost}";

            if (upgradeStatsPreviewText != null && nextTier != null)
            {
                string preview = "";
                if (nextTier.rangeBonus != 0)
                    preview += $"Range: +{nextTier.rangeBonus:F1}\n";
                if (nextTier.attackSpeedMultiplier != 1f)
                    preview += $"Attack Speed: x{nextTier.attackSpeedMultiplier:F2}\n";
                if (nextTier.damageBonus != 0)
                    preview += $"Damage: +{nextTier.damageBonus:F1}\n";
                if (nextTier.overrideBulletPrefab != null)
                    preview += "New Projectile\n";
                if (nextTier.overrideTowerPrefab != null)
                    preview += "New Model\n";
                
                upgradeStatsPreviewText.text = preview.TrimEnd();
            }

            // Enable/disable button based on resources
            if (upgradeButton != null)
            {
                bool hasEnoughMoney = ResourceManager.Instance != null && ResourceManager.Instance.Balance >= cost;
                upgradeButton.interactable = hasEnoughMoney;
            }
        }
        else
        {
            // Max tier reached
            if (nextTierNameText != null)
                nextTierNameText.text = "Max Tier";
            if (upgradeCostText != null)
                upgradeCostText.text = "";
            if (upgradeStatsPreviewText != null)
                upgradeStatsPreviewText.text = "Fully Upgraded";
            if (upgradeButton != null)
                upgradeButton.interactable = false;
        }
    }

    private void OnUpgradeButtonClicked()
    {
        var info = TowerSelectionManager.Instance?.GetSelectedTowerInfo();
        if (info == null || !info.CanUpgrade()) return;

        int cost = info.GetNextUpgradeCost();
        if (ResourceManager.Instance == null || !ResourceManager.Instance.TrySpend(cost))
        {
            Debug.Log("Not enough resources to upgrade tower.");
            return;
        }

        if (info.ApplyUpgrade())
        {
            Debug.Log($"Tower upgraded! New tier: {info.upgradeProgress.CurrentTierIndex + 1}");
            RefreshUI();
        }
    }

    private void OnSellButtonClicked()
    {
        var tower = TowerSelectionManager.Instance?.SelectedTower;
        if (tower == null) return;

        var info = tower.GetComponent<TowerInfo>();
        int refund = info != null ? info.cost / 2 : 0; // 50% refund

        if (ResourceManager.Instance != null && refund > 0)
            ResourceManager.Instance.AddBalance(refund);

        TowerSelectionManager.Instance.DeselectTower();
        Destroy(tower);
    }

    private void OnCloseButtonClicked()
    {
        TowerSelectionManager.Instance?.DeselectTower();
    }
}
