using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class Iteration8_Setup : EditorWindow
{
    [MenuItem("RingGame/Iteration 8/Setup SFX Manager")]
    public static void SetupSFXManager()
    {
        var existing = Object.FindObjectOfType<SFXManager>(true);
        if (existing != null)
        {
            Debug.Log("[Iter8] SFXManager already exists on: " + existing.gameObject.name);
            Selection.activeGameObject = existing.gameObject;
            EditorUtility.DisplayDialog("Already Exists",
                "SFXManager already exists in scene.\nSelected it in hierarchy.", "OK");
            return;
        }

        var go = new GameObject("SFXManager");
        go.AddComponent<SFXManager>();
        Undo.RegisterCreatedObjectUndo(go, "Create SFXManager");

        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeGameObject = go;

        Debug.Log("[Iter8] SFXManager created. Save scene (Ctrl+S).");
        EditorUtility.DisplayDialog("Done",
            "SFXManager created in scene root.\n\n" +
            "It uses DontDestroyOnLoad, so place it in MainMenu scene.\n" +
            "SFXLibrary generates all clips at Awake.\n\n" +
            "Save scene (Ctrl+S).", "OK");
    }

    [MenuItem("RingGame/Iteration 8/Validate SFX")]
    public static void Validate()
    {
        Debug.Log("[Iter8 Validate] ══════════════════════════════");

        var sfx = Object.FindObjectOfType<SFXManager>(true);
        if (sfx == null)
        {
            Debug.LogWarning("[Iter8] SFXManager not found. Run Setup SFX Manager in MainMenu scene.");
            return;
        }
        Debug.Log("[Iter8] ✓ SFXManager found on: " + sfx.gameObject.name);

        var audio = Object.FindObjectOfType<AudioManager>(true);
        if (audio == null)
            Debug.LogWarning("[Iter8] AudioManager not found. SFX won't play without it.");
        else
            Debug.Log("[Iter8] ✓ AudioManager found");

        var sfxSource = audio?.GetType().GetField("sfxSource",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (sfxSource != null)
        {
            var src = sfxSource.GetValue(audio) as AudioSource;
            if (src == null)
                Debug.LogWarning("[Iter8] AudioManager.sfxSource is null — assign an AudioSource.");
            else
                Debug.Log("[Iter8] ✓ AudioManager.sfxSource assigned");
        }

        Debug.Log("[Iter8 Validate] ✅ Done.");
        Debug.Log("[Iter8 Validate] ══════════════════════════════");
    }
}
