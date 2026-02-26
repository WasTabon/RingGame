using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Default Music")]
    [SerializeField] private AudioClip defaultMusicClip;

    [Header("Beat Detection")]
    [SerializeField][Range(0.1f, 2f)] private float beatSensitivity = 0.5f;
    [SerializeField][Range(0.05f, 0.5f)] private float beatCooldown = 0.2f;
    [SerializeField] private int spectrumSize = 64;
    [SerializeField] private FFTWindow fftWindow = FFTWindow.BlackmanHarris;

    public System.Action OnBeat;

    public float LowFreqValue { get; private set; }
    public float MidFreqValue { get; private set; }

    private float[] spectrumData;
    private float energyAverage;
    private float cooldownTimer;
    private float previousLow;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        spectrumData = new float[spectrumSize];
    }

    private void Start()
    {
        if (defaultMusicClip != null)
            PlayMusic(defaultMusicClip);
    }

    private void Update()
    {
        if (musicSource == null || !musicSource.isPlaying)
            return;

        musicSource.GetSpectrumData(spectrumData, 0, fftWindow);

        float lowEnergy = GetBandAverage(0, 4);
        float midEnergy = GetBandAverage(4, 20);

        LowFreqValue = Mathf.Lerp(LowFreqValue, Mathf.Clamp01(lowEnergy * 120f), Time.deltaTime * 20f);
        MidFreqValue = Mathf.Lerp(MidFreqValue, Mathf.Clamp01(midEnergy * 60f), Time.deltaTime * 12f);

        energyAverage = Mathf.Lerp(energyAverage, lowEnergy, Time.deltaTime * 2.5f);

        cooldownTimer -= Time.deltaTime;

        float dynamicThreshold = energyAverage * (1f + beatSensitivity * 2.5f);
        bool isSpike = lowEnergy > dynamicThreshold && lowEnergy > previousLow * 1.1f;

        if (isSpike && cooldownTimer <= 0f)
        {
            cooldownTimer = beatCooldown;
            OnBeat?.Invoke();
        }

        previousLow = lowEnergy;
    }

    private float GetBandAverage(int start, int end)
    {
        end = Mathf.Clamp(end, start + 1, spectrumSize);
        float sum = 0f;
        for (int i = start; i < end; i++)
            sum += spectrumData[i];
        return sum / (end - start);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void SetMusicVolume(float vol) => musicSource.volume = Mathf.Clamp01(vol);
    public void SetSFXVolume(float vol) => sfxSource.volume = Mathf.Clamp01(vol);
    public void SetBeatSensitivity(float value) => beatSensitivity = Mathf.Clamp(value, 0.1f, 2f);
}
