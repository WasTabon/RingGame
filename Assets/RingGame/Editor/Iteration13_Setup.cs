using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Iteration13_Setup : EditorWindow
{
    private Transform canvasParent;
    private Transform topBarParent;

    [MenuItem("RingGame/Iteration 13/Setup Game Tutorial")]
    public static void Open()
    {
        GetWindow<Iteration13_Setup>("Iteration 13 \u2014 Game Tutorial");
    }

    private void OnGUI()
    {
        GUILayout.Label("Game Tutorial (Iteration 13)", EditorStyles.boldLabel);
        GUILayout.Space(4);
        GUILayout.Label("Creates GameTutorialUI panel + ? button in TopBar.", EditorStyles.miniLabel);
        GUILayout.Label("Run on Game scene.", EditorStyles.miniLabel);
        GUILayout.Space(8);

        canvasParent = (Transform)EditorGUILayout.ObjectField(
            "Canvas Parent (RhythmPhaseCanvas)", canvasParent, typeof(Transform), true);

        topBarParent = (Transform)EditorGUILayout.ObjectField(
            "TopBar Parent (TopBar in RhythmPhaseCanvas)", topBarParent, typeof(Transform), true);

        if (canvasParent == null)
            EditorGUILayout.HelpBox("Drag RhythmPhaseCanvas from Hierarchy", MessageType.Warning);
        if (topBarParent == null)
            EditorGUILayout.HelpBox("Drag TopBar from Hierarchy (child of RhythmPhaseCanvas)", MessageType.Warning);

        GUI.enabled = canvasParent != null && topBarParent != null;

        GUILayout.Space(8);
        if (GUILayout.Button("Setup Game Tutorial", GUILayout.Height(40)))
            DoSetup();

        GUI.enabled = true;

        GUILayout.Space(16);
        GUILayout.Label("Utilities", EditorStyles.boldLabel);
        if (GUILayout.Button("Validate", GUILayout.Height(30)))
            Validate();

        GUILayout.Space(4);
        if (GUILayout.Button("Reset Game Tutorial Flag (Debug)", GUILayout.Height(25)))
        {
            PlayerPrefs.SetInt("game_tutorial_done", 0);
            PlayerPrefs.Save();
            Debug.Log("[Iter13] Game tutorial flag reset.");
        }
    }

    private void DoSetup()
    {
        CreateGameTutorialPanel();
        CreateHelpButton();
        WireReferences();

        EditorSceneManager.MarkSceneDirty(canvasParent.gameObject.scene);

        Debug.Log("[Iter13] Setup complete. Save scene (Ctrl+S).");
        EditorUtility.DisplayDialog("Done",
            "Game Tutorial created.\n\n" +
            "\u2022 GameTutorialUI panel in " + canvasParent.name + "\n" +
            "\u2022 Help (?) button in " + topBarParent.name + "\n" +
            "\u2022 RhythmPhaseController + RhythmPhaseUI wired\n\n" +
            "Save scene (Ctrl+S).", "OK");
    }

    private void CreateGameTutorialPanel()
    {
        var existing = Object.FindObjectOfType<GameTutorialUI>(true);
        if (existing != null)
        {
            Debug.Log("[Iter13] GameTutorialUI already exists on: " + existing.gameObject.name);
            return;
        }

        var symbolConfig = AssetDatabase.LoadAssetAtPath<SymbolConfig>("Assets/RingGame/Data/SymbolConfig.asset");
        if (symbolConfig == null)
        {
            Debug.LogError("[Iter13] SymbolConfig not found at Assets/RingGame/Data/SymbolConfig.asset!");
            return;
        }

        var rootGO = new GameObject("GameTutorialPanel");
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
        dimImg.color = new Color(0f, 0f, 0f, 0.8f);
        dimImg.raycastTarget = true;
        var dimCG = dimGO.AddComponent<CanvasGroup>();

        var cardGO = new GameObject("Card");
        cardGO.transform.SetParent(rootGO.transform, false);
        var cardRT = cardGO.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.06f, 0.12f);
        cardRT.anchorMax = new Vector2(0.94f, 0.88f);
        cardRT.offsetMin = Vector2.zero;
        cardRT.offsetMax = Vector2.zero;
        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.06f, 0.05f, 0.15f, 0.95f);
        cardImg.raycastTarget = true;
        var cardCG = cardGO.AddComponent<CanvasGroup>();

        var titleGO = CreateChild(cardGO, "Title", new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f));
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "HOW TO PLAY";
        titleTMP.fontSize = 26f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.82f, 0.15f);
        titleTMP.raycastTarget = false;

        var mapHeaderGO = CreateChild(cardGO, "MapHeader", new Vector2(0.05f, 0.8f), new Vector2(0.95f, 0.88f));
        var mapHeaderTMP = mapHeaderGO.AddComponent<TextMeshProUGUI>();
        mapHeaderTMP.text = "SWIPE DIRECTIONS";
        mapHeaderTMP.fontSize = 16f;
        mapHeaderTMP.fontStyle = FontStyles.Bold;
        mapHeaderTMP.alignment = TextAlignmentOptions.Center;
        mapHeaderTMP.color = new Color(0.7f, 0.7f, 0.9f);
        mapHeaderTMP.raycastTarget = false;

        var suitIcons = new Image[4];
        var dirLabels = new TextMeshProUGUI[4];

        float rowTop = 0.79f;
        float rowHeight = 0.09f;

        for (int i = 0; i < 4; i++)
        {
            float top = rowTop - i * rowHeight;
            float bottom = top - rowHeight + 0.015f;

            var rowGO = CreateChild(cardGO, "SuitRow_" + i, new Vector2(0.08f, bottom), new Vector2(0.92f, top));
            var rowImg = rowGO.AddComponent<Image>();
            rowImg.color = new Color(0.1f, 0.09f, 0.2f, 0.7f);
            rowImg.raycastTarget = false;

            var iconGO = CreateChild(rowGO, "SuitIcon_" + i, new Vector2(0.05f, 0.1f), new Vector2(0.2f, 0.9f));
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            iconImg.color = Color.white;
            suitIcons[i] = iconImg;

            var labelGO = CreateChild(rowGO, "DirLabel_" + i, new Vector2(0.4f, 0.05f), new Vector2(0.95f, 0.95f));
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "\u2191 UP";
            labelTMP.fontSize = 28f;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.alignment = TextAlignmentOptions.Center;
            labelTMP.color = Color.white;
            labelTMP.raycastTarget = false;
            dirLabels[i] = labelTMP;
        }

        var rulesGO = CreateChild(cardGO, "RulesText", new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.42f));
        var rulesTMP = rulesGO.AddComponent<TextMeshProUGUI>();
        rulesTMP.text = "";
        rulesTMP.fontSize = 15f;
        rulesTMP.alignment = TextAlignmentOptions.TopLeft;
        rulesTMP.color = new Color(0.8f, 0.8f, 0.92f);
        rulesTMP.enableWordWrapping = true;
        rulesTMP.raycastTarget = false;

        var gotItBtnGO = CreateChild(cardGO, "GotItButton", new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.14f));
        var gotItBtnImg = gotItBtnGO.AddComponent<Image>();
        gotItBtnImg.color = new Color(0.3f, 0.85f, 0.5f);
        var gotItBtn = gotItBtnGO.AddComponent<Button>();
        var gotItBtnRT = gotItBtnGO.GetComponent<RectTransform>();

        var gotItTextGO = CreateChild(gotItBtnGO, "Text", new Vector2(0f, 0f), new Vector2(1f, 1f));
        var gotItTMP = gotItTextGO.AddComponent<TextMeshProUGUI>();
        gotItTMP.text = "GOT IT!";
        gotItTMP.fontSize = 24f;
        gotItTMP.fontStyle = FontStyles.Bold;
        gotItTMP.alignment = TextAlignmentOptions.Center;
        gotItTMP.color = new Color(0.02f, 0.08f, 0.04f);
        gotItTMP.raycastTarget = false;

        var tutUI = rootGO.AddComponent<GameTutorialUI>();
        var so = new SerializedObject(tutUI);

        so.FindProperty("panelGroup").objectReferenceValue = cardCG;
        so.FindProperty("panelRect").objectReferenceValue = cardRT;
        so.FindProperty("dimOverlay").objectReferenceValue = dimCG;
        so.FindProperty("gotItButton").objectReferenceValue = gotItBtn;
        so.FindProperty("gotItButtonRect").objectReferenceValue = gotItBtnRT;
        so.FindProperty("symbolConfig").objectReferenceValue = symbolConfig;
        so.FindProperty("rulesText").objectReferenceValue = rulesTMP;

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
        Undo.RegisterCreatedObjectUndo(rootGO, "Create GameTutorialUI");
        Debug.Log("[Iter13] GameTutorialUI panel created inside " + canvasParent.name);
    }

    private void CreateHelpButton()
    {
        var existingHelp = FindChildByName(topBarParent, "HelpButton");
        if (existingHelp != null)
        {
            Debug.Log("[Iter13] HelpButton already exists in TopBar.");
            return;
        }

        var helpGO = new GameObject("HelpButton");
        helpGO.transform.SetParent(topBarParent, false);
        var helpRT = helpGO.AddComponent<RectTransform>();
        helpRT.anchorMin = new Vector2(0f, 0.5f);
        helpRT.anchorMax = new Vector2(0f, 0.5f);
        helpRT.pivot = new Vector2(0f, 0.5f);
        helpRT.anchoredPosition = new Vector2(170f, 0f);
        helpRT.sizeDelta = new Vector2(36f, 36f);

        var helpImg = helpGO.AddComponent<Image>();
        helpImg.color = new Color(0.25f, 0.23f, 0.45f);

        var helpBtn = helpGO.AddComponent<Button>();

        var helpTextGO = new GameObject("Text");
        helpTextGO.transform.SetParent(helpGO.transform, false);
        var helpTextRT = helpTextGO.AddComponent<RectTransform>();
        helpTextRT.anchorMin = Vector2.zero;
        helpTextRT.anchorMax = Vector2.one;
        helpTextRT.offsetMin = Vector2.zero;
        helpTextRT.offsetMax = Vector2.zero;
        var helpTMP = helpTextGO.AddComponent<TextMeshProUGUI>();
        helpTMP.text = "?";
        helpTMP.fontSize = 22f;
        helpTMP.fontStyle = FontStyles.Bold;
        helpTMP.alignment = TextAlignmentOptions.Center;
        helpTMP.color = new Color(0.8f, 0.8f, 1f);
        helpTMP.raycastTarget = false;

        Undo.RegisterCreatedObjectUndo(helpGO, "Create HelpButton");
        Debug.Log("[Iter13] HelpButton (?) created in " + topBarParent.name);
    }

    private void WireReferences()
    {
        var ctrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (ctrl != null)
        {
            var so = new SerializedObject(ctrl);

            var gameTutUI = Object.FindObjectOfType<GameTutorialUI>(true);
            if (gameTutUI != null)
            {
                so.FindProperty("gameTutorialUI").objectReferenceValue = gameTutUI;
                Debug.Log("[Iter13] RhythmPhaseController.gameTutorialUI wired.");
            }
            else
            {
                Debug.LogError("[Iter13] GameTutorialUI not found! Cannot wire to RhythmPhaseController.");
            }

            var swipeMapUI = Object.FindObjectOfType<SwipeMapUI>(true);
            if (swipeMapUI != null)
            {
                var prop = so.FindProperty("swipeMapUI");
                if (prop.objectReferenceValue == null)
                {
                    prop.objectReferenceValue = swipeMapUI;
                    Debug.Log("[Iter13] RhythmPhaseController.swipeMapUI re-wired.");
                }
            }
            else
            {
                Debug.LogError("[Iter13] SwipeMapUI not found! Run Iteration 12 Setup first.");
            }

            var rhythmUI = Object.FindObjectOfType<RhythmPhaseUI>(true);
            if (rhythmUI != null)
            {
                var prop = so.FindProperty("rhythmPhaseUI");
                if (prop.objectReferenceValue == null)
                {
                    prop.objectReferenceValue = rhythmUI;
                    Debug.Log("[Iter13] RhythmPhaseController.rhythmPhaseUI re-wired.");
                }
            }
            else
            {
                Debug.LogError("[Iter13] RhythmPhaseUI not found!");
            }

            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogError("[Iter13] RhythmPhaseController not found in scene!");
        }

        var phaseUI = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (phaseUI != null)
        {
            var so = new SerializedObject(phaseUI);
            var helpBtn = FindChildByName(topBarParent, "HelpButton");
            if (helpBtn != null)
            {
                var btn = helpBtn.GetComponent<Button>();
                if (btn != null)
                {
                    so.FindProperty("helpButton").objectReferenceValue = btn;
                    so.ApplyModifiedProperties();
                    Debug.Log("[Iter13] RhythmPhaseUI.helpButton wired.");
                }
                else
                {
                    Debug.LogError("[Iter13] HelpButton has no Button component!");
                }
            }
            else
            {
                Debug.LogError("[Iter13] HelpButton not found in TopBar!");
            }
        }
        else
        {
            Debug.LogError("[Iter13] RhythmPhaseUI not found in scene!");
        }
    }

    [MenuItem("RingGame/Iteration 13/Validate")]
    public static void Validate()
    {
        Debug.Log("[Iter13 Validate] \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550");
        int issues = 0;

        var gameTut = Object.FindObjectOfType<GameTutorialUI>(true);
        if (gameTut != null)
        {
            Debug.Log("[Iter13] \u2713 GameTutorialUI found on: " + gameTut.gameObject.name);
            var so = new SerializedObject(gameTut);
            CheckProp(so, "panelGroup", ref issues);
            CheckProp(so, "panelRect", ref issues);
            CheckProp(so, "dimOverlay", ref issues);
            CheckProp(so, "gotItButton", ref issues);
            CheckProp(so, "symbolConfig", ref issues);
            CheckProp(so, "rulesText", ref issues);
            CheckArrayProp(so, "suitIcons", 4, ref issues);
            CheckArrayProp(so, "directionLabels", 4, ref issues);
        }
        else
        {
            Debug.LogError("[Iter13] GameTutorialUI NOT found! Run Setup.");
            issues++;
        }

        var ctrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (ctrl != null)
        {
            var so = new SerializedObject(ctrl);
            if (so.FindProperty("gameTutorialUI").objectReferenceValue != null)
                Debug.Log("[Iter13] \u2713 RhythmPhaseController.gameTutorialUI wired");
            else
            {
                Debug.LogError("[Iter13] RhythmPhaseController.gameTutorialUI is null!");
                issues++;
            }
            if (so.FindProperty("swipeMapUI").objectReferenceValue != null)
                Debug.Log("[Iter13] \u2713 RhythmPhaseController.swipeMapUI wired");
            else
            {
                Debug.LogError("[Iter13] RhythmPhaseController.swipeMapUI is null!");
                issues++;
            }
        }
        else
        {
            Debug.LogError("[Iter13] RhythmPhaseController NOT found!");
            issues++;
        }

        var phaseUI = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (phaseUI != null)
        {
            var so = new SerializedObject(phaseUI);
            var helpProp = so.FindProperty("helpButton");
            if (helpProp != null && helpProp.objectReferenceValue != null)
                Debug.Log("[Iter13] \u2713 RhythmPhaseUI.helpButton wired");
            else
            {
                Debug.LogError("[Iter13] RhythmPhaseUI.helpButton is null!");
                issues++;
            }
        }
        else
        {
            Debug.LogError("[Iter13] RhythmPhaseUI NOT found!");
            issues++;
        }

        var settings = Object.FindObjectOfType<SettingsManager>(true);
        if (settings != null)
            Debug.Log("[Iter13] \u2713 SettingsManager found (GameTutorialDone=" + settings.GameTutorialDone + ")");
        else
        {
            Debug.LogError("[Iter13] SettingsManager NOT found!");
            issues++;
        }

        if (issues == 0)
            Debug.Log("[Iter13 Validate] \u2705 All good!");
        else
            Debug.LogError("[Iter13 Validate] " + issues + " issue(s) found.");

        Debug.Log("[Iter13 Validate] \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550");
    }

    private static void CheckProp(SerializedObject so, string name, ref int issues)
    {
        if (so.FindProperty(name) != null && so.FindProperty(name).objectReferenceValue != null)
            Debug.Log("[Iter13]   \u2713 " + name);
        else
        {
            Debug.LogError("[Iter13]   " + name + " is null!");
            issues++;
        }
    }

    private static void CheckArrayProp(SerializedObject so, string name, int expected, ref int issues)
    {
        var prop = so.FindProperty(name);
        if (prop == null || prop.arraySize != expected)
        {
            Debug.LogError("[Iter13]   " + name + " missing or wrong size!");
            issues++;
            return;
        }
        for (int i = 0; i < expected; i++)
        {
            if (prop.GetArrayElementAtIndex(i).objectReferenceValue == null)
            {
                Debug.LogError("[Iter13]   " + name + "[" + i + "] is null!");
                issues++;
                return;
            }
        }
        Debug.Log("[Iter13]   \u2713 " + name + " (" + expected + " elements)");
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

    private static Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindChildByName(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
