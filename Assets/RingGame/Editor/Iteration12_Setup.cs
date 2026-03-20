using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Iteration12_Setup : EditorWindow
{
    private Transform canvasParent;

    [MenuItem("RingGame/Iteration 12/Setup Swipe Direction System")]
    public static void Open()
    {
        GetWindow<Iteration12_Setup>("Iteration 12 \u2014 Swipe Directions");
    }

    private void OnGUI()
    {
        GUILayout.Label("Swipe Direction System (Iteration 12)", EditorStyles.boldLabel);
        GUILayout.Space(4);
        GUILayout.Label("Creates SwipeDirectionMap + SwipeMapUI panel.", EditorStyles.miniLabel);
        GUILayout.Label("Run on Game scene with RhythmPhaseCanvas.", EditorStyles.miniLabel);
        GUILayout.Space(8);

        canvasParent = (Transform)EditorGUILayout.ObjectField(
            "Canvas Parent (RhythmPhaseCanvas)", canvasParent, typeof(Transform), true);

        if (canvasParent == null)
            EditorGUILayout.HelpBox("Drag RhythmPhaseCanvas from Hierarchy", MessageType.Warning);

        GUI.enabled = canvasParent != null;

        GUILayout.Space(8);
        if (GUILayout.Button("Setup Swipe Direction System", GUILayout.Height(40)))
            DoSetup();

        GUI.enabled = true;

        GUILayout.Space(16);
        GUILayout.Label("Utilities", EditorStyles.boldLabel);
        if (GUILayout.Button("Validate", GUILayout.Height(30)))
            Validate();
    }

    private void DoSetup()
    {
        CreateSwipeDirectionMap();
        CreateSwipeMapUI();
        WireRhythmPhaseController();

        EditorSceneManager.MarkSceneDirty(canvasParent.gameObject.scene);

        Debug.Log("[Iter12] Setup complete. Save scene (Ctrl+S).");
        EditorUtility.DisplayDialog("Done",
            "Swipe Direction System created.\n\n" +
            "\u2022 SwipeDirectionMap in scene root\n" +
            "\u2022 SwipeMapUI panel in " + canvasParent.name + "\n" +
            "\u2022 RhythmPhaseController wired\n\n" +
            "Save scene (Ctrl+S).", "OK");
    }

    private void CreateSwipeDirectionMap()
    {
        var existing = Object.FindObjectOfType<SwipeDirectionMap>(true);
        if (existing != null)
        {
            Debug.Log("[Iter12] SwipeDirectionMap already exists on: " + existing.gameObject.name);
            return;
        }

        var go = new GameObject("SwipeDirectionMap");
        go.AddComponent<SwipeDirectionMap>();
        Undo.RegisterCreatedObjectUndo(go, "Create SwipeDirectionMap");
        Debug.Log("[Iter12] SwipeDirectionMap created in scene root.");
    }

    private void CreateSwipeMapUI()
    {
        var existing = Object.FindObjectOfType<SwipeMapUI>(true);
        if (existing != null)
        {
            Debug.Log("[Iter12] SwipeMapUI already exists on: " + existing.gameObject.name);
            return;
        }

        var symbolConfig = AssetDatabase.LoadAssetAtPath<SymbolConfig>("Assets/RingGame/Data/SymbolConfig.asset");
        if (symbolConfig == null)
        {
            Debug.LogError("[Iter12] SymbolConfig not found at Assets/RingGame/Data/SymbolConfig.asset! Run Iteration 3 first.");
            return;
        }

        var rootGO = new GameObject("SwipeMapPanel");
        rootGO.transform.SetParent(canvasParent, false);
        rootGO.transform.SetAsLastSibling();
        var rootRT = rootGO.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;

        var dimGO = new GameObject("DimOverlay");
        dimGO.transform.SetParent(rootGO.transform, false);
        var dimRT = dimGO.AddComponent<RectTransform>();
        dimRT.anchorMin = Vector2.zero;
        dimRT.anchorMax = Vector2.one;
        dimRT.offsetMin = Vector2.zero;
        dimRT.offsetMax = Vector2.zero;
        var dimImg = dimGO.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.75f);
        dimImg.raycastTarget = true;
        var dimCG = dimGO.AddComponent<CanvasGroup>();

        var cardGO = new GameObject("Card");
        cardGO.transform.SetParent(rootGO.transform, false);
        var cardRT = cardGO.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.08f, 0.2f);
        cardRT.anchorMax = new Vector2(0.92f, 0.8f);
        cardRT.offsetMin = Vector2.zero;
        cardRT.offsetMax = Vector2.zero;
        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.06f, 0.05f, 0.15f, 0.95f);
        cardImg.raycastTarget = true;
        var cardCG = cardGO.AddComponent<CanvasGroup>();

        var titleGO = CreateChild(cardGO, "Title", new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.95f));
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "SWIPE DIRECTIONS";
        titleTMP.fontSize = 26f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.82f, 0.15f);
        titleTMP.raycastTarget = false;

        var subtitleGO = CreateChild(cardGO, "Subtitle", new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.82f));
        var subtitleTMP = subtitleGO.AddComponent<TextMeshProUGUI>();
        subtitleTMP.text = "Memorize the directions for each suit";
        subtitleTMP.fontSize = 14f;
        subtitleTMP.alignment = TextAlignmentOptions.Center;
        subtitleTMP.color = new Color(0.6f, 0.6f, 0.8f);
        subtitleTMP.raycastTarget = false;

        var suitIcons = new Image[4];
        var dirLabels = new TextMeshProUGUI[4];

        float rowTop = 0.68f;
        float rowHeight = 0.12f;

        for (int i = 0; i < 4; i++)
        {
            float top = rowTop - i * rowHeight;
            float bottom = top - rowHeight + 0.02f;

            var rowGO = CreateChild(cardGO, "SuitRow_" + i, new Vector2(0.08f, bottom), new Vector2(0.92f, top));
            var rowImg = rowGO.AddComponent<Image>();
            rowImg.color = new Color(0.1f, 0.09f, 0.2f, 0.7f);
            rowImg.raycastTarget = false;

            var iconGO = CreateChild(rowGO, "SuitIcon_" + i, new Vector2(0.05f, 0.1f), new Vector2(0.25f, 0.9f));
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            iconImg.color = Color.white;
            suitIcons[i] = iconImg;

            var arrowGO = CreateChild(rowGO, "DirectionLabel_" + i, new Vector2(0.55f, 0.05f), new Vector2(0.95f, 0.95f));
            var arrowTMP = arrowGO.AddComponent<TextMeshProUGUI>();
            arrowTMP.text = "\u2191";
            arrowTMP.fontSize = 48f;
            arrowTMP.fontStyle = FontStyles.Bold;
            arrowTMP.alignment = TextAlignmentOptions.Center;
            arrowTMP.color = Color.white;
            arrowTMP.raycastTarget = false;
            dirLabels[i] = arrowTMP;
        }

        var goBtnGO = CreateChild(cardGO, "GoButton", new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.16f));
        var goBtnImg = goBtnGO.AddComponent<Image>();
        goBtnImg.color = new Color(1f, 0.78f, 0.05f);
        var goBtn = goBtnGO.AddComponent<Button>();
        var goBtnRT = goBtnGO.GetComponent<RectTransform>();

        var goTextGO = CreateChild(goBtnGO, "Text", new Vector2(0f, 0f), new Vector2(1f, 1f));
        var goTMP = goTextGO.AddComponent<TextMeshProUGUI>();
        goTMP.text = "GO!";
        goTMP.fontSize = 28f;
        goTMP.fontStyle = FontStyles.Bold;
        goTMP.alignment = TextAlignmentOptions.Center;
        goTMP.color = new Color(0.08f, 0.05f, 0f);
        goTMP.raycastTarget = false;

        var swipeMapUI = rootGO.AddComponent<SwipeMapUI>();
        var so = new SerializedObject(swipeMapUI);

        so.FindProperty("panelGroup").objectReferenceValue = cardCG;
        so.FindProperty("panelRect").objectReferenceValue = cardRT;
        so.FindProperty("dimOverlay").objectReferenceValue = dimCG;
        so.FindProperty("goButton").objectReferenceValue = goBtn;
        so.FindProperty("goButtonRect").objectReferenceValue = goBtnRT;
        so.FindProperty("symbolConfig").objectReferenceValue = symbolConfig;

        var iconsProp = so.FindProperty("suitIcons");
        iconsProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
            iconsProp.GetArrayElementAtIndex(i).objectReferenceValue = suitIcons[i];

        var labelsProp = so.FindProperty("directionLabels");
        labelsProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
            labelsProp.GetArrayElementAtIndex(i).objectReferenceValue = dirLabels[i];

        so.ApplyModifiedProperties();

        rootGO.SetActive(false);

        Undo.RegisterCreatedObjectUndo(rootGO, "Create SwipeMapUI");
        Debug.Log("[Iter12] SwipeMapUI panel created inside " + canvasParent.name);
    }

    private void WireRhythmPhaseController()
    {
        var ctrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (ctrl != null)
        {
            var so = new SerializedObject(ctrl);
            var swipeMapUI = Object.FindObjectOfType<SwipeMapUI>(true);
            if (swipeMapUI != null)
            {
                so.FindProperty("swipeMapUI").objectReferenceValue = swipeMapUI;
                so.ApplyModifiedProperties();
                Debug.Log("[Iter12] RhythmPhaseController.swipeMapUI wired.");
            }
            else
            {
                Debug.LogError("[Iter12] SwipeMapUI not found! Cannot wire to RhythmPhaseController.");
            }

            var rhythmUI = Object.FindObjectOfType<RhythmPhaseUI>(true);
            if (rhythmUI != null)
            {
                var prop = so.FindProperty("rhythmPhaseUI");
                if (prop.objectReferenceValue == null)
                {
                    prop.objectReferenceValue = rhythmUI;
                    so.ApplyModifiedProperties();
                    Debug.Log("[Iter12] RhythmPhaseController.rhythmPhaseUI re-wired.");
                }
            }
            else
            {
                Debug.LogError("[Iter12] RhythmPhaseUI not found!");
            }
        }
        else
        {
            Debug.LogError("[Iter12] RhythmPhaseController not found in scene!");
        }
    }

    [MenuItem("RingGame/Iteration 12/Validate")]
    public static void Validate()
    {
        Debug.Log("[Iter12 Validate] \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550");
        int issues = 0;

        var sdm = Object.FindObjectOfType<SwipeDirectionMap>(true);
        if (sdm != null)
            Debug.Log("[Iter12] \u2713 SwipeDirectionMap found on: " + sdm.gameObject.name);
        else
        {
            Debug.LogError("[Iter12] SwipeDirectionMap NOT found! Run Setup.");
            issues++;
        }

        var smui = Object.FindObjectOfType<SwipeMapUI>(true);
        if (smui != null)
        {
            Debug.Log("[Iter12] \u2713 SwipeMapUI found on: " + smui.gameObject.name);

            var so = new SerializedObject(smui);
            CheckProp(so, "panelGroup", ref issues);
            CheckProp(so, "panelRect", ref issues);
            CheckProp(so, "dimOverlay", ref issues);
            CheckProp(so, "goButton", ref issues);
            CheckProp(so, "symbolConfig", ref issues);
            CheckArrayProp(so, "suitIcons", 4, ref issues);
            CheckArrayProp(so, "directionLabels", 4, ref issues);
        }
        else
        {
            Debug.LogError("[Iter12] SwipeMapUI NOT found! Run Setup.");
            issues++;
        }

        var ctrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (ctrl != null)
        {
            var so = new SerializedObject(ctrl);
            if (so.FindProperty("swipeMapUI").objectReferenceValue != null)
                Debug.Log("[Iter12] \u2713 RhythmPhaseController.swipeMapUI wired");
            else
            {
                Debug.LogError("[Iter12] RhythmPhaseController.swipeMapUI is null! Run Setup.");
                issues++;
            }
            if (so.FindProperty("rhythmPhaseUI").objectReferenceValue != null)
                Debug.Log("[Iter12] \u2713 RhythmPhaseController.rhythmPhaseUI wired");
            else
            {
                Debug.LogError("[Iter12] RhythmPhaseController.rhythmPhaseUI is null!");
                issues++;
            }
        }
        else
        {
            Debug.LogError("[Iter12] RhythmPhaseController NOT found!");
            issues++;
        }

        var tih = Object.FindObjectOfType<TapInputHandler>(true);
        if (tih != null)
            Debug.Log("[Iter12] \u2713 TapInputHandler found");
        else
        {
            Debug.LogError("[Iter12] TapInputHandler NOT found!");
            issues++;
        }

        if (issues == 0)
            Debug.Log("[Iter12 Validate] \u2705 All good!");
        else
            Debug.LogError("[Iter12 Validate] " + issues + " issue(s) found.");

        Debug.Log("[Iter12 Validate] \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550");
    }

    private static void CheckProp(SerializedObject so, string name, ref int issues)
    {
        if (so.FindProperty(name).objectReferenceValue != null)
            Debug.Log("[Iter12]   \u2713 " + name);
        else
        {
            Debug.LogError("[Iter12]   " + name + " is null!");
            issues++;
        }
    }

    private static void CheckArrayProp(SerializedObject so, string name, int expected, ref int issues)
    {
        var prop = so.FindProperty(name);
        if (prop.arraySize != expected)
        {
            Debug.LogError("[Iter12]   " + name + " array size is " + prop.arraySize + ", expected " + expected);
            issues++;
            return;
        }
        for (int i = 0; i < expected; i++)
        {
            if (prop.GetArrayElementAtIndex(i).objectReferenceValue == null)
            {
                Debug.LogError("[Iter12]   " + name + "[" + i + "] is null!");
                issues++;
                return;
            }
        }
        Debug.Log("[Iter12]   \u2713 " + name + " (" + expected + " elements)");
    }

    private static GameObject CreateChild(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }
}
