using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class Iteration7_Setup : EditorWindow
{
    private RhythmPhaseUI rhythmPhaseUI;
    private Transform gridParent;

    [MenuItem("RingGame/Iteration 7/Setup Symbol Grid")]
    public static void Open()
    {
        GetWindow<Iteration7_Setup>("Iteration 7 — Symbol Grid");
    }

    private void OnGUI()
    {
        GUILayout.Label("Symbol Grid Setup (4×4 = 16 cells)", EditorStyles.boldLabel);
        GUILayout.Space(8);

        rhythmPhaseUI = (RhythmPhaseUI)EditorGUILayout.ObjectField(
            "RhythmPhaseUI", rhythmPhaseUI, typeof(RhythmPhaseUI), true);

        gridParent = (Transform)EditorGUILayout.ObjectField(
            "Grid Parent (GridArea)", gridParent, typeof(Transform), true);

        GUILayout.Space(4);

        if (rhythmPhaseUI == null)
            EditorGUILayout.HelpBox("Перетащи RhythmPhaseUI компонент из Inspector", MessageType.Warning);
        if (gridParent == null)
            EditorGUILayout.HelpBox("Перетащи GridArea из Hierarchy (RhythmPhaseCanvas → GridArea)", MessageType.Warning);

        GUI.enabled = rhythmPhaseUI != null && gridParent != null;

        GUILayout.Space(8);
        if (GUILayout.Button("Setup Symbol Grid", GUILayout.Height(40)))
            Setup();

        GUI.enabled = true;

        GUILayout.Space(16);
        GUILayout.Label("Utilities", EditorStyles.boldLabel);

        if (GUILayout.Button("Validate & Fix", GUILayout.Height(30)))
            ValidateAndFix();
    }

    private void Setup()
    {
        Undo.RecordObject(gridParent.gameObject, "Iter7 Grid Setup");

        var toDelete = new List<GameObject>();
        foreach (Transform child in gridParent)
            toDelete.Add(child.gameObject);
        foreach (var go in toDelete)
            DestroyImmediate(go);

        var existingGLG = gridParent.GetComponent<GridLayoutGroup>();
        if (existingGLG != null)
            DestroyImmediate(existingGLG);
        var existingImage = gridParent.GetComponent<Image>();
        if (existingImage != null)
            existingImage.color = new Color(0.08f, 0.07f, 0.18f, 0.9f);

        var cellsGO = new GameObject("GridCells");
        cellsGO.transform.SetParent(gridParent, false);
        var cellsRT = cellsGO.AddComponent<RectTransform>();
        cellsRT.anchorMin = new Vector2(0.5f, 0.5f);
        cellsRT.anchorMax = new Vector2(0.5f, 0.5f);
        cellsRT.pivot = new Vector2(0.5f, 0.5f);
        cellsRT.anchoredPosition = Vector2.zero;
        cellsRT.sizeDelta = new Vector2(180f, 180f);

        var glg = cellsGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(38f, 38f);
        glg.spacing = new Vector2(4f, 4f);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 4;
        glg.childAlignment = TextAnchor.MiddleCenter;
        glg.padding = new RectOffset(2, 2, 2, 2);

        var symbolBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/RingGame/Textures/SymbolBg.png");

        var bgs = new Image[16];
        var icons = new Image[16];

        for (int i = 0; i < 16; i++)
        {
            var cellGO = new GameObject($"Cell_{i}");
            cellGO.transform.SetParent(cellsGO.transform, false);
            cellGO.AddComponent<RectTransform>();

            var bgImg = cellGO.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.13f, 0.28f, 0.85f);
            bgImg.raycastTarget = false;
            if (symbolBgSprite != null) bgImg.sprite = symbolBgSprite;
            bgs[i] = bgImg;

            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(cellGO.transform, false);
            var iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.12f, 0.12f);
            iconRT.anchorMax = new Vector2(0.88f, 0.88f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            iconImg.color = new Color(1f, 1f, 1f, 0f);
            icons[i] = iconImg;
        }

        var so = new SerializedObject(rhythmPhaseUI);

        var bgsProp = so.FindProperty("gridCellBgs");
        bgsProp.arraySize = 16;
        for (int i = 0; i < 16; i++)
            bgsProp.GetArrayElementAtIndex(i).objectReferenceValue = bgs[i];

        var iconsProp = so.FindProperty("gridCellIcons");
        iconsProp.arraySize = 16;
        for (int i = 0; i < 16; i++)
            iconsProp.GetArrayElementAtIndex(i).objectReferenceValue = icons[i];

        var cfg = AssetDatabase.LoadAssetAtPath<SymbolConfig>("Assets/RingGame/Data/SymbolConfig.asset");
        if (cfg != null)
            so.FindProperty("gridSymbolConfig").objectReferenceValue = cfg;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(rhythmPhaseUI);
        EditorSceneManager.MarkSceneDirty(rhythmPhaseUI.gameObject.scene);

        Debug.Log("[Iter7] Symbol Grid created: 16 cells (4×4) with icons. Save scene (Ctrl+S).");
        EditorUtility.DisplayDialog("Done",
            "Symbol Grid created.\n16 cells (4×4) wired to RhythmPhaseUI.\n\nSave scene (Ctrl+S).", "OK");
    }

    [MenuItem("RingGame/Iteration 7/Validate & Fix")]
    public static void ValidateAndFix()
    {
        Debug.Log("[Iter7 Validate] ══════════════════════════════");
        int fixed_ = 0;

        var ui = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (ui == null)
        {
            Debug.LogWarning("[Iter7] ⚠️ RhythmPhaseUI not found.");
            return;
        }

        var so = new SerializedObject(ui);

        bool hasBgs = PropArrayValid(so, "gridCellBgs", 16);
        bool hasIcons = PropArrayValid(so, "gridCellIcons", 16);

        if (!hasBgs || !hasIcons)
        {
            Debug.LogWarning("[Iter7] ⚠️ gridCellBgs or gridCellIcons not fully assigned — run Setup Symbol Grid.");
        }
        else
        {
            Debug.Log("[Iter7] ✓ gridCellBgs (16 elements)");
            Debug.Log("[Iter7] ✓ gridCellIcons (16 elements)");
        }

        if (!PropReal<SymbolConfig>(so, "gridSymbolConfig"))
        {
            var cfg = AssetDatabase.LoadAssetAtPath<SymbolConfig>("Assets/RingGame/Data/SymbolConfig.asset");
            if (cfg != null)
            {
                so.FindProperty("gridSymbolConfig").objectReferenceValue = cfg;
                so.ApplyModifiedProperties();
                Debug.Log("[Iter7] 🔧 gridSymbolConfig assigned.");
                fixed_++;
            }
            else
            {
                Debug.LogWarning("[Iter7] ⚠️ gridSymbolConfig is null and SymbolConfig.asset not found.");
            }
        }
        else
        {
            Debug.Log("[Iter7] ✓ gridSymbolConfig");
        }

        var ctrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (ctrl != null)
        {
            var cso = new SerializedObject(ctrl);
            if (!PropReal<RhythmPhaseUI>(cso, "rhythmPhaseUI"))
            {
                cso.FindProperty("rhythmPhaseUI").objectReferenceValue = ui;
                cso.ApplyModifiedProperties();
                Debug.Log("[Iter7] 🔧 RhythmPhaseController.rhythmPhaseUI reassigned.");
                fixed_++;
            }
            else
            {
                Debug.Log("[Iter7] ✓ RhythmPhaseController.rhythmPhaseUI");
            }
        }

        var gridArea = so.FindProperty("gridArea");
        if (gridArea.objectReferenceValue != null)
        {
            var gridRT = gridArea.objectReferenceValue as RectTransform;
            if (gridRT != null)
            {
                var images = gridRT.GetComponentsInChildren<Image>(true);
                int blockers = 0;
                foreach (var img in images)
                {
                    if (!img.raycastTarget) continue;
                    if (img.GetComponent<Button>() != null) continue;
                    if (img.GetComponentInParent<Button>() != null) continue;
                    img.raycastTarget = false;
                    blockers++;
                }
                if (blockers > 0)
                {
                    Debug.Log($"[Iter7] 🔧 Disabled raycastTarget on {blockers} grid Image(s).");
                    fixed_++;
                }
                else
                {
                    Debug.Log("[Iter7] ✓ No raycast blockers in grid");
                }
            }
        }

        if (fixed_ > 0)
        {
            EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
            Debug.Log($"[Iter7 Validate] Fixed {fixed_} issue(s). Save scene.");
        }
        else
        {
            Debug.Log("[Iter7 Validate] ✅ No issues found.");
        }

        Debug.Log("[Iter7 Validate] ══════════════════════════════");
    }

    static bool PropReal<T>(SerializedObject so, string name) where T : Object
    {
        var obj = so.FindProperty(name).objectReferenceValue as T;
        if (obj == null) return false;
        if (obj is Component c) return c.gameObject != null;
        return true;
    }

    static bool PropArrayValid(SerializedObject so, string name, int expectedSize)
    {
        var prop = so.FindProperty(name);
        if (prop == null) return false;
        if (prop.arraySize != expectedSize) return false;
        for (int i = 0; i < expectedSize; i++)
        {
            if (prop.GetArrayElementAtIndex(i).objectReferenceValue == null)
                return false;
        }
        return true;
    }
}
