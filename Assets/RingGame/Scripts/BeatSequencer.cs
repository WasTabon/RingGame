using UnityEngine;
using System.Collections;

public class BeatSequencer : MonoBehaviour
{
    public static BeatSequencer Instance { get; private set; }

    [Header("Timing")]
    [SerializeField] private float bpm = 120f;
    [SerializeField] private float beatInterval = 1.5f;
    [SerializeField] private float cueLeadTime = 0.8f;

    [Header("Timing Windows (ms)")]
    [SerializeField] private float[] stageWindowMs = { 200f, 170f, 140f, 110f };

    public System.Action<int> OnBeatCue;
    public System.Action<int> OnBeatExpired;
    public System.Action OnCycleComplete;

    public bool IsWindowOpen { get; private set; }
    public int CurrentBeatRingIndex { get; private set; }
    public float WindowProgress { get; private set; }

    private int currentStage = 1;
    private int beatsPerCycle = 4;
    private int currentBeatInCycle = 0;
    private bool isRunning;
    private float windowDuration;
    private float windowTimer;
    private Coroutine sequencerCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartSequence(int stage, int ringCount)
    {
        StopSequence();
        currentStage = Mathf.Clamp(stage, 1, 4);
        windowDuration = stageWindowMs[currentStage - 1] / 1000f;
        currentBeatInCycle = 0;
        isRunning = true;
        IsWindowOpen = false;
        sequencerCoroutine = StartCoroutine(SequenceLoop(ringCount));
    }

    public void StopSequence()
    {
        isRunning = false;
        IsWindowOpen = false;
        WindowProgress = 0f;
        if (sequencerCoroutine != null)
        {
            StopCoroutine(sequencerCoroutine);
            sequencerCoroutine = null;
        }
    }

    private IEnumerator SequenceLoop(int ringCount)
    {
        yield return new WaitForSeconds(0.8f);

        while (isRunning)
        {
            int ringIndex = currentBeatInCycle % ringCount;
            CurrentBeatRingIndex = ringIndex;

            OnBeatCue?.Invoke(ringIndex);

            IsWindowOpen = true;
            windowTimer = 0f;
            bool wasHit = false;

            while (windowTimer < windowDuration)
            {
                windowTimer += Time.deltaTime;
                WindowProgress = windowTimer / windowDuration;

                if (!IsWindowOpen)
                {
                    wasHit = true;
                    break;
                }

                yield return null;
            }

            if (!wasHit)
            {
                IsWindowOpen = false;
                WindowProgress = 1f;
                OnBeatExpired?.Invoke(ringIndex);
            }

            currentBeatInCycle++;

            if (currentBeatInCycle >= beatsPerCycle)
            {
                currentBeatInCycle = 0;
                isRunning = false;
                IsWindowOpen = false;
                OnCycleComplete?.Invoke();
                yield break;
            }

            yield return new WaitForSeconds(beatInterval - windowDuration);
        }
    }

    public void RegisterHit()
    {
        IsWindowOpen = false;
    }

    public void SetBeatInterval(float interval)
    {
        beatInterval = interval;
    }

    public float GetWindowDuration() => windowDuration;
    public float GetBeatInterval() => beatInterval;
}
