using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    public GameObject towerPrefab;       // Drag your tower prefab here
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
            TryPlaceTower();
        }
    }

    void TryPlaceTower()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 placePos = hit.point;
            
            // Check if overlapping with a Path or another Tower
            Collider[] colliders = Physics.OverlapSphere(placePos, placeableRadius, blockedLayers);
            foreach (var col in colliders)
            {
                if (col.CompareTag("Path") || col.CompareTag("Tower"))
                {
                    Debug.Log("❌ Cannot place here — path or another tower!");
                    return;
                }
            }

            // ✅ Place tower
            GameObject newTower = Instantiate(towerPrefab, placePos, Quaternion.identity);
            newTower.tag = "Tower";
            Debug.Log("✅ Tower placed!");
        }
    }
}
