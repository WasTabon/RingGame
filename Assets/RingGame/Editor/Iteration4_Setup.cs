using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

public class Iteration4_Setup : EditorWindow
{
    [MenuItem("RingGame/Iteration 4/Validate & Fix Scene")]
    public static void ValidateAndFix()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string scenePath = "Assets/RingGame/Scenes/Game.unity";
        if (!File.Exists(scenePath))
        {
            Debug.LogError("[Validate I4] Game.unity not found!");
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        bool anyFixed = false;

        Debug.Log("[Validate I4] ℹ️ GameManager & AudioManager live on MainMenu scene — skipping.");

        anyFixed |= ValidateSingleton<BalanceManager>("BalanceManager");
        anyFixed |= ValidateSingleton<BetManager>("BetManager");
        anyFixed |= ValidateSingleton<BeatSequencer>("BeatSequencer");
        anyFixed |= ValidateSingleton<TapInputHandler>("TapInputHandler");
        anyFixed |= ValidateSingleton<RingsManager>("RingsManager");
        anyFixed |= ValidateSingleton<RhythmPhaseController>("RhythmPhaseController");
        anyFixed |= ValidateSingleton<RhythmPhaseUI>("RhythmPhaseUI");
        anyFixed |= ValidateSingleton<BetScreenUI>("BetScreenUI");
        anyFixed |= ValidateSingleton<EventSystem>("EventSystem");

        anyFixed |= ValidateControllerNotInCanvas();
        anyFixed |= ValidateRhythmCanvasInactive();
        anyFixed |= ValidateRhythmControllerRef();
        anyFixed |= ValidateShrinkingRingRef();
        anyFixed |= ValidateNullWarnings<BetScreenUI>("BetScreenUI");
        anyFixed |= ValidateNullWarnings<RhythmPhaseUI>("RhythmPhaseUI");
        anyFixed |= ValidateBuildSettings();

        if (anyFixed)
        {
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("[Validate I4] ✅ Issues fixed and scene saved.");
        }
        else
        {
            Debug.Log("[Validate I4] ✅ Everything looks good!");
        }
    }

    [MenuItem("RingGame/Iteration 4/Setup Game Scene - Rhythm Mechanics")]
    public static void SetupGameScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string scenePath = "Assets/RingGame/Scenes/Game.unity";
        if (!File.Exists(scenePath))
        {
            Debug.LogError("[Iteration 4] Game.unity not found! Run Iteration 1 first.");
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        EnsureEventSystem();
        SetupBeatSequencer();
        SetupTapInputHandler();
        SetupShrinkingRing();
        SetupFeedbackElements();
        EnsureControllerOutsideCanvas();

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[Iteration 4] Game scene updated with Rhythm Mechanics!");
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>(true) != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    private static void SetupBeatSequencer()
    {
        if (Object.FindObjectOfType<BeatSequencer>(true) != null)
        {
            Debug.Log("[Iteration 4] BeatSequencer already exists.");
            return;
        }
        new GameObject("BeatSequencer").AddComponent<BeatSequencer>();
        Debug.Log("[Iteration 4] BeatSequencer created.");
    }

    private static void SetupTapInputHandler()
    {
        if (Object.FindObjectOfType<TapInputHandler>(true) != null)
        {
            Debug.Log("[Iteration 4] TapInputHandler already exists.");
            return;
        }
        new GameObject("TapInputHandler").AddComponent<TapInputHandler>();
        Debug.Log("[Iteration 4] TapInputHandler created.");
    }

    private static void SetupShrinkingRing()
    {
        var phaseUI = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (phaseUI == null)
        {
            Debug.LogWarning("[Iteration 4] RhythmPhaseUI not found — run Iteration 3 setup first.");
            return;
        }

        var uiSO = new SerializedObject(phaseUI);
        if (uiSO.FindProperty("shrinkingRing").objectReferenceValue != null)
        {
            Debug.Log("[Iteration 4] ShrinkingRing already exists.");
            SetupFeedbackElementsInternal(phaseUI, uiSO);
            uiSO.ApplyModifiedProperties();
            return;
        }

        var canvas = phaseUI.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var ringsAreaProp = uiSO.FindProperty("ringsArea");
        Transform ringsAreaParent = canvas.transform;
        if (ringsAreaProp.objectReferenceValue != null)
            ringsAreaParent = (ringsAreaProp.objectReferenceValue as RectTransform).parent;

        var shrinkGO = new GameObject("ShrinkingRing");
        shrinkGO.transform.SetParent(canvas.transform, false);
        var shrinkRT = shrinkGO.AddComponent<RectTransform>();
        shrinkRT.anchorMin = new Vector2(0.5f, 0.5f);
        shrinkRT.anchorMax = new Vector2(0.5f, 0.5f);
        shrinkRT.pivot = new Vector2(0.5f, 0.5f);
        shrinkRT.anchoredPosition = Vector2.zero;
        shrinkRT.sizeDelta = new Vector2(300f, 300f);
        var shrinkImg = shrinkGO.AddComponent<Image>();
        shrinkImg.raycastTarget = false;

        string ringPath = "Assets/RingGame/Textures/RingSprite_0.png";
        if (File.Exists(ringPath))
            shrinkImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ringPath);

        shrinkImg.color = new Color(1f, 0.92f, 0.25f, 0.9f);
        shrinkGO.SetActive(false);

        uiSO.FindProperty("shrinkingRing").objectReferenceValue = shrinkRT;
        uiSO.FindProperty("shrinkingRingImage").objectReferenceValue = shrinkImg;

        SetupFeedbackElementsInternal(phaseUI, uiSO);
        uiSO.ApplyModifiedProperties();

        Debug.Log("[Iteration 4] ShrinkingRing created.");
    }

