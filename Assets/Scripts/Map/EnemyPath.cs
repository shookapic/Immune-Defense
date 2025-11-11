using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyPath : MonoBehaviour
{
    [Header("Auto Path Settings")]
    public string pathTag = "Path";

    [Header("Height / Surface")]
    [Tooltip("How high above the path objects the enemy should travel.")]
    public float hoverHeight = 0.6f;

    [Tooltip("If true, raycast down from above each path point to find the surface and then add hoverHeight.")]
    public bool useRaycastToSurface = true;

    [Header("Movement")]
    public float speed = 2f;

    private List<Vector3> pathPoints = new List<Vector3>();
    private int currentTarget = 0;

    void Start()
    {
        GeneratePath();
        if (pathPoints.Count > 0)
        {
            transform.position = pathPoints[0];
            currentTarget = Mathf.Min(1, pathPoints.Count - 1);
        }
    }

    void Update()
    {
        if (pathPoints.Count == 0) return;

        var target = pathPoints[currentTarget];
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentTarget++;
            if (currentTarget >= pathPoints.Count)
            {
                Destroy(gameObject);
            }
        }
    }

    public void GeneratePath()
    {
        var gos = GameObject.FindGameObjectsWithTag(pathTag);
        pathPoints.Clear();

        if (gos.Length == 0)
        {
            Debug.LogWarning($"[EnemyPath] No objects found with tag '{pathTag}'.");
            return;
        }

        // Sort left → right (use what matches your layout)
        var sorted = gos.OrderBy(g => g.transform.position.x).ToList();

        foreach (var go in sorted)
        {
            var p = go.transform.position;

            if (useRaycastToSurface)
            {
                // Cast from above to hit the collider accurately even if the mesh Y varies
                var from = p + Vector3.up * 50f;
                if (Physics.Raycast(from, Vector3.down, out RaycastHit hit, 100f))
                {
                    p = hit.point;
                }
                // else fallback to the transform position
            }

            p.y += hoverHeight; // <-- the important bit (applies to EVERY path point)
            pathPoints.Add(p);
        }

        Debug.Log($"[EnemyPath] Generated path with {pathPoints.Count} points (hover {hoverHeight}, raycast {useRaycastToSurface}).");
    }

    void OnDrawGizmosSelected()
    {
        if (pathPoints == null || pathPoints.Count < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Gizmos.DrawSphere(pathPoints[i], 0.12f);
            Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
        }
    }
}
