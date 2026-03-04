using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class Iteration11_Setup : EditorWindow
{
    private MainMenuUI mainMenuUI;
    private Transform canvasParent;

    [MenuItem("RingGame/Iteration 11/Setup Tutorial and Settings")]
    public static void Open()
    {
        GetWindow<Iteration11_Setup>("Iteration 11 — Tutorial + Settings");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tutorial + Settings Setup", EditorStyles.boldLabel);
        GUILayout.Space(8);

        mainMenuUI = (MainMenuUI)EditorGUILayout.ObjectField(
            "MainMenuUI", mainMenuUI, typeof(MainMenuUI), true);

        canvasParent = (Transform)EditorGUILayout.ObjectField(
            "Canvas Parent", canvasParent, typeof(Transform), true);

        GUILayout.Space(4);

        if (mainMenuUI == null)
            EditorGUILayout.HelpBox("Drag MainMenuUI component", MessageType.Warning);
        if (canvasParent == null)
            EditorGUILayout.HelpBox("Drag the MainMenu Canvas from Hierarchy", MessageType.Warning);

        GUI.enabled = mainMenuUI != null && canvasParent != null;

        GUILayout.Space(8);
        if (GUILayout.Button("Create Settings Panel", GUILayout.Height(35)))
            CreateSettingsPanel();

        if (GUILayout.Button("Create Tutorial Panel", GUILayout.Height(35)))
            CreateTutorialPanel();

        if (GUILayout.Button("Create SettingsManager (scene root)", GUILayout.Height(35)))
            CreateSettingsManager();

        GUI.enabled = true;

        GUILayout.Space(16);
        GUILayout.Label("Utilities", EditorStyles.boldLabel);
        if (GUILayout.Button("Validate", GUILayout.Height(30)))
            Validate();
    }

    private void CreateSettingsManager()
    {
        var existing = FindObjectOfType<SettingsManager>(true);
        if (existing != null)
        {
            Debug.Log("[Iter11] SettingsManager already exists.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        var go = new GameObject("SettingsManager");
        go.AddComponent<SettingsManager>();
        Undo.RegisterCreatedObjectUndo(go, "Create SettingsManager");
        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeGameObject = go;
        Debug.Log("[Iter11] SettingsManager created. Save scene.");
    }

    private void CreateSettingsPanel()
    {
        var panelGO = new GameObject("SettingsPanel");
        panelGO.transform.SetParent(canvasParent, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        var panelGroup = panelGO.AddComponent<CanvasGroup>();

        var dimGO = CreateChild(panelGO, "Dim", Vector2.zero, Vector2.one);
        var dimImg = dimGO.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.7f);
        dimImg.raycastTarget = true;

        var cardGO = CreateChild(panelGO, "Card", new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.8f));
        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.08f, 0.07f, 0.18f, 0.95f);
        cardImg.raycastTarget = true;

        var titleGO = CreateChild(cardGO, "Title", new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.96f));
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "SETTINGS";
        titleTMP.fontSize = 28f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        titleTMP.raycastTarget = false;

        var musicLabelGO = CreateChild(cardGO, "MusicLabel", new Vector2(0.08f, 0.72f), new Vector2(0.5f, 0.82f));
        var musicLabelTMP = musicLabelGO.AddComponent<TextMeshProUGUI>();
        musicLabelTMP.text = "MUSIC";
        musicLabelTMP.fontSize = 20f;
        musicLabelTMP.alignment = TextAlignmentOptions.MidlineLeft;
        musicLabelTMP.color = new Color(0.7f, 0.7f, 0.85f);
        musicLabelTMP.raycastTarget = false;

        var musicValGO = CreateChild(cardGO, "MusicValue", new Vector2(0.78f, 0.72f), new Vector2(0.92f, 0.82f));
        var musicValTMP = musicValGO.AddComponent<TextMeshProUGUI>();
        musicValTMP.text = "70%";
        musicValTMP.fontSize = 18f;
        musicValTMP.alignment = TextAlignmentOptions.Center;
        musicValTMP.color = Color.white;
        musicValTMP.raycastTarget = false;

        var musicSliderGO = CreateSlider(cardGO, "MusicSlider", new Vector2(0.08f, 0.64f), new Vector2(0.92f, 0.72f));

        var sfxLabelGO = CreateChild(cardGO, "SFXLabel", new Vector2(0.08f, 0.5f), new Vector2(0.5f, 0.6f));
        var sfxLabelTMP = sfxLabelGO.AddComponent<TextMeshProUGUI>();
        sfxLabelTMP.text = "SOUND FX";
        sfxLabelTMP.fontSize = 20f;
        sfxLabelTMP.alignment = TextAlignmentOptions.MidlineLeft;
        sfxLabelTMP.color = new Color(0.7f, 0.7f, 0.85f);
        sfxLabelTMP.raycastTarget = false;

        var sfxValGO = CreateChild(cardGO, "SFXValue", new Vector2(0.78f, 0.5f), new Vector2(0.92f, 0.6f));
        var sfxValTMP = sfxValGO.AddComponent<TextMeshProUGUI>();
        sfxValTMP.text = "70%";
        sfxValTMP.fontSize = 18f;
        sfxValTMP.alignment = TextAlignmentOptions.Center;
        sfxValTMP.color = Color.white;
        sfxValTMP.raycastTarget = false;

        var sfxSliderGO = CreateSlider(cardGO, "SFXSlider", new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.5f));

        var vibLabelGO = CreateChild(cardGO, "VibLabel", new Vector2(0.08f, 0.28f), new Vector2(0.55f, 0.38f));
        var vibLabelTMP = vibLabelGO.AddComponent<TextMeshProUGUI>();
        vibLabelTMP.text = "VIBRATION";
        vibLabelTMP.fontSize = 20f;
        vibLabelTMP.alignment = TextAlignmentOptions.MidlineLeft;
        vibLabelTMP.color = new Color(0.7f, 0.7f, 0.85f);
        vibLabelTMP.raycastTarget = false;

        var vibBtnGO = CreateChild(cardGO, "VibToggle", new Vector2(0.6f, 0.29f), new Vector2(0.92f, 0.37f));
        var vibBtnImg = vibBtnGO.AddComponent<Image>();
        vibBtnImg.color = new Color(0.3f, 0.85f, 0.5f);
        var vibBtn = vibBtnGO.AddComponent<Button>();
        var vibTextGO = CreateChild(vibBtnGO, "VibText", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f));
        var vibTMP = vibTextGO.AddComponent<TextMeshProUGUI>();
        vibTMP.text = "ON";
        vibTMP.fontSize = 18f;
        vibTMP.fontStyle = FontStyles.Bold;
        vibTMP.alignment = TextAlignmentOptions.Center;
        vibTMP.color = Color.white;
        vibTMP.raycastTarget = false;

        var resetBtnGO = CreateChild(cardGO, "ResetTutorial", new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.2f));
        var resetImg = resetBtnGO.AddComponent<Image>();
        resetImg.color = new Color(0.25f, 0.23f, 0.4f);
        var resetBtn = resetBtnGO.AddComponent<Button>();
        var resetTextGO = CreateChild(resetBtnGO, "Text", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f));
        var resetTMP = resetTextGO.AddComponent<TextMeshProUGUI>();
        resetTMP.text = "RESET TUTORIAL";
        resetTMP.fontSize = 16f;
        resetTMP.alignment = TextAlignmentOptions.Center;
        resetTMP.color = new Color(0.7f, 0.7f, 0.85f);
        resetTMP.raycastTarget = false;

        var closeBtnGO = CreateChild(cardGO, "CloseBtn", new Vector2(0.85f, 0.88f), new Vector2(0.97f, 0.97f));
        var closeImg = closeBtnGO.AddComponent<Image>();
        closeImg.color = new Color(0.4f, 0.35f, 0.55f);
        var closeBtn = closeBtnGO.AddComponent<Button>();
        var closeTextGO = CreateChild(closeBtnGO, "X", new Vector2(0f, 0f), new Vector2(1f, 1f));
        var closeTMP = closeTextGO.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "X";
        closeTMP.fontSize = 22f;
        closeTMP.fontStyle = FontStyles.Bold;
        closeTMP.alignment = TextAlignmentOptions.Center;
        closeTMP.color = Color.white;
        closeTMP.raycastTarget = false;

        var settingsUI = panelGO.AddComponent<SettingsUI>();
        var so = new SerializedObject(settingsUI);
        so.FindProperty("panelGroup").objectReferenceValue = panelGroup;
        so.FindProperty("panelRect").objectReferenceValue = panelRT;
        so.FindProperty("musicSlider").objectReferenceValue = musicSliderGO.GetComponent<Slider>();
        so.FindProperty("musicValueText").objectReferenceValue = musicValTMP;
        so.FindProperty("sfxSlider").objectReferenceValue = sfxSliderGO.GetComponent<Slider>();
        so.FindProperty("sfxValueText").objectReferenceValue = sfxValTMP;
        so.FindProperty("vibrationToggle").objectReferenceValue = vibBtn;
        so.FindProperty("vibrationText").objectReferenceValue = vibTMP;
        so.FindProperty("vibrationBg").objectReferenceValue = vibBtnImg;
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("resetTutorialButton").objectReferenceValue = resetBtn;
        so.ApplyModifiedProperties();

        var menuSO = new SerializedObject(mainMenuUI);
        menuSO.FindProperty("settingsUI").objectReferenceValue = settingsUI;
        menuSO.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(panelGO, "Create SettingsPanel");
        EditorSceneManager.MarkSceneDirty(panelGO.scene);
        Debug.Log("[Iter11] SettingsPanel created and wired. Save scene.");
    }

    private void CreateTutorialPanel()
    {
        var panelGO = new GameObject("TutorialPanel");
        panelGO.transform.SetParent(canvasParent, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        var panelGroup = panelGO.AddComponent<CanvasGroup>();

        var dimGO = CreateChild(panelGO, "Dim", Vector2.zero, Vector2.one);
        var dimImg = dimGO.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.8f);
        dimImg.raycastTarget = true;

        var cardGO = CreateChild(panelGO, "Card", new Vector2(0.06f, 0.15f), new Vector2(0.94f, 0.85f));
        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.06f, 0.05f, 0.15f, 0.95f);
        cardImg.raycastTarget = true;

        var titleGO = CreateChild(cardGO, "Title", new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f));
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "Welcome";
        titleTMP.fontSize = 28f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.82f, 0.15f);
        titleTMP.raycastTarget = false;

        var bodyGO = CreateChild(cardGO, "Body", new Vector2(0.06f, 0.2f), new Vector2(0.94f, 0.85f));
        var bodyTMP = bodyGO.AddComponent<TextMeshProUGUI>();
        bodyTMP.text = "";
        bodyTMP.fontSize = 18f;
        bodyTMP.alignment = TextAlignmentOptions.TopLeft;
        bodyTMP.color = new Color(0.85f, 0.85f, 0.92f);
        bodyTMP.enableWordWrapping = true;
        bodyTMP.raycastTarget = false;

        var pageGO = CreateChild(cardGO, "PageIndicator", new Vector2(0.35f, 0.1f), new Vector2(0.65f, 0.16f));
        var pageTMP = pageGO.AddComponent<TextMeshProUGUI>();
        pageTMP.text = "1 / 5";
        pageTMP.fontSize = 14f;
        pageTMP.alignment = TextAlignmentOptions.Center;
        pageTMP.color = new Color(0.5f, 0.5f, 0.65f);
        pageTMP.raycastTarget = false;

        var tapGO = CreateChild(cardGO, "TapHint", new Vector2(0.2f, 0.02f), new Vector2(0.8f, 0.08f));
        var tapTMP = tapGO.AddComponent<TextMeshProUGUI>();
        tapTMP.text = "TAP TO CONTINUE";
        tapTMP.fontSize = 13f;
        tapTMP.alignment = TextAlignmentOptions.Center;
        tapTMP.color = new Color(0.4f, 0.4f, 0.55f);
        tapTMP.raycastTarget = false;

        var nextBtnGO = CreateChild(cardGO, "NextBtn", new Vector2(0.25f, 0.1f), new Vector2(0.75f, 0.18f));
        var nextImg = nextBtnGO.AddComponent<Image>();
        nextImg.color = new Color(0.3f, 0.55f, 1f);
        var nextBtn = nextBtnGO.AddComponent<Button>();
        var nextTextGO = CreateChild(nextBtnGO, "Text", new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f));
        var nextTMP = nextTextGO.AddComponent<TextMeshProUGUI>();
        nextTMP.text = "NEXT";
        nextTMP.fontSize = 18f;
        nextTMP.fontStyle = FontStyles.Bold;
        nextTMP.alignment = TextAlignmentOptions.Center;
        nextTMP.color = Color.white;
        nextTMP.raycastTarget = false;

        var skipBtnGO = CreateChild(cardGO, "SkipBtn", new Vector2(0.8f, 0.88f), new Vector2(0.97f, 0.97f));
        var skipImg = skipBtnGO.AddComponent<Image>();
        skipImg.color = new Color(0.3f, 0.28f, 0.45f);
        var skipBtn = skipBtnGO.AddComponent<Button>();
        var skipTextGO = CreateChild(skipBtnGO, "Text", new Vector2(0f, 0f), new Vector2(1f, 1f));
        var skipTMP = skipTextGO.AddComponent<TextMeshProUGUI>();
        skipTMP.text = "SKIP";
        skipTMP.fontSize = 14f;
        skipTMP.alignment = TextAlignmentOptions.Center;
        skipTMP.color = new Color(0.7f, 0.7f, 0.8f);
        skipTMP.raycastTarget = false;

        var tutUI = panelGO.AddComponent<TutorialUI>();
        var so = new SerializedObject(tutUI);
        so.FindProperty("panelGroup").objectReferenceValue = panelGroup;
        so.FindProperty("panelRect").objectReferenceValue = panelRT;
        so.FindProperty("titleText").objectReferenceValue = titleTMP;
        so.FindProperty("bodyText").objectReferenceValue = bodyTMP;
        so.FindProperty("pageIndicator").objectReferenceValue = pageTMP;
        so.FindProperty("tapHint").objectReferenceValue = tapTMP;
        so.FindProperty("nextButton").objectReferenceValue = nextBtn;
        so.FindProperty("skipButton").objectReferenceValue = skipBtn;
        so.ApplyModifiedProperties();

        var menuSO = new SerializedObject(mainMenuUI);
        menuSO.FindProperty("tutorialUI").objectReferenceValue = tutUI;
        menuSO.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(panelGO, "Create TutorialPanel");
        EditorSceneManager.MarkSceneDirty(panelGO.scene);
        Debug.Log("[Iter11] TutorialPanel created and wired. Save scene.");
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

    private static GameObject CreateSlider(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = CreateChild(parent, name, anchorMin, anchorMax);

        var bgGO = CreateChild(go, "Background", Vector2.zero, Vector2.one);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.18f, 0.32f);

        var fillAreaGO = CreateChild(go, "Fill Area", new Vector2(0.01f, 0.2f), new Vector2(0.99f, 0.8f));
        var fillGO = CreateChild(fillAreaGO, "Fill", Vector2.zero, Vector2.one);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.4f, 0.55f, 1f);

        var handleAreaGO = CreateChild(go, "Handle Slide Area", new Vector2(0.01f, 0f), new Vector2(0.99f, 1f));
        var handleGO = CreateChild(handleAreaGO, "Handle", Vector2.zero, Vector2.one);
        var handleRT = handleGO.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20f, 0f);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Color.white;

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fillGO.GetComponent<RectTransform>();
        slider.handleRect = handleRT;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.7f;
        slider.wholeNumbers = false;

        return go;
    }

    [MenuItem("RingGame/Iteration 11/Validate")]
    public static void Validate()
    {
        Debug.Log("[Iter11 Validate] ══════════════════════════════");

        var sm = FindObjectOfType<SettingsManager>(true);
        Debug.Log(sm != null ? "[Iter11] ✓ SettingsManager" : "[Iter11] ⚠️ SettingsManager missing");

        var sui = FindObjectOfType<SettingsUI>(true);
        Debug.Log(sui != null ? "[Iter11] ✓ SettingsUI" : "[Iter11] ⚠️ SettingsUI missing");

        var tui = FindObjectOfType<TutorialUI>(true);
        Debug.Log(tui != null ? "[Iter11] ✓ TutorialUI" : "[Iter11] ⚠️ TutorialUI missing");

        var menu = FindObjectOfType<MainMenuUI>(true);
        if (menu != null)
        {
            var so = new SerializedObject(menu);
            var setRef = so.FindProperty("settingsUI").objectReferenceValue;
            var tutRef = so.FindProperty("tutorialUI").objectReferenceValue;
            Debug.Log(setRef != null ? "[Iter11] ✓ MainMenuUI.settingsUI wired" : "[Iter11] ⚠️ MainMenuUI.settingsUI null");
            Debug.Log(tutRef != null ? "[Iter11] ✓ MainMenuUI.tutorialUI wired" : "[Iter11] ⚠️ MainMenuUI.tutorialUI null");

            var settBtn = so.FindProperty("settingsButton").objectReferenceValue;
            Debug.Log(settBtn != null ? "[Iter11] ✓ settingsButton wired" : "[Iter11] ⚠️ settingsButton null — assign manually");
        }

        Debug.Log("[Iter11 Validate] ══════════════════════════════");
    }
}