    private static void SetupFeedbackElements()
    {
        var phaseUI = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (phaseUI == null) return;
        var uiSO = new SerializedObject(phaseUI);
        SetupFeedbackElementsInternal(phaseUI, uiSO);
        uiSO.ApplyModifiedProperties();
    }

    private static void SetupFeedbackElementsInternal(RhythmPhaseUI phaseUI, SerializedObject uiSO)
    {
        var canvas = phaseUI.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        if (uiSO.FindProperty("feedbackText").objectReferenceValue == null)
        {
            var feedbackGO = new GameObject("FeedbackText");
            feedbackGO.transform.SetParent(canvas.transform, false);
            var feedbackRT = feedbackGO.AddComponent<RectTransform>();
            feedbackRT.anchorMin = new Vector2(0.5f, 0.5f);
            feedbackRT.anchorMax = new Vector2(0.5f, 0.5f);
            feedbackRT.pivot = new Vector2(0.5f, 0.5f);
            feedbackRT.anchoredPosition = new Vector2(0f, 80f);
            feedbackRT.sizeDelta = new Vector2(200f, 60f);
            var tmp = feedbackGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 42f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 1f, 1f, 0f);
            tmp.raycastTarget = false;

            uiSO.FindProperty("feedbackText").objectReferenceValue = tmp;
            uiSO.FindProperty("feedbackRect").objectReferenceValue = feedbackRT;
            Debug.Log("[Iteration 4] FeedbackText created.");
        }

