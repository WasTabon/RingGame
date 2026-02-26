using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

public class Iteration2_Setup : EditorWindow
{
    [MenuItem("RingGame/Iteration 2/Setup Game Scene - Bet Screen")]
    public static void SetupGameScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string scenePath = "Assets/RingGame/Scenes/Game.unity";
        if (!File.Exists(scenePath))
        {
            Debug.LogError("[Iteration 2] Game.unity not found! Run Iteration 1 first.");
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        EnsureEventSystem();
        EnsureBalanceManager();
        EnsureBetManager();
        SetupBetScreenUI();

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[Iteration 2] Game scene updated with Bet Screen!");
    }

    [MenuItem("RingGame/Iteration 2/Reset Player Balance")]
    public static void ResetBalance()
    {
        PlayerPrefs.SetFloat("PlayerBalance", 1000f);
        PlayerPrefs.Save();
        Debug.Log("[Iteration 2] Balance reset to $1000.");
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();
    }

    private static void EnsureBalanceManager()
    {
        if (Object.FindObjectOfType<BalanceManager>() != null) return;
        new GameObject("BalanceManager").AddComponent<BalanceManager>();
        Debug.Log("[Iteration 2] BalanceManager added.");
    }

    private static void EnsureBetManager()
    {
        if (Object.FindObjectOfType<BetManager>() != null) return;
        new GameObject("BetManager").AddComponent<BetManager>();
        Debug.Log("[Iteration 2] BetManager added.");
    }

    private static void SetupBetScreenUI()
    {
        var existing = Object.FindObjectOfType<BetScreenUI>();
        if (existing != null)
        {
            Debug.Log("[Iteration 2] BetScreenUI already exists, skipping.");
            return;
        }

        var canvasGO = new GameObject("BetScreenCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(390, 844);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        var betUI = canvasGO.AddComponent<BetScreenUI>();

        var rootCG = canvasGO.AddComponent<CanvasGroup>();

        var bgGO = CreateFullRect(canvasGO.transform, "Background");
        bgGO.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.12f);

        var decorCircle = CreateCircleDecor(canvasGO.transform);

        var headerGO = new GameObject("Header");
        headerGO.transform.SetParent(canvasGO.transform, false);
        var headerRT = headerGO.AddComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0f, 1f);
        headerRT.anchorMax = new Vector2(1f, 1f);
        headerRT.pivot = new Vector2(0.5f, 1f);
        headerRT.anchoredPosition = new Vector2(0f, -50f);
        headerRT.sizeDelta = new Vector2(0f, 80f);
        headerGO.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

