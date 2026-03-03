using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class Iteration5_Setup : EditorWindow
{
    [MenuItem("RingGame/Iteration 5/Setup Result Screen")]
    public static void SetupResultScreen()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string scenePath = "Assets/RingGame/Scenes/Game.unity";
        if (!File.Exists(scenePath)) { Debug.LogError("[Iter5] Game.unity not found!"); return; }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Debug.Log("[Iter5] ══════════════════════════════");
        Debug.Log("[Iter5] Setting up Result Screen...");

        CreateResultScreenController();
        CreateResultScreenCanvas();
        WireResultScreenUI();
        WireRhythmPhaseController();

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[Iter5] ══════════════════════════════");
        Debug.Log("[Iter5] Done! Result Screen ready.");
    }

    [MenuItem("RingGame/Iteration 5/Validate & Fix")]
    public static void Validate()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string scenePath = "Assets/RingGame/Scenes/Game.unity";
        if (!File.Exists(scenePath)) { Debug.LogError("[Iter5] Game.unity not found!"); return; }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        int fixed_ = 0;
        Debug.Log("[Iter5 Validate] ══════════════════════════════");

        var rsc = Object.FindObjectOfType<ResultScreenController>(true);
        if (rsc == null) { Debug.LogWarning("[Iter5] ResultScreenController MISSING — run Setup first."); }
        else Debug.Log("[Iter5] ✓ ResultScreenController");

        var rsui = Object.FindObjectOfType<ResultScreenUI>(true);
        if (rsui == null) { Debug.LogWarning("[Iter5] ResultScreenUI MISSING — run Setup first."); }
        else
        {
            Debug.Log("[Iter5] ✓ ResultScreenUI");
            var so = new SerializedObject(rsui);
            var cfg = AssetDatabase.LoadAssetAtPath<SymbolConfig>("Assets/RingGame/Data/SymbolConfig.asset");
            if (so.FindProperty("symbolConfig").objectReferenceValue == null && cfg != null)
            {
                so.FindProperty("symbolConfig").objectReferenceValue = cfg;
                so.ApplyModifiedProperties();
                Debug.Log("[Iter5] 🔧 symbolConfig assigned to ResultScreenUI.");
                fixed_++;
            }
            else if (cfg != null) Debug.Log("[Iter5] ✓ ResultScreenUI.symbolConfig");
        }

        if (rsc != null)
        {
            var so = new SerializedObject(rsc);
            if (so.FindProperty("resultScreenUI").objectReferenceValue == null && rsui != null)
            {
                so.FindProperty("resultScreenUI").objectReferenceValue = rsui;
                so.ApplyModifiedProperties();
                Debug.Log("[Iter5] 🔧 ResultScreenController.resultScreenUI reassigned.");
                fixed_++;
            }
            else Debug.Log("[Iter5] ✓ ResultScreenController.resultScreenUI");
        }

        var rhythmCtrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (rhythmCtrl != null)
        {
            var so = new SerializedObject(rhythmCtrl);
            if (so.FindProperty("rhythmPhaseUI").objectReferenceValue == null)
            {
                var ui = Object.FindObjectOfType<RhythmPhaseUI>(true);
                if (ui != null)
                {
                    so.FindProperty("rhythmPhaseUI").objectReferenceValue = ui;
                    so.ApplyModifiedProperties();
                    Debug.Log("[Iter5] 🔧 RhythmPhaseController.rhythmPhaseUI reassigned.");
                    fixed_++;
                }
            }
            else Debug.Log("[Iter5] ✓ RhythmPhaseController.rhythmPhaseUI");
        }

        var rsCanvas = GameObject.Find("ResultScreenCanvas");
        if (rsCanvas != null && rsCanvas.activeSelf)
        {
            rsCanvas.SetActive(false);
            Debug.Log("[Iter5] 🔧 ResultScreenCanvas was active — set inactive.");
            fixed_++;
        }
        else if (rsCanvas != null) Debug.Log("[Iter5] ✓ ResultScreenCanvas inactive");

        if (fixed_ > 0) EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[Iter5 Validate] Fixed {fixed_} issue(s).");
        Debug.Log("[Iter5 Validate] ══════════════════════════════");
    }

    static void CreateResultScreenController()
    {
        var existing = Object.FindObjectOfType<ResultScreenController>(true);
        if (existing != null) { Debug.Log("[Iter5] ResultScreenController already exists."); return; }

        var go = new GameObject("ResultScreenController");
        go.AddComponent<ResultScreenController>();
        Debug.Log("[Iter5] ✓ ResultScreenController created.");
    }

    static void CreateResultScreenCanvas()
    {
        if (GameObject.Find("ResultScreenCanvas") != null)
        {
            Debug.Log("[Iter5] ResultScreenCanvas already exists.");
            return;
        }

        var canvasGO = new GameObject("ResultScreenCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        var cg = canvasGO.AddComponent<CanvasGroup>();

        // BG Panel
        var bgPanelGO = CreateImage(canvasGO.transform, "BgPanel",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0.04f, 0.04f, 0.1f, 0.95f));
        bgPanelGO.GetComponent<Image>().raycastTarget = true;

        // Flash Overlay
        var flashGO = CreateImage(canvasGO.transform, "FlashOverlay",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(1f, 1f, 1f, 0f));
        flashGO.GetComponent<Image>().raycastTarget = false;

        // Result Label
        var resultLabelGO = CreateTMP(canvasGO.transform, "ResultLabel",
            new Vector2(0f, 680f), new Vector2(800f, 120f),
            "WIN!", 72f, FontStyles.Bold, WinColor());
        resultLabelGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Multiplier
        var multGO = CreateTMP(canvasGO.transform, "MultiplierText",
            new Vector2(0f, 590f), new Vector2(400f, 60f),
            "x2.0", 38f, FontStyles.Normal, new Color(1f, 0.82f, 0.15f));
        multGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Payout Container
        var payoutContainerGO = new GameObject("PayoutContainer");
        payoutContainerGO.transform.SetParent(canvasGO.transform, false);
        var payoutRT = payoutContainerGO.AddComponent<RectTransform>();
        payoutRT.anchorMin = new Vector2(0.5f, 0.5f);
        payoutRT.anchorMax = new Vector2(0.5f, 0.5f);
        payoutRT.pivot = new Vector2(0.5f, 0.5f);
        payoutRT.anchoredPosition = new Vector2(0f, 500f);
        payoutRT.sizeDelta = new Vector2(600f, 100f);

        var payoutTextGO = CreateTMP(payoutContainerGO.transform, "PayoutText",
            Vector2.zero, new Vector2(600f, 100f),
            "+$0", 64f, FontStyles.Bold, WinColor());
        payoutTextGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Symbols Row
        var symbolsRowGO = new GameObject("SymbolsRow");
        symbolsRowGO.transform.SetParent(canvasGO.transform, false);
        var symbolsRT = symbolsRowGO.AddComponent<RectTransform>();
        symbolsRT.anchorMin = new Vector2(0.5f, 0.5f);
        symbolsRT.anchorMax = new Vector2(0.5f, 0.5f);
        symbolsRT.pivot = new Vector2(0.5f, 0.5f);
        symbolsRT.anchoredPosition = new Vector2(0f, 80f);
        symbolsRT.sizeDelta = new Vector2(900f, 140f);
        var hlg = symbolsRowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 18f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        var slots = new List<Image>();
        var bgs = new List<Image>();
        var labels = new List<TextMeshProUGUI>();

        for (int i = 0; i < 8; i++)
        {
            var slotGO = new GameObject($"Slot_{i}");
            slotGO.transform.SetParent(symbolsRowGO.transform, false);
            var slotRT = slotGO.AddComponent<RectTransform>();
            slotRT.sizeDelta = new Vector2(92f, 120f);

            var bgGO = new GameObject("Bg");
            bgGO.transform.SetParent(slotGO.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0.15f);
            bgRT.anchorMax = new Vector2(1f, 1f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);
            bgImg.raycastTarget = false;
            bgs.Add(bgImg);

            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(bgGO.transform, false);
            var iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.1f, 0.1f);
            iconRT.anchorMax = new Vector2(0.9f, 0.9f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            iconImg.color = new Color(1f, 1f, 1f, 0.15f);
            slots.Add(iconImg);

            var lblGO = CreateTMP(slotGO.transform, "Label",
                new Vector2(0f, -48f), new Vector2(92f, 30f),
                "—", 18f, FontStyles.Bold, new Color(0.7f, 0.7f, 0.9f));
            var lbl = lblGO.GetComponent<TextMeshProUGUI>();
            lbl.alignment = TextAlignmentOptions.Center;
            labels.Add(lbl);
        }

        // Separator line
        var sepGO = CreateImage(canvasGO.transform, "Separator",
            new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.5f),
            new Vector2(0f, -28f), new Vector2(0f, -26f),
            new Color(1f, 1f, 1f, 0.12f));

        // Balance & Bet
        var balanceGO = CreateTMP(canvasGO.transform, "BalanceText",
            new Vector2(0f, -160f), new Vector2(700f, 50f),
            "BALANCE  $0", 28f, FontStyles.Normal, new Color(0.8f, 0.8f, 1f));
        balanceGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var betTextGO = CreateTMP(canvasGO.transform, "BetText",
            new Vector2(0f, -210f), new Vector2(700f, 40f),
            "BET  $0", 24f, FontStyles.Normal, new Color(0.6f, 0.6f, 0.8f));
        betTextGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Buttons Row
        var btnsRowGO = new GameObject("ButtonsRow");
        btnsRowGO.transform.SetParent(canvasGO.transform, false);
        var btnsRT = btnsRowGO.AddComponent<RectTransform>();
        btnsRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnsRT.anchorMax = new Vector2(0.5f, 0.5f);
        btnsRT.pivot = new Vector2(0.5f, 0.5f);
        btnsRT.anchoredPosition = new Vector2(0f, -380f);
        btnsRT.sizeDelta = new Vector2(700f, 90f);
        var btnsHLG = btnsRowGO.AddComponent<HorizontalLayoutGroup>();
        btnsHLG.spacing = 24f;
        btnsHLG.childAlignment = TextAnchor.MiddleCenter;
        btnsHLG.childForceExpandWidth = false;
        btnsHLG.childForceExpandHeight = false;

        var playAgainBtn = CreateButton(btnsRowGO.transform, "PlayAgainBtn",
            new Vector2(310f, 80f), "PLAY AGAIN",
            new Color(0.25f, 1f, 0.55f), new Color(0.02f, 0.08f, 0.04f));

        var menuBtn = CreateButton(btnsRowGO.transform, "MenuBtn",
            new Vector2(200f, 80f), "MENU",
            new Color(0.3f, 0.35f, 0.55f), Color.white);

        // ResultScreenUI component
        var rsUI = canvasGO.AddComponent<ResultScreenUI>();
        var so = new SerializedObject(rsUI);

        so.FindProperty("rootGroup").objectReferenceValue = cg;
        so.FindProperty("resultLabel").objectReferenceValue = resultLabelGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("resultLabelRect").objectReferenceValue = resultLabelGO.GetComponent<RectTransform>();
        so.FindProperty("payoutText").objectReferenceValue = payoutTextGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("multiplierText").objectReferenceValue = multGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("payoutContainer").objectReferenceValue = payoutContainerGO.GetComponent<RectTransform>();
        so.FindProperty("symbolsRow").objectReferenceValue = symbolsRowGO.GetComponent<RectTransform>();
        so.FindProperty("balanceText").objectReferenceValue = balanceGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("betText").objectReferenceValue = betTextGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("buttonsRow").objectReferenceValue = btnsRowGO.GetComponent<RectTransform>();
        so.FindProperty("flashOverlay").objectReferenceValue = flashGO.GetComponent<Image>();
        so.FindProperty("bgPanel").objectReferenceValue = bgPanelGO.GetComponent<Image>();
        so.FindProperty("playAgainBtn").objectReferenceValue = playAgainBtn;
        so.FindProperty("menuBtn").objectReferenceValue = menuBtn;

        var slotsProp = so.FindProperty("symbolSlots");
        slotsProp.arraySize = slots.Count;
        for (int i = 0; i < slots.Count; i++)
            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];

        var bgsProp = so.FindProperty("symbolBgs");
        bgsProp.arraySize = bgs.Count;
        for (int i = 0; i < bgs.Count; i++)
            bgsProp.GetArrayElementAtIndex(i).objectReferenceValue = bgs[i];

        var labelsProp = so.FindProperty("symbolLabels");
        labelsProp.arraySize = labels.Count;
        for (int i = 0; i < labels.Count; i++)
            labelsProp.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];

        var cfg = AssetDatabase.LoadAssetAtPath<SymbolConfig>("Assets/RingGame/Data/SymbolConfig.asset");
        if (cfg != null) so.FindProperty("symbolConfig").objectReferenceValue = cfg;

        so.ApplyModifiedProperties();

        canvasGO.SetActive(false);
        Debug.Log("[Iter5] ✓ ResultScreenCanvas created with all UI elements.");
    }

    static void WireResultScreenUI()
    {
        var rsc = Object.FindObjectOfType<ResultScreenController>(true);
        if (rsc == null) return;

        var rsUI = Object.FindObjectOfType<ResultScreenUI>(true);
        if (rsUI == null) return;

        var so = new SerializedObject(rsc);
        so.FindProperty("resultScreenUI").objectReferenceValue = rsUI;
        so.ApplyModifiedProperties();
        Debug.Log("[Iter5] ✓ ResultScreenController wired to ResultScreenUI.");
    }

    static void WireRhythmPhaseController()
    {
        var ctrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (ctrl == null) return;

        var so = new SerializedObject(ctrl);
        if (so.FindProperty("rhythmPhaseUI").objectReferenceValue == null)
        {
            var ui = Object.FindObjectOfType<RhythmPhaseUI>(true);
            if (ui != null)
            {
                so.FindProperty("rhythmPhaseUI").objectReferenceValue = ui;
                so.ApplyModifiedProperties();
                Debug.Log("[Iter5] 🔧 RhythmPhaseController.rhythmPhaseUI reassigned.");
            }
        }
    }

    // ── UI factory helpers ─────────────────────────────────────────────────

    static GameObject CreateImage(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return go;
    }

    static GameObject CreateTMP(Transform parent, string name, Vector2 anchoredPos, Vector2 size,
        string text, float fontSize, FontStyles style, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.raycastTarget = false;
        return go;
    }

    static Button CreateButton(Transform parent, string name, Vector2 size, string label,
        Color bgColor, Color textColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();

        var colors = btn.colors;
        colors.highlightedColor = new Color(bgColor.r * 1.2f, bgColor.g * 1.2f, bgColor.b * 1.2f);
        colors.pressedColor = new Color(bgColor.r * 0.8f, bgColor.g * 0.8f, bgColor.b * 0.8f);
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
        tmp.fontSize = 28f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        return btn;
    }

    static Color WinColor() => new Color(1f, 0.82f, 0.15f);
}
