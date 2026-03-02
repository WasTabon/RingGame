using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RhythmPhaseController : MonoBehaviour
{
    public static RhythmPhaseController Instance { get; private set; }

    [SerializeField] private RhythmPhaseUI rhythmPhaseUI;

    private const int TotalCycles = 4;
    private const int AttemptsPerCycle = 4;

    private int currentCycle;
    private int remainingAttempts;
    private int currentStage = 1;
    private bool phaseActive;

    private List<SymbolConfig.SymbolType?> capturedSymbols = new List<SymbolConfig.SymbolType?>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnAudioBeat()
    {
        if (!phaseActive) return;
        RingsManager.Instance?.OnBeat();
    }

    private void SubscribeToEvents()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnBeat -= OnAudioBeat;
            AudioManager.Instance.OnBeat += OnAudioBeat;
        }
        if (BeatSequencer.Instance != null)
        {
            BeatSequencer.Instance.OnBeatCue -= OnBeatCue;
            BeatSequencer.Instance.OnBeatCue += OnBeatCue;
            BeatSequencer.Instance.OnBeatExpired -= OnBeatExpired;
            BeatSequencer.Instance.OnBeatExpired += OnBeatExpired;
            BeatSequencer.Instance.OnCycleComplete -= OnCycleComplete;
            BeatSequencer.Instance.OnCycleComplete += OnCycleComplete;
        }
        if (TapInputHandler.Instance != null)
        {
            TapInputHandler.Instance.OnTap -= OnTap;
            TapInputHandler.Instance.OnTap += OnTap;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.OnBeat -= OnAudioBeat;
        if (BeatSequencer.Instance != null)
        {
            BeatSequencer.Instance.OnBeatCue -= OnBeatCue;
            BeatSequencer.Instance.OnBeatExpired -= OnBeatExpired;
            BeatSequencer.Instance.OnCycleComplete -= OnCycleComplete;
        }
        if (TapInputHandler.Instance != null)
            TapInputHandler.Instance.OnTap -= OnTap;
    }

    public void ActivatePhaseFromBet()
    {
        currentStage = 1;
        capturedSymbols.Clear();
        StartPhase();
    }

    public void ActivatePhaseFromEscalation(int stage)
    {
        currentStage = stage;
        capturedSymbols.Clear();
        StartPhase();
    }

    private void StartPhase()
    {
        phaseActive = true;
        currentCycle = 1;
        remainingAttempts = AttemptsPerCycle;

        rhythmPhaseUI?.Show();
        rhythmPhaseUI?.UpdateCycleDisplay(currentCycle, TotalCycles, false);
        rhythmPhaseUI?.UpdateAttempts(remainingAttempts, false);

        RingsManager.Instance?.SetupForStage(currentStage);
        RingsManager.Instance?.PlayEntrance();

        SubscribeToEvents();
        TapInputHandler.Instance?.SetActive(true);

        StartCoroutine(StartSequenceDelayed());
    }

    private IEnumerator StartSequenceDelayed()
    {
        yield return new WaitForSeconds(0.9f);
        BeatSequencer.Instance?.StartSequence(currentStage, RingsManager.Instance.RingCount);
    }

    private void OnBeatCue(int ringIndex)
    {
        if (!phaseActive) return;
        RingsManager.Instance?.HighlightRing(ringIndex, true);
        rhythmPhaseUI?.ShowShrinkingRing(ringIndex, BeatSequencer.Instance.GetWindowDuration());
        rhythmPhaseUI?.PulseBeatFeedback();
    }

    private void OnBeatExpired(int ringIndex)
    {
        if (!phaseActive) return;

        RingsManager.Instance?.HighlightRing(ringIndex, false);
        rhythmPhaseUI?.HideShrinkingRing();
        rhythmPhaseUI?.ShowMissFeedback();

        remainingAttempts--;
        rhythmPhaseUI?.UpdateAttempts(remainingAttempts, true);

        if (remainingAttempts <= 0)
            BeatSequencer.Instance?.StopSequence();
    }

    private void OnTap()
    {
        if (!phaseActive) return;
        if (BeatSequencer.Instance == null || !BeatSequencer.Instance.IsWindowOpen) return;

        int ringIndex = BeatSequencer.Instance.CurrentBeatRingIndex;
        BeatSequencer.Instance.RegisterHit();

        RingsManager.Instance?.HighlightRing(ringIndex, false);
        rhythmPhaseUI?.HideShrinkingRing();

        bool isWild = ShouldBeWild();
        var symbol = isWild
            ? SymbolConfig.SymbolType.Wild
            : RingsManager.Instance.ActiveRings[ringIndex].SymbolType;

        capturedSymbols.Add(symbol);
        RingsManager.Instance?.CaptureRing(ringIndex);
        rhythmPhaseUI?.ShowHitFeedback();
        rhythmPhaseUI?.ShowCapturedSymbol(ringIndex, symbol);
    }

    private void OnCycleComplete()
    {
        if (!phaseActive) return;
        StartCoroutine(HandleCycleEnd());
    }

    private IEnumerator HandleCycleEnd()
    {
        TapInputHandler.Instance?.SetActive(false);
        yield return new WaitForSeconds(0.3f);

        rhythmPhaseUI?.ShowCycleCompleteEffect(currentCycle);
        RingsManager.Instance?.ShrinkAllRings(null);

        yield return new WaitForSeconds(0.7f);

        if (currentCycle >= TotalCycles)
        {
            EndPhase();
            yield break;
        }

        currentCycle++;
        remainingAttempts = AttemptsPerCycle;

        rhythmPhaseUI?.UpdateCycleDisplay(currentCycle, TotalCycles, true);
        rhythmPhaseUI?.UpdateAttempts(remainingAttempts, false);

        RingsManager.Instance?.ResetCapturedRings();

        yield return new WaitForSeconds(0.4f);

        TapInputHandler.Instance?.SetActive(true);
        BeatSequencer.Instance?.StartSequence(currentStage, RingsManager.Instance.RingCount);
    }

    private void EndPhase()
    {
        phaseActive = false;
        TapInputHandler.Instance?.SetActive(false);
        BeatSequencer.Instance?.StopSequence();

        rhythmPhaseUI?.ShowPhaseComplete(() =>
        {
            GameManager.Instance.SetState(GameManager.GameState.ResultScreen);
            rhythmPhaseUI?.Hide();
        });
    }

    private bool ShouldBeWild()
    {
        if (currentCycle < 3) return false;
        float wildChance = 0.15f + (currentStage - 1) * 0.05f;
        return Random.value < wildChance;
    }

    public void OnBackToBet()
    {
        phaseActive = false;
        BeatSequencer.Instance?.StopSequence();
        TapInputHandler.Instance?.SetActive(false);
        UnsubscribeFromEvents();

        GameManager.Instance.SetState(GameManager.GameState.BetScreen);
        rhythmPhaseUI?.Hide();

        var betUI = FindObjectOfType<BetScreenUI>(true);
        if (betUI != null)
        {
            betUI.gameObject.SetActive(true);
            betUI.ResetAndShow();
        }
    }

    public List<SymbolConfig.SymbolType?> GetCapturedSymbols() => capturedSymbols;
    public int GetCurrentStage() => currentStage;
}