        if (uiSO.FindProperty("flashOverlay").objectReferenceValue == null)
        {
            var existingFlash = canvas.transform.Find("FlashOverlay");
            if (existingFlash != null)
            {
                uiSO.FindProperty("flashOverlay").objectReferenceValue = existingFlash.GetComponent<Image>();
            }
            else
            {
                var flashGO = new GameObject("FlashOverlay");
                flashGO.transform.SetParent(canvas.transform, false);
                var flashRT = flashGO.AddComponent<RectTransform>();
                flashRT.anchorMin = Vector2.zero;
                flashRT.anchorMax = Vector2.one;
                flashRT.offsetMin = Vector2.zero;
                flashRT.offsetMax = Vector2.zero;
                var flashImg = flashGO.AddComponent<Image>();
                flashImg.color = new Color(1f, 1f, 1f, 0f);
                flashImg.raycastTarget = false;
                uiSO.FindProperty("flashOverlay").objectReferenceValue = flashImg;
                Debug.Log("[Iteration 4] FlashOverlay created.");
            }
        }
    }

    private static void EnsureControllerOutsideCanvas()
    {
        var ctrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (ctrl == null) return;

        var canvas = ctrl.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            ctrl.transform.SetParent(null);
            Debug.Log("[Iteration 4] 🔧 RhythmPhaseController moved out of canvas to root.");
        }
    }

    private static bool ValidateSingleton<T>(string label) where T : Component
    {
        var all = Object.FindObjectsOfType<T>(true);
        if (all.Length == 0)
        {
            Debug.LogWarning($"[Validate I4] ⚠️ {label} not found. Run the appropriate setup.");
            return false;
        }
        if (all.Length > 1)
        {
            Debug.Log($"[Validate I4] 🔧 {all.Length}x {label} found — removing duplicates.");
            for (int i = 1; i < all.Length; i++)
                if (all[i] != null) Object.DestroyImmediate(all[i].gameObject);
            return true;
        }
        return false;
    }

    private static bool ValidateControllerNotInCanvas()
    {
        var ctrl = Object.FindObjectOfType<RhythmPhaseController>(true);
        if (ctrl == null) return false;

        if (ctrl.GetComponentInParent<Canvas>() != null)
        {
            ctrl.transform.SetParent(null);
            Debug.Log("[Validate I4] 🔧 RhythmPhaseController was inside a Canvas — moved to root.");
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
            canvas.gameObject.SetActive(false);
            Debug.Log("[Validate I4] 🔧 RhythmPhaseCanvas was active — set inactive.");
            return true;
        }
        return false;
    }

    private static bool ValidateRhythmControllerRef()
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
                Debug.Log("[Validate I4] 🔧 RhythmPhaseController.rhythmPhaseUI reassigned.");
                return true;
            }
        }
        return false;
    }

    private static bool ValidateShrinkingRingRef()
    {
        var ui = Object.FindObjectOfType<RhythmPhaseUI>(true);
        if (ui == null) return false;
        var so = new SerializedObject(ui);
        if (so.FindProperty("shrinkingRing").objectReferenceValue == null)
        {
            Debug.LogWarning("[Validate I4] ⚠️ ShrinkingRing ref missing on RhythmPhaseUI — run Iteration 4 Setup.");
            return false;
        }
        return false;
    }

    private static bool ValidateNullWarnings<T>(string label) where T : Component
    {
        var comp = Object.FindObjectOfType<T>(true);
        if (comp == null) return false;
        var so = new SerializedObject(comp);
        var iter = so.GetIterator();
        iter.Next(true);
        while (iter.NextVisible(false))
        {
            if (iter.propertyType == SerializedPropertyType.ObjectReference
                && iter.objectReferenceValue == null
                && iter.name != "m_Script")
                Debug.LogWarning($"[Validate I4] ⚠️ {label}.{iter.name} is null.");
        }
        return false;
    }

    private static bool ValidateBuildSettings()
    {
        bool fixed_ = false;
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string mainPath = "Assets/RingGame/Scenes/MainMenu.unity";
        string gamePath = "Assets/RingGame/Scenes/Game.unity";

        if (!list.Exists(s => s.path == mainPath) && File.Exists(mainPath))
        {
            list.Insert(0, new EditorBuildSettingsScene(mainPath, true));
            fixed_ = true;
            Debug.Log("[Validate I4] 🔧 MainMenu added to Build Settings.");
        }
        if (!list.Exists(s => s.path == gamePath) && File.Exists(gamePath))
        {
            list.Add(new EditorBuildSettingsScene(gamePath, true));
            fixed_ = true;
            Debug.Log("[Validate I4] 🔧 Game added to Build Settings.");
        }
        if (fixed_) EditorBuildSettings.scenes = list.ToArray();
        return fixed_;
    }
}
