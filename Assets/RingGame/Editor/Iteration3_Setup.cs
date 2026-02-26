using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

public class Iteration3_Setup : EditorWindow
{
    [MenuItem("RingGame/Iteration 3/Validate & Fix Scene")]
    public static void ValidateAndFix()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string scenePath = "Assets/RingGame/Scenes/Game.unity";
        if (!File.Exists(scenePath))
        {
            Debug.LogError("[Validate] Game.unity not found! Run Iteration 1 setup first.");
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        bool anyFixed = false;

        // GameManager и AudioManager живут на MainMenu сцене (Iteration 1) — не проверяем их здесь
        Debug.Log("[Validate] ℹ️ GameManager & AudioManager live on MainMenu scene — skipping check here.");

        anyFixed |= ValidateSingletons<BalanceManager>("BalanceManager");
        anyFixed |= ValidateSingletons<BetManager>("BetManager");
        anyFixed |= ValidateSingletons<RingsManager>("RingsManager");
        anyFixed |= ValidateSingletons<RhythmPhaseController>("RhythmPhaseController");
        anyFixed |= ValidateSingletons<RhythmPhaseUI>("RhythmPhaseUI");
        anyFixed |= ValidateSingletons<BetScreenUI>("BetScreenUI");
        anyFixed |= ValidateSingletons<EventSystem>("EventSystem");

        anyFixed |= ValidateRhythmCanvasInactive();
        anyFixed |= ValidateRingsManagerRefs();
        anyFixed |= ValidateRhythmPhaseControllerRefs();
        anyFixed |= ValidateNullFields<BetScreenUI>("BetScreenUI");
        anyFixed |= ValidateNullFields<RhythmPhaseUI>("RhythmPhaseUI");
        anyFixed |= ValidateBuildSettings();

        if (anyFixed)
        {
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("[Validate] ✅ Validation complete — some issues were fixed and scene was saved.");
        }
        else
        {
            Debug.Log("[Validate] ✅ Everything looks good! No issues found.");
        }
    }

    private static bool ValidateSingletons<T>(string label) where T : Component
    {
        var all = Object.FindObjectsOfType<T>(true);
        if (all.Length == 0)
        {
            Debug.LogWarning($"[Validate] ⚠️ {label} not found on scene. Run the Setup script for the appropriate iteration.");
            return false;
        }
        if (all.Length > 1)
        {
            Debug.Log($"[Validate] 🔧 Found {all.Length} instances of {label} — removing duplicates, keeping first.");
            for (int i = 1; i < all.Length; i++)
            {
                if (all[i] != null)
                    Object.DestroyImmediate(all[i].gameObject);
            }
            return true;
        }
        return false;
    }

    private static bool ValidateRhythmCanvasInactive()
    {
        var ui = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (ui == null) return false;

        var canvas = ui.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.gameObject.activeSelf)
        {
            Debug.Log("[Validate] 🔧 RhythmPhaseCanvas was active — setting inactive (it activates via GameState).");
            canvas.gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    private static bool ValidateRingsManagerRefs()
    {
        var rm = Object.FindObjectOfType<RingsManager>(true);
        if (rm == null) return false;

        var so = new SerializedObject(rm);
        bool fixed_ = false;

        if (so.FindProperty("ringsContainer").objectReferenceValue == null)
        {
            var container = GameObject.Find("RingsContainer");
            if (container != null)
            {
                so.FindProperty("ringsContainer").objectReferenceValue = container.GetComponent<RectTransform>();
                so.ApplyModifiedProperties();
                Debug.Log("[Validate] 🔧 RingsManager.ringsContainer was null — reassigned.");
                fixed_ = true;
            }
            else
                Debug.LogWarning("[Validate] ⚠️ RingsManager.ringsContainer is null and RingsContainer object not found. Re-run Iteration 3 setup.");
        }

        if (so.FindProperty("ringSprite").objectReferenceValue == null)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/RingGame/Textures/RingSprite_0.png");
            if (sprite != null)
            {
                so.FindProperty("ringSprite").objectReferenceValue = sprite;
                so.ApplyModifiedProperties();
                Debug.Log("[Validate] 🔧 RingsManager.ringSprite was null — reassigned from Textures folder.");
                fixed_ = true;
            }
            else
                Debug.LogWarning("[Validate] ⚠️ RingsManager.ringSprite is null and texture not found. Re-run Iteration 3 setup.");
        }

        if (so.FindProperty("symbolBgSprite").objectReferenceValue == null)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/RingGame/Textures/SymbolBg.png");
            if (sprite != null)
            {
                so.FindProperty("symbolBgSprite").objectReferenceValue = sprite;
                so.ApplyModifiedProperties();
                Debug.Log("[Validate] 🔧 RingsManager.symbolBgSprite was null — reassigned.");
                fixed_ = true;
            }
        }

