using UnityEngine;
using UnityEngine.EventSystems;
using GameSystems;
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

        if (Input.GetMouseButtonUp(0) && canPlace && lastSelectedPrefab != null)
        {
            // If the pointer is over UI (for example a selection button), do not place a tower.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // Prevent very rapid multiple placements
            if (!TowerPlacement.CanPlaceNow()) return;

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

        if (ResourceManager.Instance == null)
        {
            Debug.LogWarning("ResourceManager not found in scene. Add a ResourceManager to enable spending.");
            return;
        }

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

        var newTower = Instantiate(prefab, previewInstance.transform.position, Quaternion.identity);

        // Attach TowerInfo with cost/name if available from repository deck
        if (DeckRepository.Instance != null && DeckRepository.Instance.StoredDeck != null)
        {
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
        // record placement time to prevent immediate duplicate placements
        TowerPlacement.RecordPlacement();
        }
    }
}
