using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SFXLibrary.Generate();
    }

    private void Play(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip, sfxVolume * volumeScale);
    }

    public void PlayHit()         => Play(SFXLibrary.Hit);
    public void PlayMiss()        => Play(SFXLibrary.Miss);
    public void PlayCapture()     => Play(SFXLibrary.Capture);
    public void PlayButtonClick() => Play(SFXLibrary.ButtonClick);
    public void PlayBetTick()     => Play(SFXLibrary.BetTick);
    public void PlayCycleEnd()    => Play(SFXLibrary.CycleEnd);
    public void PlayPhaseStart()  => Play(SFXLibrary.PhaseStart);
    public void PlayComboReveal() => Play(SFXLibrary.ComboReveal);
    public void PlaySymbolTick()  => Play(SFXLibrary.SymbolTick);
    public void PlayLose()        => Play(SFXLibrary.Lose);

    public void PlayWin(float multiplier)
    {
        if (multiplier >= 10f)     Play(SFXLibrary.Jackpot);
        else if (multiplier >= 5f) Play(SFXLibrary.BigWin);
        else if (multiplier >= 2f) Play(SFXLibrary.Win);
        else                       Play(SFXLibrary.SmallWin);
    }

    public void SetVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
    }
}
