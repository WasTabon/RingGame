using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class Iteration6_Setup : EditorWindow
{
    [MenuItem("RingGame/Iteration 6/Setup Stage Progression")]
    public static void Setup()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string mainPath = "Assets/RingGame/Scenes/MainMenu.unity";
        if (!File.Exists(mainPath)) { Debug.LogError("[Iter6] MainMenu.unity not found!"); return; }

        var scene = EditorSceneManager.OpenScene(mainPath, OpenSceneMode.Single);

        Debug.Log("[Iter6] ══════════════════════════════");
        Debug.Log("[Iter6] Setting up Stage Progression...");

        CreateStageManagerOnMainMenu();

        EditorSceneManager.SaveScene(scene, mainPath);

        var gameScene = EditorSceneManager.OpenScene("Assets/RingGame/Scenes/Game.unity", OpenSceneMode.Single);
        AddStageDisplayToBetScreen();
        EditorSceneManager.SaveScene(gameScene, "Assets/RingGame/Scenes/Game.unity");

        Debug.Log("[Iter6] ══════════════════════════════");
        Debug.Log("[Iter6] Done!");
    }

    [MenuItem("RingGame/Iteration 6/Validate & Fix")]
    public static void Validate()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Debug.Log("[Iter6 Validate] ══════════════════════════════");
        int fixed_ = 0;

        var mainScene = EditorSceneManager.OpenScene("Assets/RingGame/Scenes/MainMenu.unity", OpenSceneMode.Single);
        var sm = Object.FindObjectOfType<StageManager>(true);
        if (sm == null)
        {
            CreateStageManagerOnMainMenu();
            Debug.Log("[Iter6] 🔧 StageManager created on MainMenu.");
            fixed_++;
        }
        else Debug.Log("[Iter6] ✓ StageManager exists on MainMenu");
        EditorSceneManager.SaveScene(mainScene, "Assets/RingGame/Scenes/MainMenu.unity");

        var gameScene = EditorSceneManager.OpenScene("Assets/RingGame/Scenes/Game.unity", OpenSceneMode.Single);
        var betUI = Object.FindObjectOfType<BetScreenUI>(true);
        if (betUI != null)
        {
            var so = new SerializedObject(betUI);
            if (so.FindProperty("stageLabel").objectReferenceValue == null)
            {
                Debug.LogWarning("[Iter6] ⚠️ BetScreenUI.stageLabel is null — run Setup or assign manually.");
            }
            else Debug.Log("[Iter6] ✓ BetScreenUI.stageLabel assigned");
        }

        var rsc = Object.FindObjectOfType<ResultScreenController>(true);
        if (rsc == null) Debug.LogWarning("[Iter6] ⚠️ ResultScreenController not found — run Iteration 5 Setup.");
        else Debug.Log("[Iter6] ✓ ResultScreenController exists");

        if (fixed_ > 0) EditorSceneManager.SaveScene(gameScene, "Assets/RingGame/Scenes/Game.unity");
        Debug.Log($"[Iter6 Validate] Fixed {fixed_} issue(s).");
        Debug.Log("[Iter6 Validate] ══════════════════════════════");
    }

    [MenuItem("RingGame/Iteration 6/Reset Stage (Debug)")]
    public static void ResetStage()
    {
        PlayerPrefs.DeleteKey("CurrentStage");
        PlayerPrefs.Save();
        Debug.Log("[Iter6] Stage reset to 1.");
    }

    static void CreateStageManagerOnMainMenu()
    {
        var existing = Object.FindObjectOfType<StageManager>(true);
        if (existing != null) { Debug.Log("[Iter6] StageManager already exists."); return; }

        var go = new GameObject("StageManager");
        go.AddComponent<StageManager>();
        Debug.Log("[Iter6] ✓ StageManager created on MainMenu scene.");
    }

    static void AddStageDisplayToBetScreen()
    {
        var betUI = Object.FindObjectOfType<BetScreenUI>(true);
        if (betUI == null) { Debug.LogWarning("[Iter6] BetScreenUI not found on Game scene."); return; }

        var so = new SerializedObject(betUI);

        if (so.FindProperty("stageLabel").objectReferenceValue != null)
        {
            Debug.Log("[Iter6] Stage display already set up.");
            return;
        }

        var canvas = betUI.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var stageContainerGO = new GameObject("StageDisplay");
        stageContainerGO.transform.SetParent(canvas.transform, false);
        var rt = stageContainerGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -60f);
        rt.sizeDelta = new Vector2(400f, 80f);

        var stageLabelGO = new GameObject("StageLabel");
        stageLabelGO.transform.SetParent(stageContainerGO.transform, false);
        var slRT = stageLabelGO.AddComponent<RectTransform>();
        slRT.anchorMin = new Vector2(0.5f, 0.5f);
        slRT.anchorMax = new Vector2(0.5f, 0.5f);
        slRT.pivot = new Vector2(0.5f, 0.5f);
        slRT.anchoredPosition = new Vector2(0f, 14f);
        slRT.sizeDelta = new Vector2(400f, 44f);
        var stageTMP = stageLabelGO.AddComponent<TextMeshProUGUI>();
        stageTMP.text = "STAGE 1";
        stageTMP.fontSize = 34f;
        stageTMP.fontStyle = FontStyles.Bold;
        stageTMP.alignment = TextAlignmentOptions.Center;
        stageTMP.color = new Color(1f, 0.82f, 0.15f);
        stageTMP.raycastTarget = false;

        var ringsLabelGO = new GameObject("StageRingsLabel");
        ringsLabelGO.transform.SetParent(stageContainerGO.transform, false);
        var rlRT = ringsLabelGO.AddComponent<RectTransform>();
        rlRT.anchorMin = new Vector2(0.5f, 0.5f);
        rlRT.anchorMax = new Vector2(0.5f, 0.5f);
        rlRT.pivot = new Vector2(0.5f, 0.5f);
        rlRT.anchoredPosition = new Vector2(0f, -16f);
        rlRT.sizeDelta = new Vector2(400f, 30f);
        var ringsTMP = ringsLabelGO.AddComponent<TextMeshProUGUI>();
        ringsTMP.text = "2 RINGS";
        ringsTMP.fontSize = 20f;
        ringsTMP.alignment = TextAlignmentOptions.Center;
        ringsTMP.color = new Color(0.7f, 0.7f, 0.9f);
        ringsTMP.raycastTarget = false;

        so.FindProperty("stageLabel").objectReferenceValue = stageTMP;
        so.FindProperty("stageRingsLabel").objectReferenceValue = ringsTMP;
        so.ApplyModifiedProperties();

        Debug.Log("[Iter6] ✓ Stage display added to BetScreen.");
    }
}
