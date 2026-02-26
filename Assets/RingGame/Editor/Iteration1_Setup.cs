using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

public class Iteration1_Setup : EditorWindow
{
    [MenuItem("RingGame/Iteration 1/Setup Main Menu Scene")]
    public static void SetupMainMenuScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureFolderExists("Assets/RingGame/Scenes");
        string scenePath = "Assets/RingGame/Scenes/MainMenu.unity";

        UnityEngine.SceneManagement.Scene scene;
        if (File.Exists(scenePath))
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        else
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Setup2DCamera();
        EnsureEventSystem();
        SetupManagers();
        SetupMainMenuUI();

        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuildSettings(scenePath, 0);

        Debug.Log("[Iteration 1] Main Menu scene setup complete!");
    }

    [MenuItem("RingGame/Iteration 1/Create Empty Game Scene")]
    public static void CreateGameScene()
    {
        EnsureFolderExists("Assets/RingGame/Scenes");
        string scenePath = "Assets/RingGame/Scenes/Game.unity";

        if (File.Exists(scenePath))
        {
            AddSceneToBuildSettings(scenePath, 1);
            Debug.Log("[Iteration 1] Game scene already exists.");
            return;
        }

        var currentScene = EditorSceneManager.GetActiveScene();
        var gameScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.SetActiveScene(gameScene);

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.04f, 0.12f);
        cam.transform.position = new Vector3(0, 0, -10);

        EnsureEventSystem();

        EditorSceneManager.SaveScene(gameScene, scenePath);
        AddSceneToBuildSettings(scenePath, 1);
        EditorSceneManager.SetActiveScene(currentScene);
        EditorSceneManager.CloseScene(gameScene, true);

        Debug.Log("[Iteration 1] Game scene created!");
    }

    private static void Setup2DCamera()
    {
        var camGO = GameObject.Find("Main Camera");
        if (camGO == null)
        {
            camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
        }

        var cam = camGO.GetComponent<Camera>();
        if (cam == null) cam = camGO.AddComponent<Camera>();

        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.04f, 0.12f);
        cam.transform.position = new Vector3(0, 0, -10);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();
    }

    private static void SetupManagers()
    {
        SetupGameManager();
        SetupAudioManager();
        SetupSceneTransitionManager();
    }

    private static void SetupGameManager()
    {
        if (Object.FindObjectOfType<GameManager>() != null) return;
        new GameObject("GameManager").AddComponent<GameManager>();
        Debug.Log("[Iteration 1] GameManager created.");
    }

    private static void SetupAudioManager()
    {
        if (Object.FindObjectOfType<AudioManager>() != null) return;

        var go = new GameObject("AudioManager");
        var am = go.AddComponent<AudioManager>();

        var musicGO = new GameObject("MusicSource");
        musicGO.transform.SetParent(go.transform);
        var musicAS = musicGO.AddComponent<AudioSource>();
        musicAS.loop = true;
        musicAS.volume = 0.75f;
        musicAS.playOnAwake = false;

        var sfxGO = new GameObject("SFXSource");
        sfxGO.transform.SetParent(go.transform);
        var sfxAS = sfxGO.AddComponent<AudioSource>();
        sfxAS.playOnAwake = false;

        var so = new SerializedObject(am);
        so.FindProperty("musicSource").objectReferenceValue = musicAS;
        so.FindProperty("sfxSource").objectReferenceValue = sfxAS;
        so.ApplyModifiedProperties();

        Debug.Log("[Iteration 1] AudioManager created. Assign a music clip in Inspector.");
    }

    private static void SetupSceneTransitionManager()
    {
        if (Object.FindObjectOfType<SceneTransitionManager>() != null) return;

        var go = new GameObject("SceneTransitionManager");
        var stm = go.AddComponent<SceneTransitionManager>();

        var canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(go.transform);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var overlayGO = new GameObject("FadeOverlay");
        overlayGO.transform.SetParent(canvasGO.transform, false);
        var rt = overlayGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = overlayGO.AddComponent<Image>();
        img.color = Color.black;
        var cg = overlayGO.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = true;
        cg.interactable = false;
        overlayGO.SetActive(false);

        var so = new SerializedObject(stm);
        so.FindProperty("fadeOverlay").objectReferenceValue = cg;
        so.ApplyModifiedProperties();

        Debug.Log("[Iteration 1] SceneTransitionManager created.");
    }

    private static void SetupMainMenuUI()
    {
        if (Object.FindObjectOfType<MainMenuUI>() != null)
        {
            Debug.Log("[Iteration 1] MainMenuUI already exists, skipping.");
            return;
        }

        var canvasGO = new GameObject("MainMenuCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(390, 844);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        var menuUI = canvasGO.AddComponent<MainMenuUI>();

        var bg = CreateFullRect(canvasGO.transform, "Background");
        bg.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.12f);

        var bgRingsGO = new GameObject("BgRings");
        bgRingsGO.transform.SetParent(canvasGO.transform, false);
        var bgFullRT = bgRingsGO.AddComponent<RectTransform>();
        bgFullRT.anchorMin = Vector2.zero;
        bgFullRT.anchorMax = Vector2.one;
        bgFullRT.offsetMin = Vector2.zero;
        bgFullRT.offsetMax = Vector2.zero;
        var bgCG = bgRingsGO.AddComponent<CanvasGroup>();
        bgCG.blocksRaycasts = false;
        bgCG.interactable = false;

        float[] ringDiameters = { 700f, 520f, 340f };
        float[] ringThicknesses = { 18f, 14f, 10f };
        float[] ringAlphas = { 0.15f, 0.12f, 0.09f };
        var bgRings = new RectTransform[3];

        EnsureFolderExists("Assets/RingGame/Textures");

        for (int i = 0; i < 3; i++)
        {
            var ringGO = new GameObject($"BgRing_{i}");
            ringGO.transform.SetParent(bgRingsGO.transform, false);
            var ringRT = ringGO.AddComponent<RectTransform>();
            ringRT.anchorMin = new Vector2(0.5f, 0.5f);
            ringRT.anchorMax = new Vector2(0.5f, 0.5f);
            ringRT.pivot = new Vector2(0.5f, 0.5f);
            ringRT.anchoredPosition = Vector2.zero;
            ringRT.sizeDelta = new Vector2(ringDiameters[i], ringDiameters[i]);

            var ringImg = ringGO.AddComponent<Image>();
            ringImg.sprite = GetOrCreateRingSprite(i, ringThicknesses[i]);
            ringImg.color = new Color(0.45f, 0.65f, 1f, ringAlphas[i]);
            ringImg.raycastTarget = false;
            bgRings[i] = ringRT;
        }

        var logoContainerGO = new GameObject("LogoContainer");
        logoContainerGO.transform.SetParent(canvasGO.transform, false);
        var logoRT = logoContainerGO.AddComponent<RectTransform>();
        logoRT.anchorMin = new Vector2(0.5f, 1f);
        logoRT.anchorMax = new Vector2(0.5f, 1f);
        logoRT.pivot = new Vector2(0.5f, 1f);
        logoRT.anchoredPosition = new Vector2(0f, -130f);
        logoRT.sizeDelta = new Vector2(360f, 180f);
        var logoCG = logoContainerGO.AddComponent<CanvasGroup>();

        var titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(logoContainerGO.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.5f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "RING GAME";
        titleTMP.fontSize = 64;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.82f, 0.15f);

        var subtitleGO = new GameObject("SubtitleText");
        subtitleGO.transform.SetParent(logoContainerGO.transform, false);
        var subtitleRT = subtitleGO.AddComponent<RectTransform>();
        subtitleRT.anchorMin = new Vector2(0f, 0f);
        subtitleRT.anchorMax = new Vector2(1f, 0.5f);
        subtitleRT.offsetMin = Vector2.zero;
        subtitleRT.offsetMax = Vector2.zero;
        var subtitleTMP = subtitleGO.AddComponent<TextMeshProUGUI>();
        subtitleTMP.text = "Rhythm · Luck · Skill";
        subtitleTMP.fontSize = 24;
        subtitleTMP.alignment = TextAlignmentOptions.Center;
        subtitleTMP.color = new Color(0.75f, 0.78f, 1f, 0.8f);

        var buttonContainerGO = new GameObject("ButtonsContainer");
        buttonContainerGO.transform.SetParent(canvasGO.transform, false);
        var buttonsRT = buttonContainerGO.AddComponent<RectTransform>();
        buttonsRT.anchorMin = new Vector2(0.5f, 0f);
        buttonsRT.anchorMax = new Vector2(0.5f, 0f);
        buttonsRT.pivot = new Vector2(0.5f, 0f);
        buttonsRT.anchoredPosition = new Vector2(0f, 120f);
        buttonsRT.sizeDelta = new Vector2(300f, 90f);
        var buttonsCG = buttonContainerGO.AddComponent<CanvasGroup>();

        var playBtnGO = CreateButton(buttonContainerGO.transform, "PlayButton", "PLAY",
            Vector2.zero, new Vector2(300f, 80f), new Color(1f, 0.78f, 0.05f));

        var so = new SerializedObject(menuUI);
        so.FindProperty("logoContainer").objectReferenceValue = logoRT;
        so.FindProperty("logoGroup").objectReferenceValue = logoCG;
        so.FindProperty("titleText").objectReferenceValue = titleTMP;
        so.FindProperty("subtitleText").objectReferenceValue = subtitleTMP;
        so.FindProperty("buttonsGroup").objectReferenceValue = buttonsCG;
        so.FindProperty("playButton").objectReferenceValue = playBtnGO.GetComponent<Button>();
        so.FindProperty("playButtonRect").objectReferenceValue = playBtnGO.GetComponent<RectTransform>();

        var bgRingsProp = so.FindProperty("bgRingVisuals");
        bgRingsProp.arraySize = bgRings.Length;
        for (int i = 0; i < bgRings.Length; i++)
            bgRingsProp.GetArrayElementAtIndex(i).objectReferenceValue = bgRings[i];

        so.FindProperty("bgGroup").objectReferenceValue = bgCG;
        so.ApplyModifiedProperties();

        Debug.Log("[Iteration 1] MainMenuUI setup complete!");
    }

    private static Sprite GetOrCreateRingSprite(int index, float thickness)
    {
        string path = $"Assets/RingGame/Textures/RingSprite_{index}.png";
        if (File.Exists(path))
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
        }

        int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float outerR = size / 2f - 2f;
        float innerR = outerR - thickness;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, (dist >= innerR && dist <= outerR) ? Color.white : Color.clear);
            }
        }
        tex.Apply();

        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.alphaIsTransparency = true;
            ti.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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

    private static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = color;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 0.85f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = colors;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 30;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.1f, 0.06f, 0f);

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

    private static void AddSceneToBuildSettings(string scenePath, int index)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == scenePath)) return;
        scenes.Insert(Mathf.Clamp(index, 0, scenes.Count), new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[Iteration 1] Added {scenePath} to Build Settings.");
    }
}
