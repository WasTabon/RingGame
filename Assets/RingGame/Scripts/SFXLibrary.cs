using UnityEngine;

public static class SFXLibrary
{
    private const int SampleRate = 44100;

    public static AudioClip Hit         { get; private set; }
    public static AudioClip Miss        { get; private set; }
    public static AudioClip Capture     { get; private set; }
    public static AudioClip ButtonClick { get; private set; }
    public static AudioClip BetTick     { get; private set; }
    public static AudioClip CycleEnd    { get; private set; }
    public static AudioClip PhaseStart  { get; private set; }
    public static AudioClip ComboReveal { get; private set; }
    public static AudioClip SymbolTick  { get; private set; }
    public static AudioClip SmallWin    { get; private set; }
    public static AudioClip Win         { get; private set; }
    public static AudioClip BigWin      { get; private set; }
    public static AudioClip Jackpot     { get; private set; }
    public static AudioClip Lose        { get; private set; }

    private static bool generated;

    public static void Generate()
    {
        if (generated) return;
        generated = true;

        Hit         = CreateClip("SFX_Hit",         0.08f, (t) => RisingTone(t, 800f, 1400f, 0.08f));
        Miss        = CreateClip("SFX_Miss",        0.15f, (t) => Buzz(t, 180f, 0.15f));
        Capture     = CreateClip("SFX_Capture",     0.18f, (t) => Chime(t, 0.18f));
        ButtonClick = CreateClip("SFX_Click",       0.04f, (t) => Click(t));
        BetTick     = CreateClip("SFX_BetTick",     0.05f, (t) => Tick(t, 1200f));
        CycleEnd    = CreateClip("SFX_CycleEnd",    0.25f, (t) => Sweep(t, 600f, 1000f, 0.25f));
        PhaseStart  = CreateClip("SFX_PhaseStart",  0.3f,  (t) => Countdown(t, 0.3f));
        ComboReveal = CreateClip("SFX_ComboReveal", 0.2f,  (t) => Whoosh(t, 0.2f));
        SymbolTick  = CreateClip("SFX_SymbolTick",  0.06f, (t) => Tick(t, 900f));
        SmallWin    = CreateClip("SFX_SmallWin",    0.4f,  (t) => Jingle(t, 0.4f, 523f));
        Win         = CreateClip("SFX_Win",         0.6f,  (t) => Jingle(t, 0.6f, 659f));
        BigWin      = CreateClip("SFX_BigWin",      0.8f,  (t) => Fanfare(t, 0.8f, 784f));
        Jackpot     = CreateClip("SFX_Jackpot",     1.2f,  (t) => Fanfare(t, 1.2f, 1047f));
        Lose        = CreateClip("SFX_Lose",        0.35f, (t) => SadTone(t, 0.35f));
    }

    private static AudioClip CreateClip(string name, float duration, System.Func<float, float> generator)
    {
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        var clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            data[i] = Mathf.Clamp(generator(t), -1f, 1f);
        }

        clip.SetData(data, 0);
        return clip;
    }

    private static float Sin(float t, float freq)
    {
        return Mathf.Sin(2f * Mathf.PI * freq * t);
    }

    private static float Env(float t, float dur, float attack = 0.01f, float release = 0.05f)
    {
        if (t < attack) return t / attack;
        if (t > dur - release) return (dur - t) / release;
        return 1f;
    }

    private static float RisingTone(float t, float startFreq, float endFreq, float dur)
    {
        float freq = Mathf.Lerp(startFreq, endFreq, t / dur);
        return Sin(t, freq) * 0.5f * Env(t, dur, 0.005f, 0.02f);
    }

    private static float Buzz(float t, float freq, float dur)
    {
        float saw = (t * freq % 1f) * 2f - 1f;
        float noise = (Random.value * 2f - 1f) * 0.15f;
        float drop = Mathf.Lerp(freq, freq * 0.6f, t / dur);
        return (Sin(t, drop) * 0.3f + saw * 0.15f + noise) * Env(t, dur, 0.005f, 0.06f);
    }

    private static float Chime(float t, float dur)
    {
        float f1 = 880f;
        float f2 = 1320f;
        float f3 = 1760f;
        float e = Env(t, dur, 0.005f, 0.08f);
        float decay = Mathf.Exp(-t * 12f);
        return (Sin(t, f1) * 0.35f + Sin(t, f2) * 0.25f + Sin(t, f3) * 0.15f) * e * decay;
    }

    private static float Click(float t)
    {
        float e = Mathf.Exp(-t * 200f);
        return Sin(t, 1800f) * e * 0.4f + Sin(t, 3600f) * e * 0.2f;
    }

    private static float Tick(float t, float freq)
    {
        float e = Mathf.Exp(-t * 120f);
        return Sin(t, freq) * e * 0.35f;
    }

    private static float Sweep(float t, float startFreq, float endFreq, float dur)
    {
        float freq = Mathf.Lerp(startFreq, endFreq, t / dur);
        return Sin(t, freq) * 0.35f * Env(t, dur, 0.02f, 0.08f);
    }

    private static float Countdown(float t, float dur)
    {
        float step = dur / 3f;
        int note = Mathf.FloorToInt(t / step);
        float[] freqs = { 440f, 523f, 659f };
        float freq = note < 3 ? freqs[note] : 659f;
        float localT = t - note * step;
        float e = Mathf.Exp(-localT * 15f);
        return Sin(t, freq) * e * 0.4f;
    }

    private static float Whoosh(float t, float dur)
    {
        float freq = Mathf.Lerp(200f, 2000f, t / dur);
        float noise = (Mathf.PerlinNoise(t * 800f, 0f) * 2f - 1f) * 0.3f;
        return (Sin(t, freq) * 0.2f + noise) * Env(t, dur, 0.03f, 0.06f);
    }

    private static float Jingle(float t, float dur, float baseFreq)
    {
        float noteLen = dur / 4f;
        int note = Mathf.Min(Mathf.FloorToInt(t / noteLen), 3);
        float[] ratios = { 1f, 1.25f, 1.5f, 2f };
        float freq = baseFreq * ratios[note];
        float localT = t - note * noteLen;
        float e = Mathf.Exp(-localT * 8f);
        return (Sin(t, freq) * 0.35f + Sin(t, freq * 2f) * 0.1f) * e * Env(t, dur);
    }

    private static float Fanfare(float t, float dur, float baseFreq)
    {
        float noteLen = dur / 6f;
        int note = Mathf.Min(Mathf.FloorToInt(t / noteLen), 5);
        float[] ratios = { 1f, 1.25f, 1.5f, 1.25f, 1.5f, 2f };
        float freq = baseFreq * ratios[note];
        float localT = t - note * noteLen;
        float e = Mathf.Exp(-localT * 6f);
        float harmonics = Sin(t, freq) * 0.3f + Sin(t, freq * 1.5f) * 0.15f + Sin(t, freq * 2f) * 0.1f;
        return harmonics * e * Env(t, dur, 0.01f, 0.15f);
    }

    private static float SadTone(float t, float dur)
    {
        float freq = Mathf.Lerp(440f, 220f, t / dur);
        float e = Env(t, dur, 0.02f, 0.1f);
        return (Sin(t, freq) * 0.3f + Sin(t, freq * 0.5f) * 0.15f) * e;
    }
}
