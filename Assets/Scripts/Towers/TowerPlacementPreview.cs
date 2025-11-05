using UnityEngine;

public class TowerPlacementPreview : MonoBehaviour
{
    [Header("References")]
    public GameObject towerPrefab;
    public Material validMat;
    public Material invalidMat;

    [Header("Settings")]
    public float placeableRadius = 1.5f;
    public LayerMask blockedLayers;

    private Camera cam;
    private GameObject previewInstance;
    private Renderer[] previewRenderers;
    private bool canPlace = false;

    void Start()
    {
        cam = Camera.main;
        CreatePreview();
    }

    void Update()
    {
        UpdatePreviewPosition();

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            PlaceTower();
        }
    }

    void CreatePreview()
    {
        previewInstance = Instantiate(towerPrefab);
        previewRenderers = previewInstance.GetComponentsInChildren<Renderer>();

        // Use transparent material for preview
        foreach (var r in previewRenderers)
        {
            r.material = validMat;
        }

        // Disable any gameplay scripts/colliders inside the preview
        foreach (var col in previewInstance.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    void UpdatePreviewPosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 placePos = hit.point;
            previewInstance.transform.position = placePos;

            // Check overlap
            Collider[] overlaps = Physics.OverlapSphere(placePos, placeableRadius, blockedLayers);
            bool blocked = false;
            foreach (var col in overlaps)
            {
                Debug.Log($"Overlapping: {col.name} (Tag: {col.tag})");
                if (col.CompareTag("Path") || col.CompareTag("Tower"))
                {
                    blocked = true;
                    break;
                }
            }

            canPlace = !blocked;
            foreach (var r in previewRenderers)
                r.material = canPlace ? validMat : invalidMat;
        }
    }

    void PlaceTower()
    {
        Instantiate(towerPrefab, previewInstance.transform.position, Quaternion.identity);
    }
}
