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
    public bool debugPlacement = true;   // Show debug visualization

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
            // CRITICAL FIX: Force placement at ground level (Y=0) to prevent placing on viruses
            Vector3 placePos = new Vector3(hit.point.x, 0f, hit.point.z);
            
            // Ensure a tower is selected
            var prefab = TowerPlacementController.Instance != null ? TowerPlacementController.Instance.SelectedTowerPrefab : null;
            if (prefab == null)
            {
                Debug.Log("No tower selected to place.");
                return;
            }

            // Get the tower's actual collider size from prefab
            Collider prefabCollider = prefab.GetComponent<Collider>();
            float checkRadius = placeableRadius;
            
            // If tower has a collider, use its actual size for more accurate checking
            if (prefabCollider != null)
            {
                if (prefabCollider is SphereCollider sphere)
                {
                    checkRadius = Mathf.Max(sphere.radius * prefab.transform.localScale.x, placeableRadius);
                }
                else if (prefabCollider is BoxCollider box)
                {
                    // Use the largest dimension of the box
                    float maxSize = Mathf.Max(box.size.x * prefab.transform.localScale.x, 
                                             box.size.z * prefab.transform.localScale.z) / 2f;
                    checkRadius = Mathf.Max(maxSize, placeableRadius);
                }
                else if (prefabCollider is CapsuleCollider capsule)
                {
                    checkRadius = Mathf.Max(capsule.radius * prefab.transform.localScale.x, placeableRadius);
                }
            }

            // Check from ground up to catch towers and viruses at different heights
            // Use a tall cylinder-like check to catch towers with colliders high up
            Vector3 checkCenter = placePos + Vector3.up * 2f; // Check 2 units above ground (center of tall check)
            
            // IMPROVED: Check ALL colliders in the placement area with larger radius
            Collider[] allColliders = Physics.OverlapSphere(checkCenter, checkRadius * 2f); // Double radius for safety
            
            if (allColliders.Length > 0)
            {
                Debug.Log($"[TowerPlacement] Found {allColliders.Length} colliders at placement position (radius: {checkRadius})");
            }
            
            foreach (var col in allColliders)
            {
                // Skip if it's the ground itself
                if (col.gameObject == hit.collider.gameObject) continue;
                
                // Block if it's a path
                if (col.CompareTag("Path"))
                {
                    Debug.Log("❌ Cannot place here — on the path!");
                    return;
                }
                
                // Block if there's already a tower
                if (col.CompareTag("Tower"))
                {
                    Debug.Log("❌ Cannot place here — tower already exists!");
                    return;
                }
                
                // Block if there's an enemy/virus
                if (col.CompareTag("Enemy"))
                {
                    Debug.Log("❌ Cannot place here — virus in the way!");
                    return;
                }
                
                // Also check by GameObject name or component as fallback
                if (col.gameObject.GetComponent<agentFollow>() != null)
                {
                    Debug.Log("❌ Cannot place here — virus detected (by script)!");
                    return;
                }
                
                // Check if there's a tower component
                if (col.gameObject.GetComponent<TowerInfo>() != null)
                {
                    Debug.Log("❌ Cannot place here — tower detected (by component)!");
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
            
            // Ensure ALL colliders are enabled and NOT triggers
            Collider[] newTowerColliders = newTower.GetComponentsInChildren<Collider>();
            bool hasCollider = false;
            foreach (var col in newTowerColliders)
            {
                col.enabled = true;
                col.isTrigger = false; // Force solid collider
                hasCollider = true;
                Debug.Log($"[TowerPlacement] Tower collider: {col.GetType().Name}, enabled={col.enabled}, isTrigger={col.isTrigger}");
            }
            
            if (!hasCollider)
            {
                Debug.LogError($"⚠️ Tower prefab '{prefab.name}' has NO COLLIDER! Add a collider to prevent stacking.");
            }

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
    
    // Visualize the placement check radius
    private void OnDrawGizmos()
    {
        if (!debugPlacement) return;
        
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;
        
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 checkCenter = hit.point + Vector3.up * 2f;
            float checkRadius = placeableRadius * 2f;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(checkCenter, checkRadius);
            
            // Draw a vertical line to show the check goes upward
            Gizmos.color = Color.red;
            Gizmos.DrawLine(hit.point, checkCenter);
            
            // Draw sphere at ground level too
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(hit.point, checkRadius);
        }
    }
}
