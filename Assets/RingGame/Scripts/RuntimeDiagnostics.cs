using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RuntimeDiagnostics : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private bool showOnStart = true;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI outputText;
    private Image beatFlash;

    private bool visible;
    private float beatFlashTimer;
    private float tapFlashTimer;
    private int beatCount;
    private int tapCount;
    private int hitCount;
    private int missCount;
    private float lastLowFreq;
    private bool wasWindowOpen;

    private void Awake()
    {
        BuildUI();
    }

    private void Start()
    {
        SubscribeToEvents();
        visible = showOnStart;
        canvasGroup.alpha = visible ? 1f : 0f;
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnBeat -= OnBeat;
            AudioManager.Instance.OnBeat += OnBeat;
        }
        if (TapInputHandler.Instance != null)
        {
            TapInputHandler.Instance.OnTap -= OnTap;
            TapInputHandler.Instance.OnTap += OnTap;
        }
        if (BeatSequencer.Instance != null)
        {
            BeatSequencer.Instance.OnBeatCue -= OnBeatCue;
            BeatSequencer.Instance.OnBeatCue += OnBeatCue;
            BeatSequencer.Instance.OnBeatExpired -= OnBeatExpired;
            BeatSequencer.Instance.OnBeatExpired += OnBeatExpired;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.OnBeat -= OnBeat;
        if (TapInputHandler.Instance != null)
            TapInputHandler.Instance.OnTap -= OnTap;
        if (BeatSequencer.Instance != null)
        {
            BeatSequencer.Instance.OnBeatCue -= OnBeatCue;
            BeatSequencer.Instance.OnBeatExpired -= OnBeatExpired;
        }
    }

    private void OnBeat()
    {
        beatCount++;
        beatFlashTimer = 0.15f;
    }

    private void OnTap()
    {
        tapCount++;
        tapFlashTimer = 0.15f;

        if (BeatSequencer.Instance != null && BeatSequencer.Instance.IsWindowOpen)
            hitCount++;
        else
            missCount++;
    }

    private void OnBeatCue(int ringIndex)
    {
        beatCount++;
    }

    private void OnBeatExpired(int ringIndex)
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            visible = !visible;
            canvasGroup.DOFade(visible ? 1f : 0f, 0.2f);
        }

        if (!visible) return;

        beatFlashTimer -= Time.deltaTime;
        tapFlashTimer -= Time.deltaTime;

        if (beatFlash != null)
        {
            beatFlash.color = beatFlashTimer > 0
                ? new Color(1f, 0.82f, 0.1f, 0.85f)
                : new Color(0.3f, 0.3f, 0.3f, 0.4f);
        }

        outputText.text = BuildReport();
    }

    private string BuildReport()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("<b>── RUNTIME DIAGNOSTICS ──</b>");
        sb.AppendLine($"<size=11><color=#888>F1 to toggle</color></size>\n");

        sb.AppendLine(SectionHeader("MANAGERS"));
        sb.AppendLine(CheckInstance("GameManager", GameManager.Instance != null,
            GameManager.Instance != null ? $"State: <b>{GameManager.Instance.CurrentState}</b>" : ""));
        sb.AppendLine(CheckInstance("AudioManager", AudioManager.Instance != null,
            AudioManager.Instance != null ? $"Playing: <b>{AudioManager.Instance.GetComponent<AudioSource>() != null}</b>" : ""));
        sb.AppendLine(CheckInstance("BalanceManager", BalanceManager.Instance != null,
            BalanceManager.Instance != null ? $"Balance: <b>${BalanceManager.Instance.Balance:N0}</b>" : ""));
        sb.AppendLine(CheckInstance("BetManager", BetManager.Instance != null,
            BetManager.Instance != null ? $"Bet: <b>${BetManager.Instance.CurrentBet:N0}</b>" : ""));
        sb.AppendLine(CheckInstance("RingsManager", RingsManager.Instance != null,
            RingsManager.Instance != null ? $"Rings: <b>{RingsManager.Instance.RingCount}</b>" : ""));
        sb.AppendLine(CheckInstance("RhythmPhaseCtrl", RhythmPhaseController.Instance != null, ""));
        sb.AppendLine(CheckInstance("BeatSequencer", BeatSequencer.Instance != null, ""));
        sb.AppendLine(CheckInstance("TapInputHandler", TapInputHandler.Instance != null, ""));

        sb.AppendLine();
        sb.AppendLine(SectionHeader("AUDIO"));

        if (AudioManager.Instance != null)
        {
            float low = AudioManager.Instance.LowFreqValue;
            float mid = AudioManager.Instance.MidFreqValue;
            bool lowChanging = Mathf.Abs(low - lastLowFreq) > 0.001f;
            lastLowFreq = low;

            string musicStatus = CheckMusicSource();
            sb.AppendLine($"  Music Source   {musicStatus}");
            sb.AppendLine($"  Low Freq       {Bar(low)} {low:F3} {(lowChanging ? "<color=#4f4>" + "▲</color>" : "")}");
            sb.AppendLine($"  Mid Freq       {Bar(mid)} {mid:F3}");
            sb.AppendLine($"  Beats Detected <b>{beatCount}</b>  {(beatFlashTimer > 0 ? "<color=#FFD700>● BEAT</color>" : "○")}");
        }
        else
        {
            sb.AppendLine(Err("  AudioManager not found!"));
        }

        sb.AppendLine();
        sb.AppendLine(SectionHeader("BEAT SEQUENCER"));

        if (BeatSequencer.Instance != null)
        {
            var bs = BeatSequencer.Instance;
            sb.AppendLine($"  Window Open    {Bool(bs.IsWindowOpen)}");
            sb.AppendLine($"  Current Ring   <b>{bs.CurrentBeatRingIndex}</b>");
            sb.AppendLine($"  Window Progress {ProgressBar(bs.WindowProgress)}");
        }
        else
        {
            sb.AppendLine(Err("  BeatSequencer not found!"));
        }

        sb.AppendLine();
        sb.AppendLine(SectionHeader("INPUT"));

        if (TapInputHandler.Instance != null)
        {
            sb.AppendLine($"  Total Taps     <b>{tapCount}</b>  {(tapFlashTimer > 0 ? "<color=#4af>● TAP</color>" : "○")}");
            sb.AppendLine($"  Hits           <color=#4f4><b>{hitCount}</b></color>");
            sb.AppendLine($"  Misses         <color=#f44><b>{missCount}</b></color>");

            bool windowOpen = BeatSequencer.Instance != null && BeatSequencer.Instance.IsWindowOpen;
            sb.AppendLine($"  Next tap would <b>{(windowOpen ? "<color=#4f4>HIT</color>" : "<color=#f44>MISS</color>")}</b>");
        }
        else
        {
            sb.AppendLine(Err("  TapInputHandler not found!"));
        }

        sb.AppendLine();
        sb.AppendLine(SectionHeader("RINGS"));

        if (RingsManager.Instance != null && RingsManager.Instance.RingCount > 0)
        {
            for (int i = 0; i < RingsManager.Instance.ActiveRings.Count; i++)
            {
                var ring = RingsManager.Instance.ActiveRings[i];
                string state = ring.CurrentState.ToString();
                string stateColor = ring.CurrentState switch
                {
                    RingController.RingState.Highlighted => "#FFD700",
                    RingController.RingState.Captured => "#4f4",
                    RingController.RingState.Stopped => "#888",
                    _ => "#aaa"
                };
                sb.AppendLine($"  Ring {i}  Symbol:<b>{ring.SymbolType}</b>  State:<color={stateColor}><b>{state}</b></color>");
            }
        }
        else
        {
            sb.AppendLine("  <color=#888>No rings active (start a round)</color>");
        }

        sb.AppendLine();
        sb.AppendLine($"<size=10><color=#555>Frame: {Time.frameCount}  Time: {Time.time:F1}s</color></size>");

        return sb.ToString();
    }

    private string CheckMusicSource()
    {
        var am = AudioManager.Instance;
        if (am == null) return Err("NULL");
        var sources = am.GetComponentsInChildren<AudioSource>();
        foreach (var src in sources)
        {
            if (src.loop)
                return src.isPlaying
                    ? $"<color=#4f4>● PLAYING</color>  clip: <b>{(src.clip != null ? src.clip.name : "none")}</b>"
                    : $"<color=#f44>○ STOPPED</color>  clip: <b>{(src.clip != null ? src.clip.name : "none")}</b>";
        }
        return Err("no looping AudioSource found");
    }

    private string SectionHeader(string title)
        => $"<color=#6af><b>▸ {title}</b></color>";

    private string CheckInstance(string label, bool exists, string extra)
    {
        string status = exists ? "<color=#4f4>✓</color>" : "<color=#f44>✗</color>";
        string ext = extra.Length > 0 ? $"  {extra}" : "";
        return $"  {status} {label}{ext}";
    }

    private string Bool(bool val)
        => val ? "<color=#4f4><b>YES</b></color>" : "<color=#f44><b>NO</b></color>";

    private string Bar(float val)
    {
        int filled = Mathf.RoundToInt(val * 10f);
        filled = Mathf.Clamp(filled, 0, 10);
        string bar = "[";
        for (int i = 0; i < 10; i++)
            bar += i < filled ? "█" : "░";
        bar += "]";
        return bar;
    }

    private string ProgressBar(float val)
    {
        int filled = Mathf.RoundToInt(val * 12f);
        filled = Mathf.Clamp(filled, 0, 12);
        string bar = "[";
        for (int i = 0; i < 12; i++)
            bar += i < filled ? "<color=#FFD700>█</color>" : "░";
        bar += "]";
        return bar;
    }

    private string Err(string msg) => $"<color=#f44>{msg}</color>";

    private void BuildUI()
    {
        var canvasGO = new GameObject("DiagnosticsCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(390, 844);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        var panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0f, 0f);
        panelRT.anchorMax = new Vector2(0f, 1f);
        panelRT.pivot = new Vector2(0f, 0.5f);
        panelRT.anchoredPosition = new Vector2(4f, 0f);
        panelRT.sizeDelta = new Vector2(220f, -8f);
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.02f, 0.02f, 0.06f, 0.88f);
        panelImg.raycastTarget = false;

        var beatIndicatorGO = new GameObject("BeatFlash");
        beatIndicatorGO.transform.SetParent(panelGO.transform, false);
        var beatRT = beatIndicatorGO.AddComponent<RectTransform>();
        beatRT.anchorMin = new Vector2(1f, 1f);
        beatRT.anchorMax = new Vector2(1f, 1f);
        beatRT.pivot = new Vector2(1f, 1f);
        beatRT.anchoredPosition = new Vector2(-6f, -6f);
        beatRT.sizeDelta = new Vector2(14f, 14f);
        beatFlash = beatIndicatorGO.AddComponent<Image>();
        beatFlash.color = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        beatFlash.raycastTarget = false;

        var textGO = new GameObject("OutputText");
        textGO.transform.SetParent(panelGO.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(8f, 8f);
        textRT.offsetMax = new Vector2(-8f, -8f);
        outputText = textGO.AddComponent<TextMeshProUGUI>();
        outputText.fontSize = 11f;
        outputText.color = new Color(0.85f, 0.88f, 1f);
        outputText.richText = true;
        outputText.overflowMode = TextOverflowModes.Overflow;
        outputText.raycastTarget = false;
        outputText.text = "Loading...";
    }
}
