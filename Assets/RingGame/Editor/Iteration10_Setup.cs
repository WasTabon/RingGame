using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class Iteration10_Setup : EditorWindow
{
    private Transform bgParent;

    [MenuItem("RingGame/Iteration 10/Add FloatingBG to Scene")]
    public static void Open()
    {
        GetWindow<Iteration10_Setup>("Iteration 10 — FloatingBG");
    }

    private void OnGUI()
    {
        GUILayout.Label("Add FloatingBG", EditorStyles.boldLabel);
        GUILayout.Space(8);

        bgParent = (Transform)EditorGUILayout.ObjectField(
            "Parent (Canvas or Panel)", bgParent, typeof(Transform), true);

        if (bgParent == null)
            EditorGUILayout.HelpBox("Drag any Canvas or Panel from Hierarchy", MessageType.Warning);

        GUI.enabled = bgParent != null;

        GUILayout.Space(8);
        if (GUILayout.Button("Add FloatingBG", GUILayout.Height(40)))
            AddFloatingBG();

        GUI.enabled = true;
    }

    private void AddFloatingBG()
    {
        var existing = Object.FindObjectOfType<FloatingBG>(true);
        if (existing != null)
        {
            Debug.Log("[Iter10] FloatingBG already exists: " + existing.gameObject.name);
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        var bgGO = new GameObject("FloatingBG");
        bgGO.transform.SetParent(bgParent, false);
        bgGO.transform.SetAsFirstSibling();

        var rt = bgGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        bgGO.AddComponent<FloatingBG>();

        Undo.RegisterCreatedObjectUndo(bgGO, "Add FloatingBG");
        EditorSceneManager.MarkSceneDirty(bgGO.scene);
        Selection.activeGameObject = bgGO;

        Debug.Log("[Iter10] FloatingBG added under " + bgParent.name + ". Save scene.");
        EditorUtility.DisplayDialog("Done",
            "FloatingBG added under " + bgParent.name + ".\n\n" +
            "Do this in BOTH MainMenu and Game scenes.\n" +
            "Save scene (Ctrl+S).", "OK");
    }

    [MenuItem("RingGame/Iteration 10/Validate Visual Polish")]
    public static void Validate()
    {
        Debug.Log("[Iter10 Validate] ══════════════════════════════");

        var bg = Object.FindObjectOfType<FloatingBG>(true);
        if (bg == null)
            Debug.LogWarning("[Iter10] FloatingBG not found in scene.");
        else
            Debug.Log("[Iter10] ✓ FloatingBG found");

        var stm = Object.FindObjectOfType<SceneTransitionManager>(true);
        if (stm == null)
            Debug.LogWarning("[Iter10] SceneTransitionManager not found.");
        else
            Debug.Log("[Iter10] ✓ SceneTransitionManager found");

        Debug.Log("[Iter10 Validate] ══════════════════════════════");
    }
}
