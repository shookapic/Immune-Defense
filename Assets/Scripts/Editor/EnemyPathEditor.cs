using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyPath))]
public class EnemyPathEditor : Editor
{
    private void OnSceneGUI()
    {
        EnemyPath path = (EnemyPath)target;

        // Draw lines between points
        Handles.color = Color.red;
        for (int i = 0; i < path.pathPoints.Count - 1; i++)
        {
            Handles.DrawLine(path.pathPoints[i], path.pathPoints[i + 1]);
        }

        // Draw and move handles for each point
        for (int i = 0; i < path.pathPoints.Count; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(path.pathPoints[i], Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(path, "Move Path Point");
                path.pathPoints[i] = newPos;
                EditorUtility.SetDirty(path);
            }

            // Label points for clarity
            Handles.Label(path.pathPoints[i] + Vector3.up * 0.2f, $"P{i}", EditorStyles.boldLabel);
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemyPath path = (EnemyPath)target;

        if (GUILayout.Button("Add Point"))
        {
            Vector3 newPoint = Vector3.zero;
            if (path.pathPoints.Count > 0)
                newPoint = path.pathPoints[path.pathPoints.Count - 1] + Vector3.right;
            path.pathPoints.Add(newPoint);
        }

        if (GUILayout.Button("Clear Points"))
        {
            path.pathPoints.Clear();
        }
    }
}
