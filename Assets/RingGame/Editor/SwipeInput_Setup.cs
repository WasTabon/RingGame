using UnityEngine;
using UnityEditor;

public class SwipeInput_Setup : EditorWindow
{
    [MenuItem("RingGame/Swipe Input/Validate")]
    public static void Validate()
    {
        Debug.Log("[SwipeInput] ══════════════════════════════");

        var handler = Object.FindObjectOfType<TapInputHandler>(true);
        if (handler == null)
        {
            Debug.LogWarning("[SwipeInput] TapInputHandler not found in scene.");
            Debug.Log("[SwipeInput] ══════════════════════════════");
            return;
        }

        Debug.Log("[SwipeInput] ✓ TapInputHandler found on: " + handler.gameObject.name);

        var so = new SerializedObject(handler);
        var threshold = so.FindProperty("swipeThreshold");
        if (threshold != null)
            Debug.Log("[SwipeInput] ✓ swipeThreshold = " + threshold.floatValue + "px");
        else
            Debug.LogWarning("[SwipeInput] ⚠️ swipeThreshold property not found — is the script updated?");

        Debug.Log("[SwipeInput] ✅ Swipe input ready. No setup needed — just replace the script file.");
        Debug.Log("[SwipeInput] ══════════════════════════════");
    }
}
