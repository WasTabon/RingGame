using UnityEngine;

public static class VFXFactory
{
    private static Texture2D circleTexture;
    private static Texture2D glowTexture;
    private static Texture2D sparkTexture;
    private static Material additiveMat;
    private static Material trailMat;
    private static bool generated;

    public static Texture2D CircleTexture => circleTexture;
    public static Texture2D GlowTexture => glowTexture;
    public static Texture2D SparkTexture => sparkTexture;
    public static Material AdditiveMaterial => additiveMat;
    public static Material TrailMaterial => trailMat;

    public static void EnsureGenerated()
    {
        if (generated) return;
        generated = true;

        circleTexture = GenerateCircle(64);
        glowTexture = GenerateGlow(64);
        sparkTexture = GenerateSpark(32);

        additiveMat = new Material(Shader.Find("Mobile/Particles/Additive"));
        additiveMat.mainTexture = glowTexture;
        additiveMat.SetInt("_ZWrite", 0);
        additiveMat.renderQueue = 3100;

        trailMat = new Material(Shader.Find("Mobile/Particles/Additive"));
        trailMat.mainTexture = glowTexture;
        trailMat.SetInt("_ZWrite", 0);
        trailMat.renderQueue = 3100;
    }

    public static Sprite CreateCircleSprite()
    {
        EnsureGenerated();
        return Sprite.Create(circleTexture,
            new Rect(0, 0, circleTexture.width, circleTexture.height),
            new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite CreateGlowSprite()
    {
        EnsureGenerated();
        return Sprite.Create(glowTexture,
            new Rect(0, 0, glowTexture.width, glowTexture.height),
            new Vector2(0.5f, 0.5f), 100f);
    }

    private static Texture2D GenerateCircle(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        float center = size * 0.5f;
        float radius = size * 0.48f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float edge = Mathf.Clamp01((radius - dist) / (radius * 0.12f));
                float inner = Mathf.Clamp01((dist - radius * 0.7f) / (radius * 0.12f));
                float ring = edge * inner;
                float fill = edge * (1f - inner * 0.6f);
                float a = Mathf.Max(fill, ring);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateGlow(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        float center = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float norm = dist / center;
                float a = Mathf.Exp(-norm * norm * 3.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateSpark(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        float center = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float cross = Mathf.Max(
                    Mathf.Exp(-Mathf.Abs(dx) * 6f) * Mathf.Exp(-dist * 2f),
                    Mathf.Exp(-Mathf.Abs(dy) * 6f) * Mathf.Exp(-dist * 2f)
                );
                float core = Mathf.Exp(-dist * dist * 8f);
                float a = Mathf.Clamp01(core + cross * 0.7f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return tex;
    }

    public static ParticleSystem CreateParticleSystem(string name, Transform parent, bool worldSpace = false)
    {
        EnsureGenerated();

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.layer = parent.gameObject.layer;

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.playOnAwake = false;
        main.simulationSpace = worldSpace ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        main.maxParticles = 100;
        main.scalingMode = ParticleSystemScalingMode.Local;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled = false;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = additiveMat;
        renderer.sortingOrder = 100;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new(Color.white, 0f), new(Color.white, 1f) },
            new GradientAlphaKey[] { new(1f, 0f), new(0.8f, 0.3f), new(0f, 1f) }
        );
        col.color = gradient;

        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        return ps;
    }
}
