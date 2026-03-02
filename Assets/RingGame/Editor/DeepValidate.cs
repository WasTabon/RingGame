using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class DeepValidate : EditorWindow
{
    [MenuItem("RingGame/Deep Validate & Fix Scene")]
    public static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string scenePath = "Assets/RingGame/Scenes/Game.unity";
        if (!File.Exists(scenePath))
        {
            Debug.LogError("[DeepValidate] Game.unity not found!");
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        int fixed_ = 0;

        Debug.Log("[DeepValidate] ════════════════════════════════");
        Debug.Log("[DeepValidate] Starting deep validation...");
        Debug.Log("[DeepValidate] ════════════════════════════════");

        fixed_ += CheckSingletons();
        fixed_ += CheckControllerPlacement();
        fixed_ += CheckCanvasState();
        fixed_ += CheckRingsManagerPlacement();
        fixed_ += CheckRingsManagerRefs();
        fixed_ += CheckRhythmPhaseUIRefs();
        fixed_ += CheckShrinkingRing();
        fixed_ += CheckFeedbackElements();
        fixed_ += CheckBeatSequencer();
        fixed_ += CheckTapInputHandler();
        fixed_ += CheckRaycastTargets();
        fixed_ += CheckBuildSettings();

        Debug.Log("[DeepValidate] ════════════════════════════════");
        if (fixed_ > 0)
        {
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[DeepValidate] Done. Fixed {fixed_} issue(s). Scene saved.");
        }
        else
        {
            Debug.Log("[DeepValidate] ✅ No issues found.");
        }
        Debug.Log("[DeepValidate] ════════════════════════════════");
    }

    // ── helpers ────────────────────────────────────────────────────────────

    static bool ReallyExists(Object obj) =>
        obj != null && (obj is Component c ? c.gameObject != null : true);

    static T GetProp<T>(SerializedObject so, string name) where T : Object =>
        so.FindProperty(name).objectReferenceValue as T;

    static bool PropReal<T>(SerializedObject so, string name) where T : Object =>
        ReallyExists(so.FindProperty(name).objectReferenceValue as T);

    static void SetProp(SerializedObject so, string name, Object value)
    {
        so.FindProperty(name).objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }

    static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var f = FindDeepChild(child, name);
            if (f != null) return f;
        }
        return null;
    }

    static Sprite LoadOrGenerateRingSprite()
    {
        string path = "Assets/RingGame/Textures/RingSprite_0.png";
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;

        EnsureFolder("Assets/RingGame/Textures");
        int size = 256; float thick = 14f;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c = size / 2f, outerR = c - 2f, innerR = outerR - thick;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                float t = (dist >= innerR && dist <= outerR)
                    ? 1f - Mathf.Abs((dist - (innerR + outerR) * .5f) / ((outerR - innerR) * .5f))
                    : 0f;
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(t * 1.5f)));
            }
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null) { ti.textureType = TextureImporterType.Sprite; ti.alphaIsTransparency = true; ti.SaveAndReimport(); }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }

    // ── checks ─────────────────────────────────────────────────────────────

    static int CheckSingletons()
    {
        int fixed_ = 0;
        var types = new System.Type[]
        {
            typeof(BalanceManager), typeof(BetManager), typeof(BeatSequencer),
            typeof(TapInputHandler), typeof(RingsManager), typeof(RhythmPhaseController),
            typeof(RhythmPhaseUI), typeof(BetScreenUI), typeof(EventSystem)
        };
        foreach (var t in types)
        {
            var all = Object.FindObjectsOfType(t, true);
            if (all.Length == 0)
                Debug.LogWarning($"[DeepValidate] ⚠️  {t.Name} NOT FOUND — run the appropriate Setup script.");
            else if (all.Length > 1)
            {
                for (int i = 1; i < all.Length; i++)
                    Object.DestroyImmediate((all[i] as Component)?.gameObject);
                Debug.Log($"[DeepValidate] 🔧 {t.Name}: removed {all.Length - 1} duplicate(s).");
                fixed_++;
            }
            else
                Debug.Log($"[DeepValidate] ✓  {t.Name}");
        }
        return fixed_;
    }

    static int CheckControllerPlacement()
    {
        int fixed_ = 0;
        var ctrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (ctrl == null) return 0;

        if (ctrl.GetComponentInParent<Canvas>() != null)
        {
            ctrl.transform.SetParent(null);
            Debug.Log("[DeepValidate] 🔧 RhythmPhaseController was inside Canvas — moved to scene root.");
            fixed_++;
        }
        else Debug.Log("[DeepValidate] ✓  RhythmPhaseController at scene root");

        if (!ctrl.gameObject.activeSelf)
        {
            ctrl.gameObject.SetActive(true);
            Debug.Log("[DeepValidate] 🔧 RhythmPhaseController was inactive — activated.");
            fixed_++;
        }
        return fixed_;
    }

    static int CheckCanvasState()
    {
        int fixed_ = 0;
        var rhythmUI = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (rhythmUI != null)
        {
            var canvas = rhythmUI.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.gameObject.activeSelf)
            {
                canvas.gameObject.SetActive(false);
                Debug.Log("[DeepValidate] 🔧 RhythmPhaseCanvas was active — set inactive.");
                fixed_++;
            }
            else Debug.Log("[DeepValidate] ✓  RhythmPhaseCanvas inactive");
        }

        var betUI = Object.FindObjectOfType<BetScreenUI>(true);
        if (betUI != null)
        {
            var canvas = betUI.GetComponentInParent<Canvas>();
            if (canvas != null && !canvas.gameObject.activeSelf)
            {
                canvas.gameObject.SetActive(true);
                Debug.Log("[DeepValidate] 🔧 BetScreenCanvas was inactive — activated.");
                fixed_++;
            }
            else Debug.Log("[DeepValidate] ✓  BetScreenCanvas active");
        }
        return fixed_;
    }

    static int CheckRingsManagerPlacement()
    {
        int fixed_ = 0;
        var rm = Object.FindObjectOfType<RingsManager>(true);
        if (rm == null) return 0;

        var rhythmCanvas = Object.FindObjectOfType<RhythmPhaseUI>(true)?.GetComponentInParent<Canvas>();
        if (rhythmCanvas == null) return 0;

        if (rm.GetComponentInParent<Canvas>() != rhythmCanvas)
        {
            rm.transform.SetParent(rhythmCanvas.transform, false);
            Debug.Log("[DeepValidate] 🔧 RingsManager was outside RhythmPhaseCanvas — moved inside.");
            fixed_++;
        }
        else Debug.Log("[DeepValidate] ✓  RingsManager inside RhythmPhaseCanvas");
        return fixed_;
    }

    static int CheckRingsManagerRefs()
    {
        int fixed_ = 0;
        var rm = Object.FindObjectOfType<RingsManager>(true);
        if (rm == null) return 0;
        var so = new SerializedObject(rm);

        if (!PropReal<RectTransform>(so, "ringsContainer"))
        {
            var t = FindDeepChild(rm.transform.root, "RingsContainer");
            if (t == null)
            {
                var go = new GameObject("RingsContainer");
                go.transform.SetParent(rm.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(500f, 500f);
                go.AddComponent<CanvasGroup>();
                SetProp(so, "ringsContainer", rt);
                Debug.Log("[DeepValidate] 🔧 RingsContainer created and assigned.");
            }
            else
            {
                SetProp(so, "ringsContainer", t.GetComponent<RectTransform>());
                Debug.Log("[DeepValidate] 🔧 ringsContainer ref was broken — reassigned.");
            }
            fixed_++;
        }
        else Debug.Log("[DeepValidate] ✓  RingsManager.ringsContainer");

        if (!PropReal<Sprite>(so, "ringSprite"))
        {
            var sp = LoadOrGenerateRingSprite();
            if (sp != null) { SetProp(so, "ringSprite", sp); fixed_++; Debug.Log("[DeepValidate] 🔧 ringSprite reassigned."); }
        }
        else Debug.Log("[DeepValidate] ✓  RingsManager.ringSprite");

        if (!PropReal<Sprite>(so, "symbolBgSprite"))
        {
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/RingGame/Textures/SymbolBg.png");
            if (sp != null) { SetProp(so, "symbolBgSprite", sp); fixed_++; Debug.Log("[DeepValidate] 🔧 symbolBgSprite reassigned."); }
        }
        else Debug.Log("[DeepValidate] ✓  RingsManager.symbolBgSprite");

        if (!PropReal<SymbolConfig>(so, "symbolConfig"))
        {
            var cfg = AssetDatabase.LoadAssetAtPath<SymbolConfig>("Assets/RingGame/Data/SymbolConfig.asset");
            if (cfg != null) { SetProp(so, "symbolConfig", cfg); fixed_++; Debug.Log("[DeepValidate] 🔧 symbolConfig reassigned."); }
            else Debug.LogWarning("[DeepValidate] ⚠️  SymbolConfig not found. Run: RingGame → Iteration 3 → Create Symbol Config Asset");
        }
        else Debug.Log("[DeepValidate] ✓  RingsManager.symbolConfig");

        return fixed_;
    }

    static int CheckRhythmPhaseUIRefs()
    {
        int fixed_ = 0;
        var ui = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (ui == null) return 0;
        var so = new SerializedObject(ui);
        var canvas = ui.GetComponentInParent<Canvas>();
        if (canvas == null) return 0;

        if (!PropReal<CanvasGroup>(so, "rootGroup"))
        {
            var cg = canvas.GetComponent<CanvasGroup>() ?? canvas.gameObject.AddComponent<CanvasGroup>();
            SetProp(so, "rootGroup", cg); fixed_++;
            Debug.Log("[DeepValidate] 🔧 rootGroup reassigned.");
        }
        else Debug.Log("[DeepValidate] ✓  RhythmPhaseUI.rootGroup");

        void TryReassignRT(string propName, string childName)
        {
            if (!PropReal<RectTransform>(so, propName))
            {
                var t = FindDeepChild(canvas.transform, childName);
                if (t != null) { SetProp(so, propName, t.GetComponent<RectTransform>()); fixed_++; Debug.Log($"[DeepValidate] 🔧 {propName} reassigned."); }
                else Debug.LogWarning($"[DeepValidate] ⚠️  {propName} is broken and {childName} not found in canvas.");
            }
            else Debug.Log($"[DeepValidate] ✓  RhythmPhaseUI.{propName}");
        }

        TryReassignRT("topBar", "TopBar");
        TryReassignRT("ringsArea", "RingsArea");
        TryReassignRT("gridArea", "GridArea");
        TryReassignRT("attemptsRow", "AttemptsRow");

        var ctrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (ctrl != null)
        {
            var cso = new SerializedObject(ctrl);
            if (!PropReal<RhythmPhaseUI>(cso, "rhythmPhaseUI"))
            {
                SetProp(cso, "rhythmPhaseUI", ui); fixed_++;
                Debug.Log("[DeepValidate] 🔧 RhythmPhaseController.rhythmPhaseUI reassigned.");
            }
            else Debug.Log("[DeepValidate] ✓  RhythmPhaseController.rhythmPhaseUI");
        }

        return fixed_;
    }

    static int CheckShrinkingRing()
    {
        int fixed_ = 0;
        var ui = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (ui == null) return 0;
        var so = new SerializedObject(ui);
        var canvas = ui.GetComponentInParent<Canvas>();
        if (canvas == null) return 0;

        bool ringReal = PropReal<RectTransform>(so, "shrinkingRing");
        bool imgReal  = PropReal<Image>(so, "shrinkingRingImage");

        var existing = FindDeepChild(canvas.transform, "ShrinkingRing");

        if (!ringReal || !imgReal || existing == null)
        {
            if (existing == null)
            {
                var go = new GameObject("ShrinkingRing");
                go.transform.SetParent(canvas.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(300f, 300f);
                var img = go.AddComponent<Image>();
                img.raycastTarget = false;
                var sprite = LoadOrGenerateRingSprite();
                if (sprite != null) img.sprite = sprite;
                img.color = new Color(1f, 0.92f, 0.25f, 0.9f);
                go.SetActive(false);
                existing = go.transform;
                Debug.Log("[DeepValidate] 🔧 ShrinkingRing object created.");
            }
            existing.SetAsLastSibling();
            SetProp(so, "shrinkingRing", existing.GetComponent<RectTransform>());
            SetProp(so, "shrinkingRingImage", existing.GetComponent<Image>());
            Debug.Log("[DeepValidate] 🔧 ShrinkingRing refs assigned.");
            fixed_++;
        }
        else
        {
            var img = existing.GetComponent<Image>();
            if (img != null && img.sprite == null)
            {
                img.sprite = LoadOrGenerateRingSprite();
                Debug.Log("[DeepValidate] 🔧 ShrinkingRing had no sprite — assigned.");
                fixed_++;
            }
            existing.SetAsLastSibling();
            Debug.Log("[DeepValidate] ✓  ShrinkingRing exists and refs assigned");
        }
        return fixed_;
    }

    static int CheckFeedbackElements()
    {
        int fixed_ = 0;
        var ui = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (ui == null) return 0;
        var so = new SerializedObject(ui);
        var canvas = ui.GetComponentInParent<Canvas>();
        if (canvas == null) return 0;

        // FeedbackText
        if (!PropReal<TextMeshProUGUI>(so, "feedbackText"))
        {
            var existing = FindDeepChild(canvas.transform, "FeedbackText");
            if (existing == null)
            {
                var go = new GameObject("FeedbackText");
                go.transform.SetParent(canvas.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 80f);
                rt.sizeDelta = new Vector2(200f, 60f);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 42f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(1f, 1f, 1f, 0f);
                tmp.raycastTarget = false;
                existing = go.transform;
                Debug.Log("[DeepValidate] 🔧 FeedbackText created.");
            }
            SetProp(so, "feedbackText", existing.GetComponent<TextMeshProUGUI>());
            SetProp(so, "feedbackRect", existing.GetComponent<RectTransform>());
            fixed_++;
        }
        else Debug.Log("[DeepValidate] ✓  feedbackText assigned");

        // FlashOverlay
        if (!PropReal<Image>(so, "flashOverlay"))
        {
            var existing = FindDeepChild(canvas.transform, "FlashOverlay");
            if (existing == null)
            {
                var go = new GameObject("FlashOverlay");
                go.transform.SetParent(canvas.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                var img = go.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0f);
                img.raycastTarget = false;
                existing = go.transform;
                Debug.Log("[DeepValidate] 🔧 FlashOverlay created.");
            }
            SetProp(so, "flashOverlay", existing.GetComponent<Image>());
            fixed_++;
        }
        else Debug.Log("[DeepValidate] ✓  flashOverlay assigned");

        return fixed_;
    }

    static int CheckBeatSequencer()
    {
        int fixed_ = 0;
        var bs = Object.FindObjectOfType<BeatSequencer>(true);
        if (bs == null)
        {
            new GameObject("BeatSequencer").AddComponent<BeatSequencer>();
            Debug.Log("[DeepValidate] 🔧 BeatSequencer created.");
            fixed_++;
        }
        else if (bs.GetComponentInParent<Canvas>() != null)
        {
            bs.transform.SetParent(null);
            Debug.Log("[DeepValidate] 🔧 BeatSequencer moved to scene root.");
            fixed_++;
        }
        else Debug.Log("[DeepValidate] ✓  BeatSequencer");
        return fixed_;
    }

    static int CheckTapInputHandler()
    {
        int fixed_ = 0;
        var ti = Object.FindObjectOfType<TapInputHandler>(true);
        if (ti == null)
        {
            new GameObject("TapInputHandler").AddComponent<TapInputHandler>();
            Debug.Log("[DeepValidate] 🔧 TapInputHandler created.");
            fixed_++;
        }
        else if (ti.GetComponentInParent<Canvas>() != null)
        {
            ti.transform.SetParent(null);
            Debug.Log("[DeepValidate] 🔧 TapInputHandler moved to scene root.");
            fixed_++;
        }
        else Debug.Log("[DeepValidate] ✓  TapInputHandler");
        return fixed_;
    }

    static int CheckRaycastTargets()
    {
        int fixed_ = 0;
        var rhythmUI = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (rhythmUI == null) return 0;
        var canvas = rhythmUI.GetComponentInParent<Canvas>();
        if (canvas == null) return 0;

        var images = canvas.GetComponentsInChildren<Image>(true);
        var blocked = new List<string>();
        foreach (var img in images)
        {
            if (!img.raycastTarget) continue;
            if (img.GetComponent<Button>() != null) continue;
            if (img.GetComponentInParent<Button>() != null) continue;
            img.raycastTarget = false;
            blocked.Add(img.gameObject.name);
        }
        if (blocked.Count > 0)
        {
            Debug.Log($"[DeepValidate] 🔧 Disabled raycastTarget on {blocked.Count} non-interactive Image(s) in RhythmPhaseCanvas (were blocking taps): {string.Join(", ", blocked)}");
            fixed_++;
        }
        else Debug.Log("[DeepValidate] ✓  No tap-blocking raycastTargets in RhythmPhaseCanvas");
        return fixed_;
    }

    static int CheckBuildSettings()
    {
        int fixed_ = 0;
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string mainPath = "Assets/RingGame/Scenes/MainMenu.unity";
        string gamePath = "Assets/RingGame/Scenes/Game.unity";
        if (!list.Exists(s => s.path == mainPath) && File.Exists(mainPath))
        {
            list.Insert(0, new EditorBuildSettingsScene(mainPath, true));
            Debug.Log("[DeepValidate] 🔧 MainMenu added to Build Settings.");
            fixed_++;
        }
        if (!list.Exists(s => s.path == gamePath) && File.Exists(gamePath))
        {
            list.Add(new EditorBuildSettingsScene(gamePath, true));
            Debug.Log("[DeepValidate] 🔧 Game added to Build Settings.");
            fixed_++;
        }
        if (fixed_ > 0) EditorBuildSettings.scenes = list.ToArray();
        else Debug.Log("[DeepValidate] ✓  Build Settings");
        return fixed_;
    }
}