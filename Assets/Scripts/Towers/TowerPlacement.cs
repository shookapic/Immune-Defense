using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlacement : MonoBehaviour
{
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
        if (Input.GetMouseButtonDown(0))
        {
            // Ignore clicks that happen over UI (buttons, panels, etc.) so UI interactions don't place towers.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

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
        }
    }
}