        if (so.FindProperty("symbolConfig").objectReferenceValue == null)
        {
            var config = AssetDatabase.LoadAssetAtPath<SymbolConfig>("Assets/RingGame/Data/SymbolConfig.asset");
            if (config != null)
            {
                so.FindProperty("symbolConfig").objectReferenceValue = config;
                so.ApplyModifiedProperties();
                Debug.Log("[Validate] 🔧 RingsManager.symbolConfig was null — reassigned.");
                fixed_ = true;
            }
            else
                Debug.LogWarning("[Validate] ⚠️ SymbolConfig asset not found. Run: RingGame → Iteration 3 → Create Symbol Config Asset.");
        }

        return fixed_;
    }

    private static bool ValidateRhythmPhaseControllerRefs()
    {
        var ctrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (ctrl == null) return false;

        var so = new SerializedObject(ctrl);
        if (so.FindProperty("rhythmPhaseUI").objectReferenceValue == null)
        {
            var ui = Object.FindObjectOfType<RhythmPhaseUI>(true);
            if (ui != null)
            {
                so.FindProperty("rhythmPhaseUI").objectReferenceValue = ui;
                so.ApplyModifiedProperties();
                Debug.Log("[Validate] 🔧 RhythmPhaseController.rhythmPhaseUI was null — reassigned.");
                return true;
            }
            else
                Debug.LogWarning("[Validate] ⚠️ RhythmPhaseController.rhythmPhaseUI is null and RhythmPhaseUI not found.");
        }
        return false;
    }

    private static bool ValidateNullFields<T>(string label) where T : Component
    {
        var component = Object.FindObjectOfType<T>(true);
        if (component == null) return false;

        var so = new SerializedObject(component);
        var iter = so.GetIterator();
        bool hasNull = false;

        iter.Next(true);
        while (iter.NextVisible(false))
        {
            if (iter.propertyType == SerializedPropertyType.ObjectReference &&
                iter.objectReferenceValue == null &&
                iter.name != "m_Script")
            {
                Debug.LogWarning($"[Validate] ⚠️ {label}.{iter.name} is null — assign it manually or re-run the setup script.");
                hasNull = true;
            }
        }
        return false;
    }

    private static bool ValidateBuildSettings()
    {
        bool fixed_ = false;
        var scenes = EditorBuildSettings.scenes;
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes);

        string mainMenuPath = "Assets/RingGame/Scenes/MainMenu.unity";
        string gamePath = "Assets/RingGame/Scenes/Game.unity";

        bool hasMainMenu = list.Exists(s => s.path == mainMenuPath);
        bool hasGame = list.Exists(s => s.path == gamePath);

        if (!hasMainMenu && File.Exists(mainMenuPath))
        {
            list.Insert(0, new EditorBuildSettingsScene(mainMenuPath, true));
            Debug.Log("[Validate] 🔧 MainMenu.unity was missing from Build Settings — added at index 0.");
            fixed_ = true;
        }
        if (!hasGame && File.Exists(gamePath))
        {
            list.Add(new EditorBuildSettingsScene(gamePath, true));
            Debug.Log("[Validate] 🔧 Game.unity was missing from Build Settings — added.");
            fixed_ = true;
        }

        if (fixed_)
            EditorBuildSettings.scenes = list.ToArray();

        var mainIdx = list.FindIndex(s => s.path == mainMenuPath);
        var gameIdx = list.FindIndex(s => s.path == gamePath);

        if (mainIdx > 0)
            Debug.LogWarning($"[Validate] ⚠️ MainMenu.unity is at index {mainIdx} in Build Settings — it should be index 0 (first scene loaded).");
        if (gameIdx >= 0 && gameIdx < mainIdx)
            Debug.LogWarning($"[Validate] ⚠️ Game.unity (index {gameIdx}) appears before MainMenu.unity (index {mainIdx}) in Build Settings.");

        return fixed_;
    }

    [MenuItem("RingGame/Iteration 3/Setup Game Scene - Rings")]
    public static void SetupGameScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string scenePath = "Assets/RingGame/Scenes/Game.unity";
        if (!File.Exists(scenePath))
        {
            Debug.LogError("[Iteration 3] Game.unity not found! Run Iteration 1 first.");
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        EnsureEventSystem();
        EnsureManagers();

        var symbolConfig = EnsureSymbolConfig();
        var ringSprite = GetOrGenerateRingSprite();
        var symbolBgSprite = GetOrGenerateSymbolBgSprite();

        SetupRhythmPhaseCanvas(symbolConfig, ringSprite, symbolBgSprite);

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[Iteration 3] Game scene updated with Rings system!");
    }

    [MenuItem("RingGame/Iteration 3/Create Symbol Config Asset")]
    public static void CreateSymbolConfigAsset()
    {
        EnsureFolderExists("Assets/RingGame/Data");
        string path = "Assets/RingGame/Data/SymbolConfig.asset";
        if (File.Exists(path))
        {
            Debug.Log("[Iteration 3] SymbolConfig already exists at " + path);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SymbolConfig>(path);
            return;
        }
        var config = ScriptableObject.CreateInstance<SymbolConfig>();
        AssetDatabase.CreateAsset(config, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = config;
        Debug.Log("[Iteration 3] SymbolConfig created at " + path + " — assign your suit sprites here!");
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    private static void EnsureManagers()
    {
        if (Object.FindObjectOfType<BalanceManager>() == null)
            new GameObject("BalanceManager").AddComponent<BalanceManager>();

        if (Object.FindObjectOfType<BetManager>() == null)
            new GameObject("BetManager").AddComponent<BetManager>();
    }

    private static SymbolConfig EnsureSymbolConfig()
    {
        string path = "Assets/RingGame/Data/SymbolConfig.asset";
        EnsureFolderExists("Assets/RingGame/Data");

        var config = AssetDatabase.LoadAssetAtPath<SymbolConfig>(path);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<SymbolConfig>();
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
        }
        return config;
    }

    private static void SetupRhythmPhaseCanvas(SymbolConfig symbolConfig, Sprite ringSprite, Sprite symbolBgSprite)
    {
        if (Object.FindObjectOfType<RhythmPhaseUI>(true) != null)
        {
            Debug.Log("[Iteration 3] RhythmPhaseUI already exists. Updating RingsManager references.");
            UpdateRingsManagerReferences(symbolConfig, ringSprite, symbolBgSprite);
            return;
        }

        var canvasGO = new GameObject("RhythmPhaseCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 15;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(390, 844);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var rootCG = canvasGO.AddComponent<CanvasGroup>();
        var phaseUI = canvasGO.AddComponent<RhythmPhaseUI>();

        var bgGO = CreateFullRect(canvasGO.transform, "Background");
        bgGO.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.12f);

        var topBarGO = new GameObject("TopBar");
        topBarGO.transform.SetParent(canvasGO.transform, false);
        var topBarRT = topBarGO.AddComponent<RectTransform>();
        topBarRT.anchorMin = new Vector2(0f, 1f);
        topBarRT.anchorMax = new Vector2(1f, 1f);
        topBarRT.pivot = new Vector2(0.5f, 1f);
        topBarRT.anchoredPosition = new Vector2(0f, -44f);
        topBarRT.sizeDelta = new Vector2(0f, 70f);
        topBarGO.AddComponent<Image>().color = new Color(0.08f, 0.07f, 0.18f, 0.95f);

        var cycleTxtGO = CreateTMPLabel(topBarGO.transform, "CycleText", "CYCLE 1/4",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(160f, 44f),
            20f, new Color(0.7f, 0.75f, 1f), FontStyles.Bold, TextAlignmentOptions.Left);

        var betTxtGO = CreateTMPLabel(topBarGO.transform, "BetText", "$10",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(120f, 44f),
            22f, new Color(1f, 0.82f, 0.15f), FontStyles.Bold, TextAlignmentOptions.Center);

        var attemptsRowGO = new GameObject("AttemptsRow");
        attemptsRowGO.transform.SetParent(topBarGO.transform, false);
        var attRowRT = attemptsRowGO.AddComponent<RectTransform>();
        attRowRT.anchorMin = new Vector2(1f, 0.5f);
        attRowRT.anchorMax = new Vector2(1f, 0.5f);
        attRowRT.pivot = new Vector2(1f, 0.5f);
        attRowRT.anchoredPosition = new Vector2(-16f, 0f);
        attRowRT.sizeDelta = new Vector2(120f, 28f);

        var dots = new Image[4];
        for (int i = 0; i < 4; i++)
        {
            var dotGO = new GameObject($"Dot_{i}");
            dotGO.transform.SetParent(attemptsRowGO.transform, false);
            var dotRT = dotGO.AddComponent<RectTransform>();
            dotRT.anchorMin = new Vector2(0.5f, 0.5f);
            dotRT.anchorMax = new Vector2(0.5f, 0.5f);
            dotRT.pivot = new Vector2(0.5f, 0.5f);
            dotRT.anchoredPosition = new Vector2(-42f + i * 28f, 0f);
            dotRT.sizeDelta = new Vector2(18f, 18f);
            var dotImg = dotGO.AddComponent<Image>();
            dotImg.color = new Color(1f, 0.82f, 0.15f, 1f);
            dots[i] = dotImg;
        }

        var ringsAreaGO = new GameObject("RingsArea");
        ringsAreaGO.transform.SetParent(canvasGO.transform, false);
        var ringsAreaRT = ringsAreaGO.AddComponent<RectTransform>();
        ringsAreaRT.anchorMin = new Vector2(0.5f, 0.5f);
        ringsAreaRT.anchorMax = new Vector2(0.5f, 0.5f);
        ringsAreaRT.pivot = new Vector2(0.5f, 0.5f);
        ringsAreaRT.anchoredPosition = new Vector2(0f, 55f);
        ringsAreaRT.sizeDelta = new Vector2(500f, 500f);
        var ringsAreaCG = ringsAreaGO.AddComponent<CanvasGroup>();

        var ringsContainerGO = new GameObject("RingsContainer");
        ringsContainerGO.transform.SetParent(ringsAreaGO.transform, false);
        var ringsContainerRT = ringsContainerGO.AddComponent<RectTransform>();
        ringsContainerRT.anchorMin = new Vector2(0.5f, 0.5f);
        ringsContainerRT.anchorMax = new Vector2(0.5f, 0.5f);
        ringsContainerRT.pivot = new Vector2(0.5f, 0.5f);
        ringsContainerRT.anchoredPosition = Vector2.zero;
        ringsContainerRT.sizeDelta = new Vector2(500f, 500f);
        ringsContainerGO.AddComponent<CanvasGroup>();

        var ringsManagerGO = new GameObject("RingsManager");
        ringsManagerGO.transform.SetParent(canvasGO.transform, false);
        var ringsManager = ringsManagerGO.AddComponent<RingsManager>();

        var rmSO = new SerializedObject(ringsManager);
        rmSO.FindProperty("ringsContainer").objectReferenceValue = ringsContainerRT;
        rmSO.FindProperty("symbolConfig").objectReferenceValue = symbolConfig;
        rmSO.FindProperty("ringSprite").objectReferenceValue = ringSprite;
        rmSO.FindProperty("symbolBgSprite").objectReferenceValue = symbolBgSprite;
        rmSO.ApplyModifiedProperties();

        var gridAreaGO = new GameObject("GridArea");
        gridAreaGO.transform.SetParent(canvasGO.transform, false);
        var gridAreaRT = gridAreaGO.AddComponent<RectTransform>();
        gridAreaRT.anchorMin = new Vector2(0f, 0f);
        gridAreaRT.anchorMax = new Vector2(1f, 0f);
        gridAreaRT.pivot = new Vector2(0.5f, 0f);
        gridAreaRT.anchoredPosition = new Vector2(0f, 20f);
        gridAreaRT.sizeDelta = new Vector2(-40f, 180f);
        gridAreaGO.AddComponent<Image>().color = new Color(0.08f, 0.07f, 0.18f, 0.9f);
        var gridCG = gridAreaGO.AddComponent<CanvasGroup>();

        var gridLabelGO = CreateTMPLabel(gridAreaGO.transform, "GridLabel", "GRID",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-60f, -8f), new Vector2(60f, -36f),
            14f, new Color(0.5f, 0.55f, 0.75f), FontStyles.Normal, TextAlignmentOptions.Center);

        var gridCellsGO = new GameObject("GridCells");
        gridCellsGO.transform.SetParent(gridAreaGO.transform, false);
        var gridCellsRT = gridCellsGO.AddComponent<RectTransform>();
        gridCellsRT.anchorMin = new Vector2(0.5f, 0.5f);
        gridCellsRT.anchorMax = new Vector2(0.5f, 0.5f);
        gridCellsRT.pivot = new Vector2(0.5f, 0.5f);
        gridCellsRT.anchoredPosition = new Vector2(0f, -15f);
        gridCellsRT.sizeDelta = new Vector2(240f, 100f);

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 2; col++)
            {
                var cellGO = new GameObject($"Cell_{row}_{col}");
                cellGO.transform.SetParent(gridCellsGO.transform, false);
                var cellRT = cellGO.AddComponent<RectTransform>();
                cellRT.anchorMin = new Vector2(0.5f, 0.5f);
                cellRT.anchorMax = new Vector2(0.5f, 0.5f);
                cellRT.pivot = new Vector2(0.5f, 0.5f);
                cellRT.sizeDelta = new Vector2(50f, 50f);
                cellRT.anchoredPosition = new Vector2(-30f + col * 62f, 37f - row * 26f);
                var cellImg = cellGO.AddComponent<Image>();
                cellImg.color = new Color(0.15f, 0.13f, 0.28f, 0.85f);
            }
        }


        var backBtnGO = CreateButton(canvasGO.transform, "BackButton", "← BACK",
            new Vector2(50f, -90f), new Vector2(90f, 34f), new Color(0.18f, 0.15f, 0.3f),
            new Color(0.7f, 0.72f, 1f), 14f, new Vector2(0f, 1f), new Vector2(0f, 1f));

        canvasGO.SetActive(false);

        // RhythmPhaseController на отдельном активном объекте — НЕ внутри неактивного канваса
        var existingCtrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        RhythmPhaseController controller;
        if (existingCtrl != null)
        {
            controller = existingCtrl;
            if (controller.transform.parent != null && controller.transform.IsChildOf(canvasGO.transform))
                controller.transform.SetParent(null);
        }
        else
        {
            var controllerGO = new GameObject("RhythmPhaseController");
            controller = controllerGO.AddComponent<RhythmPhaseController>();
        }

        var ctrlSO = new SerializedObject(controller);
        ctrlSO.FindProperty("rhythmPhaseUI").objectReferenceValue = phaseUI;
        ctrlSO.ApplyModifiedProperties();

        var uiSO = new SerializedObject(phaseUI);
        uiSO.FindProperty("rootGroup").objectReferenceValue = rootCG;
        uiSO.FindProperty("topBar").objectReferenceValue = topBarRT;
        uiSO.FindProperty("cycleText").objectReferenceValue = cycleTxtGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("betAmountText").objectReferenceValue = betTxtGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("attemptsRow").objectReferenceValue = attRowRT;
        var dotsProp = uiSO.FindProperty("attemptDots");
        dotsProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
            dotsProp.GetArrayElementAtIndex(i).objectReferenceValue = dots[i];
        uiSO.FindProperty("ringsArea").objectReferenceValue = ringsAreaRT;
        uiSO.FindProperty("gridArea").objectReferenceValue = gridAreaRT;
        uiSO.FindProperty("gridGroup").objectReferenceValue = gridCG;
        uiSO.FindProperty("backButton").objectReferenceValue = backBtnGO.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();

        Debug.Log("[Iteration 3] RhythmPhase canvas created!");
    }

    private static void UpdateRingsManagerReferences(SymbolConfig symbolConfig, Sprite ringSprite, Sprite symbolBgSprite)
    {
        var rm = Object.FindObjectOfType<RingsManager>(true);
        if (rm == null) return;

        var so = new SerializedObject(rm);
        so.FindProperty("symbolConfig").objectReferenceValue = symbolConfig;
        so.FindProperty("ringSprite").objectReferenceValue = ringSprite;
        so.FindProperty("symbolBgSprite").objectReferenceValue = symbolBgSprite;
        so.ApplyModifiedProperties();
        Debug.Log("[Iteration 3] RingsManager references updated.");
    }

    private static Sprite GetOrGenerateRingSprite()
    {
        EnsureFolderExists("Assets/RingGame/Textures");

        string path = "Assets/RingGame/Textures/RingSprite_0.png";
        if (File.Exists(path))
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
        }

        return GenerateRingTexture(path, 256, 14f);
    }

    private static Sprite GetOrGenerateSymbolBgSprite()
    {
        EnsureFolderExists("Assets/RingGame/Textures");
        string path = "Assets/RingGame/Textures/SymbolBg.png";
        if (File.Exists(path))
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
        }

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c = size / 2f;
        float r = c - 2f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                float alpha = dist < r ? Mathf.Clamp01((r - dist) / 2f) : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        tex.Apply();

        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        SetSpriteImportSettings(path);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite GenerateRingTexture(string path, int size, float thickness)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c = size / 2f;
        float outerR = c - 2f;
        float innerR = outerR - thickness;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                float t = 0f;
                if (dist >= innerR && dist <= outerR)
                    t = 1f - Mathf.Abs((dist - (innerR + outerR) * 0.5f) / ((outerR - innerR) * 0.5f));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(t * 1.5f)));
            }
        tex.Apply();

        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        SetSpriteImportSettings(path);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void SetSpriteImportSettings(string path)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;
        ti.textureType = TextureImporterType.Sprite;
        ti.alphaIsTransparency = true;
        ti.filterMode = FilterMode.Bilinear;
        ti.SaveAndReimport();
    }

    private static GameObject CreateFullRect(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>();
        return go;
    }

    private static GameObject CreateTMPLabel(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        float fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.color = color;
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Vector2 size, Color bgColor, Color textColor,
        float fontSize, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = bgColor;
        go.AddComponent<Button>();

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = textColor;
        return go;
    }

    private static void EnsureFolderExists(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
