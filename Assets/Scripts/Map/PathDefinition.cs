using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PathDefinition : MonoBehaviour
{
    [Tooltip("If true, points are read in the order of children under this object.")]
    public bool useChildrenOrder = true;

    [Tooltip("Optional: manually assign points if not using child order.")]
    public List<Transform> points = new List<Transform>();

    public IReadOnlyList<Vector3> GetWorldPoints()
    {
        var result = new List<Vector3>();
        var src = useChildrenOrder ? GetChildrenPoints() : points;
        foreach (var t in src)
        {
            if (t) result.Add(t.position);
        }
        return result;
    }

    List<Transform> GetChildrenPoints()
    {
        var list = new List<Transform>();
        foreach (Transform child in transform)
            list.Add(child);
        return list;
    }

    void OnValidate()
    {
        // Keep points list in sync when toggling modes
        if (useChildrenOrder == false && points.Count == 0)
        {
            foreach (Transform child in transform)
                points.Add(child);
        }
    }

    void OnDrawGizmos()
    {
        var wp = GetWorldPoints();
        Gizmos.color = Color.cyan;
        for (int i = 0; i < wp.Count; i++)
        {
            Gizmos.DrawSphere(wp[i], 0.1f);
            if (i + 1 < wp.Count)
                Gizmos.DrawLine(wp[i], wp[i + 1]);
        }
    }
}
