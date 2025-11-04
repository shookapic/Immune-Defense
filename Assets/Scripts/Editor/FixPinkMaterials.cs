using UnityEditor;
using UnityEngine;

public class FixPinkMaterials : EditorWindow
{
    [MenuItem("Tools/Fix Pink Materials")]
    public static void FixPink()
    {
        string[] matGUIDs = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0;

        foreach (string guid in matGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null) continue;

            // Detect invalid or unsupported shaders
            if (mat.shader == null ||
                mat.shader.name.Contains("Universal") ||
                mat.shader.name.Contains("HD") ||
                mat.shader.name.Contains("Shader Graph"))
            {
                mat.shader = Shader.Find("Standard");
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Fixed {fixedCount} materials!");
    }
}
