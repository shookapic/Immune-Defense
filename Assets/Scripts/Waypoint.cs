using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("Waypoint Settings")]
    [Tooltip("Order of this waypoint in the path (0 = start, 1 = second, etc.)")]
    public int waypointID = 0;
    
    private void OnDrawGizmos()
    {
        // Draw the waypoint in the editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        // Draw the ID as a label
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"WP {waypointID}");
#endif
    }
}
