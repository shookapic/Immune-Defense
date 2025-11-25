using UnityEngine;
using UnityEngine.EventSystems;
using GameSystems;

public class TowerPlacement : MonoBehaviour
{
    // Shared rate limiter so multiple placement scripts (preview + placement) cannot
    // place more than one tower within `placementCooldown` seconds.
    public static float placementCooldown = 0.2f; // seconds
    private static float lastPlacementTime = -999f;

    public static bool CanPlaceNow()
    {
        return Time.time - lastPlacementTime >= placementCooldown;
    }

    public static void RecordPlacement()
    {
        lastPlacementTime = Time.time;
    }

    // Uses the selected prefab from TowerPlacementController
    public float placeableRadius = 1.5f; // How big the placement check is
    public LayerMask blockedLayers;      // What counts as unplaceable (e.g., path, other towers)

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // Ignore clicks that happen over UI (buttons, panels, etc.) so UI interactions don't place towers.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // Prevent very rapid multiple placements
            if (!CanPlaceNow()) return;

            TryPlaceTower();
        }
    }

    void TryPlaceTower()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 placePos = hit.point;
            // Ensure a tower is selected
            var prefab = TowerPlacementController.Instance != null ? TowerPlacementController.Instance.SelectedTowerPrefab : null;
            if (prefab == null)
            {
                Debug.Log("No tower selected to place.");
                return;
            }

            // Check if overlapping with a Path or another Tower
            Collider[] colliders = Physics.OverlapSphere(placePos, placeableRadius, blockedLayers);
            foreach (var col in colliders)
            {
                if (col.CompareTag("Path") || col.CompareTag("Tower") || col.CompareTag("Enemy"))
                {
                    Debug.Log("❌ Cannot place here — path or another tower!");
                    return;
                }
            }

            // Determine cost for this prefab (prefer CardData in repository, fallback to prefab's TowerInfo)
            int cost = 0;
            if (DeckRepository.Instance != null && DeckRepository.Instance.StoredDeck != null)
            {
                foreach (var data in DeckRepository.Instance.StoredDeck)
                {
                    if (data != null && data.towerPrefab == prefab)
                    {
                        cost = data.cost;
                        break;
                    }
                }
            }

            if (cost == 0)
            {
                var prefabInfo = prefab.GetComponent<TowerInfo>();
                if (prefabInfo != null) cost = prefabInfo.cost;
            }

            // Check player resources
            if (ResourceManager.Instance == null)
            {
                Debug.LogWarning("ResourceManager not found in scene. Add a ResourceManager to enable spending.");
                return;
            }

            // If cost is zero or negative, treat it as unknown/misconfigured and prevent placement.
            if (cost <= 0)
            {
                Debug.LogWarning($"Cannot place tower '{prefab.name}': cost not configured (cost={cost}).");
                return;
            }

            if (!ResourceManager.Instance.TrySpend(cost))
            {
                Debug.Log($"Not enough resources to place tower (cost {cost}, balance {ResourceManager.Instance.Balance}).");
                return;
            }

            // ✅ Place tower
            GameObject newTower = Instantiate(prefab, placePos, Quaternion.identity);
            newTower.tag = "Tower";

            // Attach TowerInfo with cost/name if available from repository deck
            if (DeckRepository.Instance != null && DeckRepository.Instance.StoredDeck != null)
            {
                // Find matching CardData by prefab reference
                foreach (var data in DeckRepository.Instance.StoredDeck)
                {
                    if (data != null && data.towerPrefab == prefab)
                    {
                        var info = newTower.GetComponent<TowerInfo>();
                        if (info == null) info = newTower.AddComponent<TowerInfo>();
                        info.towerName = data.towerName;
                        info.cost = data.cost;
                        info.sourceData = data;
                        break;
                    }
                }
            }

            Debug.Log("✅ Tower placed!");

            // record placement time to prevent immediate duplicate placements
            RecordPlacement();
        }
    }
}
