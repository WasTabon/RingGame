using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class RuntimeDiagnostics : MonoBehaviour
{
    private List<string> log = new List<string>();
    private float startTime;
    private int beatCount;
    private int beatCueCount;
    private int beatExpiredCount;
    private int tapCount;
    private int tapHitCount;
    private int tapMissCount;
    private float lowFreqMax;
    private float lowFreqMin = 999f;
    private int lowFreqChanges;
    private float lastLowFreq;
    private int frameCount;
    private bool summarySaved;

    private void Start()
    {
        startTime = Time.time;
        Log("=== DIAGNOSTICS STARTED ===");
        Log($"Time: {System.DateTime.Now}");
        Log("");
        CheckManagers();
        SubscribeToEvents();
        Log("");
        Log("=== EVENTS LOG ===");
    }

    private void CheckManagers()
    {
        Log("--- MANAGERS ---");
        LogCheck("GameManager", GameManager.Instance != null,
            GameManager.Instance != null ? $"State={GameManager.Instance.CurrentState}" : "");
        LogCheck("AudioManager", AudioManager.Instance != null, "");
        LogCheck("BalanceManager", BalanceManager.Instance != null,
            BalanceManager.Instance != null ? $"Balance=${BalanceManager.Instance.Balance}" : "");
        LogCheck("BetManager", BetManager.Instance != null,
            BetManager.Instance != null ? $"Bet=${BetManager.Instance.CurrentBet}" : "");
        LogCheck("RingsManager", RingsManager.Instance != null,
            RingsManager.Instance != null ? $"Rings={RingsManager.Instance.RingCount}" : "");
        LogCheck("RhythmPhaseController", RhythmPhaseController.Instance != null, "");
        LogCheck("BeatSequencer", BeatSequencer.Instance != null, "");
        LogCheck("TapInputHandler", TapInputHandler.Instance != null, "");

        if (AudioManager.Instance != null)
        {
            var sources = AudioManager.Instance.GetComponentsInChildren<AudioSource>();
            foreach (var src in sources)
            {
                if (src.loop)
                    Log($"  AudioSource: isPlaying={src.isPlaying}  clip={(src.clip != null ? src.clip.name : "NULL")}");
            }
        }
    }

    private void SubscribeToEvents()
    {
        Log("--- SUBSCRIPTIONS ---");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnBeat -= OnBeat;
            AudioManager.Instance.OnBeat += OnBeat;
            Log("  [OK] AudioManager.OnBeat");
        }
        else Log("  [FAIL] AudioManager.Instance is NULL");

        if (TapInputHandler.Instance != null)
        {
            TapInputHandler.Instance.OnTap -= OnTap;
            TapInputHandler.Instance.OnTap += OnTap;
            Log("  [OK] TapInputHandler.OnTap");
        }
        else Log("  [FAIL] TapInputHandler.Instance is NULL");

        if (BeatSequencer.Instance != null)
        {
            BeatSequencer.Instance.OnBeatCue -= OnBeatCue;
            BeatSequencer.Instance.OnBeatCue += OnBeatCue;
            BeatSequencer.Instance.OnBeatExpired -= OnBeatExpired;
            BeatSequencer.Instance.OnBeatExpired += OnBeatExpired;
            BeatSequencer.Instance.OnCycleComplete -= OnCycleComplete;
            BeatSequencer.Instance.OnCycleComplete += OnCycleComplete;
            Log("  [OK] BeatSequencer events");
        }
        else Log("  [FAIL] BeatSequencer.Instance is NULL");

        if (RingsManager.Instance != null)
        {
            RingsManager.Instance.OnRingsSpawned -= OnRingsSpawned;
            RingsManager.Instance.OnRingsSpawned += OnRingsSpawned;
            Log("  [OK] RingsManager.OnRingsSpawned");
        }
        else Log("  [FAIL] RingsManager.Instance is NULL");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= OnStateChanged;
            GameManager.Instance.OnStateChanged += OnStateChanged;
            Log("  [OK] GameManager.OnStateChanged");
        }
        else Log("  [FAIL] GameManager.Instance is NULL");
    }

    private void Update()
    {
        frameCount++;

        if (AudioManager.Instance != null)
        {
            float low = AudioManager.Instance.LowFreqValue;
            if (Mathf.Abs(low - lastLowFreq) > 0.01f)
            {
                lowFreqChanges++;
                if (low > lowFreqMax) lowFreqMax = low;
                if (low < lowFreqMin) lowFreqMin = low;
                lastLowFreq = low;
            }
        }

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            bool windowOpen = BeatSequencer.Instance != null && BeatSequencer.Instance.IsWindowOpen;
            Log($"  [RAW INPUT] at t={T()}  BeatWindowOpen={windowOpen}  TapHandlerExists={TapInputHandler.Instance != null}");
        }
    }

    private void OnBeat()
    {
        beatCount++;
        Log($"  [AUDIO BEAT] #{beatCount} t={T()}  LowFreq={AudioManager.Instance?.LowFreqValue:F3}");
    }

    private void OnBeatCue(int ringIndex)
    {
        beatCueCount++;
        Log($"  [BEAT CUE] #{beatCueCount} ring={ringIndex} t={T()}  WindowOpen={BeatSequencer.Instance?.IsWindowOpen}");
        if (RingsManager.Instance != null && ringIndex < RingsManager.Instance.RingCount)
            Log($"    Ring{ringIndex} state={RingsManager.Instance.ActiveRings[ringIndex].CurrentState}");
    }

    private void OnBeatExpired(int ringIndex)
    {
        beatExpiredCount++;
        Log($"  [BEAT EXPIRED] ring={ringIndex} t={T()}");
    }

    private void OnCycleComplete()
    {
        Log($"  [CYCLE COMPLETE] t={T()}");
    }

    private void OnTap()
    {
        tapCount++;
        bool windowOpen = BeatSequencer.Instance?.IsWindowOpen ?? false;
        int ringIndex = BeatSequencer.Instance?.CurrentBeatRingIndex ?? -1;
        if (windowOpen)
        {
            tapHitCount++;
            Log($"  [TAP HIT] #{tapCount} t={T()} ring={ringIndex} progress={BeatSequencer.Instance?.WindowProgress:F2}");
        }
        else
        {
            tapMissCount++;
            Log($"  [TAP MISS] #{tapCount} t={T()} window was closed");
        }
    }

    private void OnRingsSpawned()
    {
        Log($"  [RINGS SPAWNED] count={RingsManager.Instance?.RingCount} t={T()}");
        if (RingsManager.Instance != null)
            for (int i = 0; i < RingsManager.Instance.ActiveRings.Count; i++)
            {
                var r = RingsManager.Instance.ActiveRings[i];
                Log($"    Ring{i}: symbol={r.SymbolType} state={r.CurrentState}");
            }
    }

    private void OnStateChanged(GameManager.GameState state)
    {
        Log($"  [STATE CHANGE] -> {state} t={T()}");
        if (state == GameManager.GameState.RhythmPhase)
        {
            Log($"    RhythmPhaseController={RhythmPhaseController.Instance != null}");
            Log($"    BeatSequencer={BeatSequencer.Instance != null}");
            Log($"    TapInputHandler={TapInputHandler.Instance != null}");
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        SaveLog();
    }

    private void OnApplicationQuit()
    {
        SaveLog();
    }

    private void UnsubscribeFromEvents()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.OnBeat -= OnBeat;
        if (TapInputHandler.Instance != null) TapInputHandler.Instance.OnTap -= OnTap;
        if (BeatSequencer.Instance != null)
        {
            BeatSequencer.Instance.OnBeatCue -= OnBeatCue;
            BeatSequencer.Instance.OnBeatExpired -= OnBeatExpired;
            BeatSequencer.Instance.OnCycleComplete -= OnCycleComplete;
        }
        if (RingsManager.Instance != null) RingsManager.Instance.OnRingsSpawned -= OnRingsSpawned;
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void SaveLog()
    {
        if (summarySaved) return;
        summarySaved = true;

        Log("");
        Log("=== SUMMARY ===");
        Log($"Duration: {(Time.time - startTime):F1}s  Frames: {frameCount}");
        Log($"Audio beats: {beatCount}");
        Log($"LowFreq changes: {lowFreqChanges}  min={lowFreqMin:F3}  max={lowFreqMax:F3}");
        Log(lowFreqChanges > 10
            ? "  -> [OK] Audio spectrum updating"
            : "  -> [WARN] LowFreq barely changed — music not playing or AudioSource not assigned");
        Log($"Beat cues: {beatCueCount}  Expired: {beatExpiredCount}");
        Log(beatCueCount == 0
            ? "  -> [WARN] No beat cues — BeatSequencer never started or not connected to controller"
            : "  -> [OK] Beat cues fired");
        Log($"Taps: {tapCount}  Hits: {tapHitCount}  Misses: {tapMissCount}");
        Log(tapCount == 0
            ? "  -> [WARN] No taps — TapInputHandler not working or not active"
            : "  -> [OK] Taps registered");

        string path = Path.Combine(Application.persistentDataPath, "diagnostics.txt");
        File.WriteAllLines(path, log);
        Debug.Log($"[Diagnostics] Saved to: {path}");
    }

    private void Log(string msg) => log.Add(msg);
    private string T() => $"{(Time.time - startTime):F2}s";
    private void LogCheck(string label, bool ok, string extra)
    {
        string ext = extra.Length > 0 ? $"  ({extra})" : "";
        log.Add($"  {(ok ? "[OK]" : "[MISSING]")} {label}{ext}");
    }
}