using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class agentFollow : MonoBehaviour
{
    [HideInInspector] 
    public WaveSpawner spawner; 

    [Header("Path Settings")]
    public string pathTag = "Path";
    
    [Header("Movement Settings")]
    public float speed = 2f;
    public float waypointThreshold = 0.5f;

    private List<Vector3> waypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool hasNotifiedSpawner = false;

    private void Start() 
    {
        // Path setup will be done by ConfigureAndStart
    }
    
    public void ConfigureAndStart(float assignedSpeed, Transform assignedTarget = null)
    {
        Debug.Log($"[agentFollow] ConfigureAndStart called with speed: {assignedSpeed}");
        
        speed = assignedSpeed;
        
        // Find all waypoints with Waypoint component
        Waypoint[] waypointComponents = FindObjectsOfType<Waypoint>();
        
        Debug.Log($"[agentFollow] Found {waypointComponents.Length} waypoints in scene");
        
        if (waypointComponents.Length == 0)
        {
            Debug.LogError($"[agentFollow] No Waypoint components found in scene! Make sure you have GameObjects with the Waypoint script attached. Destroying agent.");
            NotifySpawnerAndDestroy();
            return;
        }
        
        // Sort waypoints by their ID
        waypoints = waypointComponents
            .OrderBy(wp => wp.waypointID)
            .Select(wp => wp.transform.position)
            .ToList();
        
        // Add the final target if provided
        if (assignedTarget != null)
        {
            waypoints.Add(assignedTarget.position);
            Debug.Log($"[agentFollow] Added final target at {assignedTarget.position}");
        }
        
        Debug.Log($"[agentFollow] Path created with {waypoints.Count} waypoints. Speed: {speed}. Starting movement!");
        
        // Start at first waypoint
        currentWaypointIndex = 0;
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving || waypoints.Count == 0) return;
        
        // Get current target waypoint
        Vector3 targetWaypoint = waypoints[currentWaypointIndex];
        
        // Move towards waypoint
        Vector3 direction = (targetWaypoint - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // Rotate to face direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        
        // Check if reached waypoint
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint);
        if (distanceToWaypoint < waypointThreshold)
        {
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= waypoints.Count)
            {
                // Reached the end
                Debug.Log("[agentFollow] Agent reached the end of path!");
                NotifySpawnerAndDestroy();
            }
        }
    }
    
    // Gère la notification du Spawner et la destruction de l'objet
    private void NotifySpawnerAndDestroy()
    {
        if (!hasNotifiedSpawner && spawner != null)
        {
            spawner.DecrementActiveEnemyCount(); 
            hasNotifiedSpawner = true;
            Debug.Log($"[agentFollow] Enemy destroyed. Remaining enemies: {spawner.activeEnemyCount}");
        }
        else if (spawner == null)
        {
            Debug.LogWarning("[agentFollow] Spawner reference is null, cannot decrement count!");
        }
        
        Destroy(gameObject);
    }

    // Gère la destruction par une source externe (ex: tour)
    private void OnDestroy()
    {
        if (Application.isPlaying) 
        {
            if (!hasNotifiedSpawner && spawner != null)
            {
                spawner.DecrementActiveEnemyCount();
                hasNotifiedSpawner = true;
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (waypoints != null && waypoints.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
                Gizmos.DrawSphere(waypoints[i], 0.2f);
            }
            Gizmos.DrawSphere(waypoints[waypoints.Count - 1], 0.2f);
            
            if (currentWaypointIndex < waypoints.Count)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(waypoints[currentWaypointIndex], 0.3f);
            }
        }
    }
}