        var balanceLabelGO = CreateTMPText(headerGO.transform, "BalanceLabel", "BALANCE",
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -10f), new Vector2(0f, -38f),
            18f, new Color(0.6f, 0.65f, 0.85f));

        var balanceValueGO = CreateTMPText(headerGO.transform, "BalanceValue", "$1000",
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, -40f), new Vector2(0f, -8f),
            36f, Color.white, FontStyles.Bold);

        var betDisplayGO = new GameObject("BetDisplay");
        betDisplayGO.transform.SetParent(canvasGO.transform, false);
        var betDisplayRT = betDisplayGO.AddComponent<RectTransform>();
        betDisplayRT.anchorMin = new Vector2(0.5f, 0.5f);
        betDisplayRT.anchorMax = new Vector2(0.5f, 0.5f);
        betDisplayRT.pivot = new Vector2(0.5f, 0.5f);
        betDisplayRT.anchoredPosition = new Vector2(0f, 60f);
        betDisplayRT.sizeDelta = new Vector2(300f, 160f);
        var betDisplayBg = betDisplayGO.AddComponent<Image>();
        betDisplayBg.color = new Color(0.12f, 0.1f, 0.22f);

        var betLabelGO = CreateTMPText(betDisplayGO.transform, "BetLabel", "YOUR BET",
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -16f), new Vector2(-20f, -42f),
            16f, new Color(0.6f, 0.65f, 0.85f));

        var betValueGO = CreateTMPText(betDisplayGO.transform, "BetValue", "$10",
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, -46f), new Vector2(-20f, 16f),
            56f, new Color(1f, 0.82f, 0.15f), FontStyles.Bold);

        var betControlsGO = new GameObject("BetControls");
        betControlsGO.transform.SetParent(canvasGO.transform, false);
        var betControlsRT = betControlsGO.AddComponent<RectTransform>();
        betControlsRT.anchorMin = new Vector2(0.5f, 0.5f);
        betControlsRT.anchorMax = new Vector2(0.5f, 0.5f);
        betControlsRT.pivot = new Vector2(0.5f, 0.5f);
        betControlsRT.anchoredPosition = new Vector2(0f, -40f);
        betControlsRT.sizeDelta = new Vector2(300f, 60f);

        var decreaseBtnGO = CreateRoundButton(betControlsGO.transform, "DecreaseBtn", "−",
            new Vector2(-100f, 0f), 60f, new Color(0.18f, 0.15f, 0.3f), new Color(0.85f, 0.85f, 1f));

        var increaseBtnGO = CreateRoundButton(betControlsGO.transform, "IncreaseBtn", "+",
            new Vector2(100f, 0f), 60f, new Color(1f, 0.78f, 0.05f), new Color(0.1f, 0.06f, 0f));

        var sliderTrackGO = new GameObject("SliderTrack");
        sliderTrackGO.transform.SetParent(betControlsGO.transform, false);
        var sliderTrackRT = sliderTrackGO.AddComponent<RectTransform>();
        sliderTrackRT.anchorMin = new Vector2(0.5f, 0.5f);
        sliderTrackRT.anchorMax = new Vector2(0.5f, 0.5f);
        sliderTrackRT.pivot = new Vector2(0.5f, 0.5f);
        sliderTrackRT.anchoredPosition = Vector2.zero;
        sliderTrackRT.sizeDelta = new Vector2(120f, 6f);
        sliderTrackGO.AddComponent<Image>().color = new Color(0.25f, 0.22f, 0.4f);

        var sliderFillGO = new GameObject("SliderFill");
        sliderFillGO.transform.SetParent(sliderTrackGO.transform, false);
        var sliderFillRT = sliderFillGO.AddComponent<RectTransform>();
        sliderFillRT.anchorMin = new Vector2(0f, 0f);
        sliderFillRT.anchorMax = new Vector2(0f, 1f);
        sliderFillRT.pivot = new Vector2(0f, 0.5f);
        sliderFillRT.anchoredPosition = Vector2.zero;
        sliderFillRT.sizeDelta = new Vector2(8f, 0f);
        sliderFillGO.AddComponent<Image>().color = new Color(1f, 0.82f, 0.15f);

        var presetRowGO = new GameObject("PresetRow");
        presetRowGO.transform.SetParent(canvasGO.transform, false);
        var presetRT = presetRowGO.AddComponent<RectTransform>();
        presetRT.anchorMin = new Vector2(0.5f, 0.5f);
        presetRT.anchorMax = new Vector2(0.5f, 0.5f);
        presetRT.pivot = new Vector2(0.5f, 0.5f);
        presetRT.anchoredPosition = new Vector2(0f, -115f);
        presetRT.sizeDelta = new Vector2(300f, 44f);

        var minBtnGO = CreatePresetButton(presetRowGO.transform, "MinBtn", "MIN", new Vector2(-100f, 0f));
        var halfBtnGO = CreatePresetButton(presetRowGO.transform, "HalfBtn", "HALF", new Vector2(0f, 0f));
        var maxBtnGO = CreatePresetButton(presetRowGO.transform, "MaxBtn", "MAX", new Vector2(100f, 0f));

        var startBtnGO = new GameObject("StartButton");
        startBtnGO.transform.SetParent(canvasGO.transform, false);
        var startRT = startBtnGO.AddComponent<RectTransform>();
        startRT.anchorMin = new Vector2(0.5f, 0f);
        startRT.anchorMax = new Vector2(0.5f, 0f);
        startRT.pivot = new Vector2(0.5f, 0f);
        startRT.anchoredPosition = new Vector2(0f, 80f);
        startRT.sizeDelta = new Vector2(300f, 72f);
        startBtnGO.AddComponent<Image>().color = new Color(1f, 0.78f, 0.05f);
        startBtnGO.AddComponent<Button>();
        var startCG = startBtnGO.AddComponent<CanvasGroup>();

        var startTextGO = CreateTMPText(startBtnGO.transform, "StartText", "START GAME",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            28f, new Color(0.08f, 0.05f, 0f), FontStyles.Bold);

        var so = new SerializedObject(betUI);
        so.FindProperty("rootGroup").objectReferenceValue = rootCG;
        so.FindProperty("rootRect").objectReferenceValue = canvasGO.GetComponent<RectTransform>();
        so.FindProperty("headerRect").objectReferenceValue = headerRT;
        so.FindProperty("balanceLabel").objectReferenceValue = balanceLabelGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("balanceValue").objectReferenceValue = balanceValueGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("betDisplayRect").objectReferenceValue = betDisplayRT;
        so.FindProperty("betLabel").objectReferenceValue = betLabelGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("betValue").objectReferenceValue = betValueGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("betDisplayBg").objectReferenceValue = betDisplayBg;
        so.FindProperty("decreaseBtn").objectReferenceValue = decreaseBtnGO.GetComponent<Button>();
        so.FindProperty("decreaseBtnRect").objectReferenceValue = decreaseBtnGO.GetComponent<RectTransform>();
        so.FindProperty("increaseBtn").objectReferenceValue = increaseBtnGO.GetComponent<Button>();
        so.FindProperty("increaseBtnRect").objectReferenceValue = increaseBtnGO.GetComponent<RectTransform>();
        so.FindProperty("minBtn").objectReferenceValue = minBtnGO.GetComponent<Button>();
        so.FindProperty("halfBtn").objectReferenceValue = halfBtnGO.GetComponent<Button>();
        so.FindProperty("maxBtn").objectReferenceValue = maxBtnGO.GetComponent<Button>();
        so.FindProperty("presetRow").objectReferenceValue = presetRT;
        so.FindProperty("startBtn").objectReferenceValue = startBtnGO.GetComponent<Button>();
        so.FindProperty("startBtnRect").objectReferenceValue = startRT;
        so.FindProperty("startBtnGroup").objectReferenceValue = startCG;
        so.FindProperty("startBtnText").objectReferenceValue = startTextGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("startBtnImage").objectReferenceValue = startBtnGO.GetComponent<Image>();
        so.FindProperty("sliderFill").objectReferenceValue = sliderFillRT;
        so.FindProperty("sliderTrack").objectReferenceValue = sliderTrackRT;
        so.ApplyModifiedProperties();

        Debug.Log("[Iteration 2] BetScreenUI setup complete!");
    }

    private static GameObject CreateCircleDecor(Transform parent)
    {
        var go = new GameObject("DecorCircle");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 60f);
        rt.sizeDelta = new Vector2(360f, 360f);
        var img = go.AddComponent<Image>();

        string path = "Assets/RingGame/Textures/RingSprite_0.png";
        if (File.Exists(path))
            img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        img.color = new Color(0.4f, 0.5f, 1f, 0.06f);
        img.raycastTarget = false;
        return go;
    }

    private static GameObject CreateRoundButton(Transform parent, string name, string label,
        Vector2 pos, float size, Color bgColor, Color textColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(size, size);
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
        tmp.fontSize = 32;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = textColor;

        return go;
    }

    private static GameObject CreatePresetButton(Transform parent, string name, string label, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(88f, 36f);
        go.AddComponent<Image>().color = new Color(0.18f, 0.15f, 0.3f);
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
        tmp.fontSize = 16;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.75f, 0.78f, 1f);

        return go;
    }

    private static GameObject CreateTMPText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        float fontSize, Color color, FontStyles style = FontStyles.Normal)
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
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        return go;
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
}
