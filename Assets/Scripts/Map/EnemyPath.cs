using UnityEngine;
using System.Collections.Generic;

public class EnemyPath : MonoBehaviour
{
    [Header("Path Points (world positions)")]
    public List<Vector3> pathPoints = new List<Vector3>();

    [Header("Movement Settings")]
    public float speed = 2f;

    private int currentTarget = 0;

    void Start()
    {
        // Start at the first point if it exists
        if (pathPoints.Count > 0)
            transform.position = pathPoints[0];
    }

    void Update()
    {
        if (pathPoints.Count == 0) return;

        Vector3 target = pathPoints[currentTarget];
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Check if we reached the current target
        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentTarget++;
            if (currentTarget >= pathPoints.Count)
            {
                // Reached the end, you can destroy or loop
                Destroy(gameObject);
                // Or loop: currentTarget = 0;
            }
        }
    }

    // To visualize the path in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            Gizmos.DrawSphere(pathPoints[i], 0.05f);
        }
    }
}
