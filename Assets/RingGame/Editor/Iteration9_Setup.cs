using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class Iteration9_Setup : EditorWindow
{
    [MenuItem("RingGame/Iteration 9/Setup VFX Manager")]
    public static void SetupVFXManager()
    {
        var existing = Object.FindObjectOfType<VFXManager>(true);
        if (existing != null)
        {
            Debug.Log("[Iter9] VFXManager already exists on: " + existing.gameObject.name);
            Selection.activeGameObject = existing.gameObject;
            EditorUtility.DisplayDialog("Already Exists",
                "VFXManager already exists in scene.\nSelected it in hierarchy.", "OK");
            return;
        }

        var go = new GameObject("VFXManager");
        go.AddComponent<VFXManager>();
        Undo.RegisterCreatedObjectUndo(go, "Create VFXManager");

        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeGameObject = go;

        Debug.Log("[Iter9] VFXManager created. It auto-creates its own VFX Canvas at runtime. Save scene (Ctrl+S).");
        EditorUtility.DisplayDialog("Done",
            "VFXManager created in scene root.\n\n" +
            "At runtime it auto-creates:\n" +
            "• VFXCanvas (ScreenSpace-Overlay, sortOrder=200)\n" +
            "• Particle pool (80 UI Image objects)\n" +
            "• Procedural glow/circle textures\n\n" +
            "Place in Game scene (not MainMenu).\n" +
            "Save scene (Ctrl+S).", "OK");
    }

    [MenuItem("RingGame/Iteration 9/Validate VFX")]
    public static void Validate()
    {
        Debug.Log("[Iter9 Validate] ══════════════════════════════");

        var vfx = Object.FindObjectOfType<VFXManager>(true);
        if (vfx == null)
        {
            Debug.LogWarning("[Iter9] VFXManager not found. Run Setup VFX Manager in Game scene.");
            return;
        }
        Debug.Log("[Iter9] ✓ VFXManager found on: " + vfx.gameObject.name);

        if (vfx.transform.parent != null)
            Debug.LogWarning("[Iter9] VFXManager should be at scene root, not parented.");
        else
            Debug.Log("[Iter9] ✓ VFXManager at scene root");

        Debug.Log("[Iter9 Validate] ✅ Done.");
        Debug.Log("[Iter9 Validate] ══════════════════════════════");
    }
}
