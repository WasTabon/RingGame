using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class CreateRhythmUIElements : EditorWindow
{
    private Transform canvasParent;
    private RhythmPhaseUI rhythmPhaseUI;

    [MenuItem("RingGame/Create Rhythm UI Elements")]
    public static void Open()
    {
        GetWindow<CreateRhythmUIElements>("Create Rhythm UI Elements");
    }

    private void OnGUI()
    {
        GUILayout.Label("Создаст ShrinkingRing, FeedbackText, FlashOverlay", EditorStyles.boldLabel);
        GUILayout.Space(8);

        rhythmPhaseUI = (RhythmPhaseUI)EditorGUILayout.ObjectField(
            "RhythmPhaseUI", rhythmPhaseUI, typeof(RhythmPhaseUI), true);

        canvasParent = (Transform)EditorGUILayout.ObjectField(
            "Родитель (RhythmPhaseCanvas)", canvasParent, typeof(Transform), true);

        GUILayout.Space(8);

        if (rhythmPhaseUI == null)
            EditorGUILayout.HelpBox("Перетащи компонент RhythmPhaseUI из Inspector", MessageType.Warning);
        if (canvasParent == null)
            EditorGUILayout.HelpBox("Перетащи RhythmPhaseCanvas из Hierarchy", MessageType.Warning);

        GUI.enabled = rhythmPhaseUI != null && canvasParent != null;

        GUILayout.Space(4);
        if (GUILayout.Button("Создать и назначить", GUILayout.Height(36)))
            Create();

        GUI.enabled = true;
    }

    private void Create()
    {
        var so = new SerializedObject(rhythmPhaseUI);
        var sprite = LoadRingSprite();
        int created = 0;

        // ShrinkingRing
        var shrinkProp = so.FindProperty("shrinkingRing");
        var shrinkImgProp = so.FindProperty("shrinkingRingImage");
        if (!ReallyExists<RectTransform>(shrinkProp))
        {
            var existing = FindChild(canvasParent, "ShrinkingRing");
            if (existing == null)
            {
                var go = new GameObject("ShrinkingRing");
                go.transform.SetParent(canvasParent, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(300f, 300f);
                var img = go.AddComponent<Image>();
                img.sprite = sprite;
                img.color = new Color(1f, 0.92f, 0.25f, 0.9f);
                img.raycastTarget = false;
                go.SetActive(false);
                go.transform.SetAsLastSibling();
                existing = go.transform;
                Debug.Log("[CreateRhythmUI] ShrinkingRing создан.");
            }
            shrinkProp.objectReferenceValue = existing.GetComponent<RectTransform>();
            shrinkImgProp.objectReferenceValue = existing.GetComponent<Image>();
            created++;
        }
        else Debug.Log("[CreateRhythmUI] ShrinkingRing — уже есть, пропускаем.");

        // FeedbackText
        var feedTxtProp = so.FindProperty("feedbackText");
        var feedRectProp = so.FindProperty("feedbackRect");
        if (!ReallyExists<TextMeshProUGUI>(feedTxtProp))
        {
            var existing = FindChild(canvasParent, "FeedbackText");
            if (existing == null)
            {
                var go = new GameObject("FeedbackText");
                go.transform.SetParent(canvasParent, false);
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
                Debug.Log("[CreateRhythmUI] FeedbackText создан.");
            }
            feedTxtProp.objectReferenceValue = existing.GetComponent<TextMeshProUGUI>();
            feedRectProp.objectReferenceValue = existing.GetComponent<RectTransform>();
            created++;
        }
        else Debug.Log("[CreateRhythmUI] FeedbackText — уже есть, пропускаем.");

        // FlashOverlay
        var flashProp = so.FindProperty("flashOverlay");
        if (!ReallyExists<Image>(flashProp))
        {
            var existing = FindChild(canvasParent, "FlashOverlay");
            if (existing == null)
            {
                var go = new GameObject("FlashOverlay");
                go.transform.SetParent(canvasParent, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                var img = go.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0f);
                img.raycastTarget = false;
                existing = go.transform;
                Debug.Log("[CreateRhythmUI] FlashOverlay создан.");
            }
            flashProp.objectReferenceValue = existing.GetComponent<Image>();
            created++;
        }
        else Debug.Log("[CreateRhythmUI] FlashOverlay — уже есть, пропускаем.");

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(rhythmPhaseUI);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            rhythmPhaseUI.gameObject.scene);

        Debug.Log($"[CreateRhythmUI] Готово. Создано/назначено: {created}. Сохрани сцену (Ctrl+S).");
        EditorUtility.DisplayDialog("Готово",
            $"Создано и назначено: {created} элемент(а).\nНе забудь сохранить сцену (Ctrl+S).", "OK");
    }

    private bool ReallyExists<T>(SerializedProperty prop) where T : Object
    {
        var obj = prop.objectReferenceValue as T;
        if (obj == null) return false;
        if (obj is Component c) return c.gameObject != null;
        return true;
    }

    private Transform FindChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var f = FindChild(child, name);
            if (f != null) return f;
        }
        return null;
    }

    private Sprite LoadRingSprite()
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

    private void EnsureFolder(string path)
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
}
