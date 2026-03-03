using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    private const string StageKey = "CurrentStage";
    private const int MinStage = 1;
    private const int MaxStage = 4;

    public int CurrentStage { get; private set; }
    public bool IsMaxStage => CurrentStage >= MaxStage;

    public System.Action<int> OnStageChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentStage = PlayerPrefs.GetInt(StageKey, MinStage);
    }

    public void AdvanceStage()
    {
        if (CurrentStage >= MaxStage)
        {
            CurrentStage = MinStage;
        }
        else
        {
            CurrentStage++;
        }
        Save();
        OnStageChanged?.Invoke(CurrentStage);
    }

    public void ResetToFirstStage()
    {
        CurrentStage = MinStage;
        Save();
        OnStageChanged?.Invoke(CurrentStage);
    }

    public void SetStage(int stage)
    {
        CurrentStage = Mathf.Clamp(stage, MinStage, MaxStage);
        Save();
        OnStageChanged?.Invoke(CurrentStage);
    }

    public int GetRingCountForStage(int stage)
    {
        return Mathf.Clamp(stage + 1, 2, 5);
    }

    public string GetStageLabel()
    {
        return $"STAGE {CurrentStage}";
    }

    public float GetTimingWindowMs()
    {
        return CurrentStage switch
        {
            1 => 800f,
            2 => 600f,
            3 => 400f,
            4 => 250f,
            _ => 800f
        };
    }

    private void Save()
    {
        PlayerPrefs.SetInt(StageKey, CurrentStage);
        PlayerPrefs.Save();
    }
}
