using UnityEngine;
using UnityEngine.EventSystems;
public class TowerPlacementPreview : MonoBehaviour
{
    [Header("References")]
    // The preview will use the currently selected prefab from TowerPlacementController.
    public Material validMat;
    public Material invalidMat;

    [Header("Settings")]
    public float placeableRadius = 1.5f;
    public LayerMask blockedLayers;

    private Camera cam;
    private GameObject previewInstance;
    private Renderer[] previewRenderers;
    private bool canPlace = false;
    private GameObject lastSelectedPrefab;

    void Start()
    {
        cam = Camera.main;
        // create preview only if a prefab is selected
        if (TowerPlacementController.Instance != null)
        {
            TowerPlacementController.Instance.OnSelectionChanged += OnSelectionChanged;
            lastSelectedPrefab = TowerPlacementController.Instance.SelectedTowerPrefab;
            if (lastSelectedPrefab != null)
                CreatePreview(lastSelectedPrefab);
        }
    }

    void Update()
    {
        // Ensure preview follows current selection
        var selected = TowerPlacementController.Instance != null ? TowerPlacementController.Instance.SelectedTowerPrefab : null;
        if (selected != lastSelectedPrefab)
        {
            // recreate or destroy preview depending on selection
            if (previewInstance != null)
            {
                Destroy(previewInstance);
                previewInstance = null;
                previewRenderers = null;
            }

            lastSelectedPrefab = selected;
            if (lastSelectedPrefab != null)
                CreatePreview(lastSelectedPrefab);
        }

        UpdatePreviewPosition();

        if (Input.GetMouseButtonDown(0) && canPlace && lastSelectedPrefab != null)
        {
            // If the pointer is over UI (for example a selection button), do not place a tower.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            PlaceTower();
        }
    }

    void OnSelectionChanged(GameObject prefab)
    {
        // Handled in Update loop to safely create/destroy preview at runtime
    }

    void CreatePreview(GameObject prefab)
    {
        previewInstance = Instantiate(prefab);
        previewRenderers = previewInstance.GetComponentsInChildren<Renderer>();

        // Use transparent material for preview
        foreach (var r in previewRenderers)
        {
            r.material = validMat;
        }

        // Disable any gameplay scripts/colliders inside the preview
        foreach (var col in previewInstance.GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Disable any runtime behaviours (MonoBehaviours, Animator, AudioSource, NavMeshAgent, etc.)
        // so the preview doesn't run AI/attack logic.
        foreach (var b in previewInstance.GetComponentsInChildren<Behaviour>())
        {
            b.enabled = false;
        }
    }

    void UpdatePreviewPosition()
    {
        if (previewInstance == null)
        {
            canPlace = false;
            return;
        }
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
                if (col.CompareTag("Path") || col.CompareTag("Tower") || col.CompareTag("Enemy"))
                {
                    blocked = true;
                    break;
                }
            }

            canPlace = !blocked;
            foreach (var r in previewRenderers)
                r.material = canPlace ? validMat : invalidMat;
        }
        else
        {
            canPlace = false;
        }
    }

    void PlaceTower()
    {
        var prefab = TowerPlacementController.Instance != null ? TowerPlacementController.Instance.SelectedTowerPrefab : null;
        if (prefab == null)
            return;

        Instantiate(prefab, previewInstance.transform.position, Quaternion.identity);
    }
}
