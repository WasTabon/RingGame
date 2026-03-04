using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private const string KeyMusicVol = "setting_music_vol";
    private const string KeySFXVol = "setting_sfx_vol";
    private const string KeyVibration = "setting_vibration";
    private const string KeyTutorialDone = "tutorial_done";

    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }
    public bool VibrationEnabled { get; private set; }
    public bool TutorialDone => PlayerPrefs.GetInt(KeyTutorialDone, 0) == 1;

    public System.Action OnSettingsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        MusicVolume = PlayerPrefs.GetFloat(KeyMusicVol, 0.7f);
        SFXVolume = PlayerPrefs.GetFloat(KeySFXVol, 0.7f);
        VibrationEnabled = PlayerPrefs.GetInt(KeyVibration, 1) == 1;
        ApplyAll();
    }

    public void SetMusicVolume(float vol)
    {
        MusicVolume = Mathf.Clamp01(vol);
        PlayerPrefs.SetFloat(KeyMusicVol, MusicVolume);
        AudioManager.Instance?.SetMusicVolume(MusicVolume);
        OnSettingsChanged?.Invoke();
    }

    public void SetSFXVolume(float vol)
    {
        SFXVolume = Mathf.Clamp01(vol);
        PlayerPrefs.SetFloat(KeySFXVol, SFXVolume);
        SFXManager.Instance?.SetVolume(SFXVolume);
        OnSettingsChanged?.Invoke();
    }

    public void SetVibration(bool enabled)
    {
        VibrationEnabled = enabled;
        PlayerPrefs.SetInt(KeyVibration, enabled ? 1 : 0);
        OnSettingsChanged?.Invoke();
    }

    public void MarkTutorialDone()
    {
        PlayerPrefs.SetInt(KeyTutorialDone, 1);
        PlayerPrefs.Save();
    }

    public void ResetTutorial()
    {
        PlayerPrefs.SetInt(KeyTutorialDone, 0);
        PlayerPrefs.Save();
    }

    private void ApplyAll()
    {
        AudioManager.Instance?.SetMusicVolume(MusicVolume);
        SFXManager.Instance?.SetVolume(SFXVolume);
    }

    public void Vibrate()
    {
        if (!VibrationEnabled) return;
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) PlayerPrefs.Save();
    }
}
