using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Container (auto-created if null)")]
    [SerializeField] private RectTransform vfxContainer;

    [Header("Settings")]
    [SerializeField] private int poolSize = 80;

    private Sprite glowSprite;
    private Sprite circleSprite;
    private Queue<Image> particlePool = new Queue<Image>();
    private Canvas rootCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        VFXFactory.EnsureGenerated();
        glowSprite = VFXFactory.CreateGlowSprite();
        circleSprite = VFXFactory.CreateCircleSprite();

        EnsureContainer();
        WarmPool();
    }

    private void EnsureContainer()
    {
        if (vfxContainer != null) return;

        var canvasGO = new GameObject("VFXCanvas");
        canvasGO.transform.SetParent(transform, false);
        rootCanvas = canvasGO.AddComponent<Canvas>();
        rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootCanvas.sortingOrder = 200;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);

        var containerGO = new GameObject("Particles");
        containerGO.transform.SetParent(canvasGO.transform, false);
        vfxContainer = containerGO.AddComponent<RectTransform>();
        vfxContainer.anchorMin = Vector2.zero;
        vfxContainer.anchorMax = Vector2.one;
        vfxContainer.offsetMin = Vector2.zero;
        vfxContainer.offsetMax = Vector2.zero;
    }

    private void WarmPool()
    {
        for (int i = 0; i < poolSize; i++)
            particlePool.Enqueue(CreateParticle());
    }

    private Image CreateParticle()
    {
        var go = new GameObject("P", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(vfxContainer, false);
        var img = go.GetComponent<Image>();
        img.sprite = glowSprite;
        img.raycastTarget = false;
        img.color = Color.clear;
        go.SetActive(false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(20f, 20f);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        return img;
    }

    private Image GetParticle()
    {
        Image p;
        if (particlePool.Count > 0)
        {
            p = particlePool.Dequeue();
        }
        else
        {
            p = CreateParticle();
        }

        p.gameObject.SetActive(true);
        p.transform.SetAsLastSibling();
        return p;
    }

    private void ReturnParticle(Image p)
    {
        if (p == null) return;
        p.DOKill();
        p.rectTransform.DOKill();
        p.color = Color.clear;
        p.gameObject.SetActive(false);
        p.rectTransform.localScale = Vector3.one;
        p.rectTransform.localRotation = Quaternion.identity;
        particlePool.Enqueue(p);
    }

    private Vector2 WorldToVFX(Vector3 worldPos)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            vfxContainer, screenPos, null, out Vector2 localPos);
        return localPos;
    }

    public void SpawnCaptureBurst(Vector3 worldPos, Color color)
    {
        Vector2 pos = WorldToVFX(worldPos);
        int count = 12;

        for (int i = 0; i < count; i++)
        {
            var p = GetParticle();
            p.sprite = glowSprite;
            float angle = (360f / count) * i + Random.Range(-15f, 15f);
            float rad = angle * Mathf.Deg2Rad;
            float dist = Random.Range(60f, 130f);
            float size = Random.Range(14f, 28f);
            float duration = Random.Range(0.4f, 0.65f);

            p.rectTransform.anchoredPosition = pos;
            p.rectTransform.sizeDelta = new Vector2(size, size);
            p.rectTransform.localScale = Vector3.one * 0.3f;
            p.color = new Color(color.r, color.g, color.b, 0.9f);

            Vector2 target = pos + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * dist;

            var seq = DOTween.Sequence();
            seq.Append(p.rectTransform.DOScale(1.2f, duration * 0.25f).SetEase(Ease.OutQuad));
            seq.Join(p.rectTransform.DOAnchorPos(target, duration).SetEase(Ease.OutCubic));
            seq.Join(p.rectTransform.DOLocalRotate(new Vector3(0, 0, Random.Range(-180f, 180f)), duration, RotateMode.FastBeyond360));
            seq.Insert(duration * 0.3f, p.rectTransform.DOScale(0f, duration * 0.7f).SetEase(Ease.InQuad));
            seq.Insert(duration * 0.5f, p.DOFade(0f, duration * 0.5f));
            seq.OnComplete(() => ReturnParticle(p));
        }

        SpawnFlash(pos, color, 50f, 0.15f);
    }

    public void SpawnHitSpark(Vector3 worldPos, Color color)
    {
        Vector2 pos = WorldToVFX(worldPos);
        SpawnFlash(pos, color, 40f, 0.12f);

        for (int i = 0; i < 4; i++)
        {
            var p = GetParticle();
            p.sprite = circleSprite;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(25f, 50f);
            float size = Random.Range(6f, 12f);

            p.rectTransform.anchoredPosition = pos;
            p.rectTransform.sizeDelta = new Vector2(size, size);
            p.rectTransform.localScale = Vector3.one;
            p.color = new Color(color.r, color.g, color.b, 0.8f);

            Vector2 target = pos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            var seq = DOTween.Sequence();
            seq.Append(p.rectTransform.DOAnchorPos(target, 0.25f).SetEase(Ease.OutCubic));
            seq.Join(p.DOFade(0f, 0.25f).SetEase(Ease.InQuad));
            seq.Join(p.rectTransform.DOScale(0f, 0.25f).SetEase(Ease.InQuad));
            seq.OnComplete(() => ReturnParticle(p));
        }
    }

    private void SpawnFlash(Vector2 pos, Color color, float size, float duration)
    {
        var p = GetParticle();
        p.sprite = glowSprite;
        p.rectTransform.anchoredPosition = pos;
        p.rectTransform.sizeDelta = new Vector2(size * 0.3f, size * 0.3f);
        p.rectTransform.localScale = Vector3.one;
        p.color = new Color(color.r, color.g, color.b, 0.95f);

        var seq = DOTween.Sequence();
        seq.Append(p.rectTransform.DOSizeDelta(new Vector2(size, size), duration * 0.3f).SetEase(Ease.OutQuad));
        seq.Join(p.DOFade(0.6f, duration * 0.3f));
        seq.Append(p.DOFade(0f, duration * 0.7f).SetEase(Ease.InQuad));
        seq.Join(p.rectTransform.DOSizeDelta(new Vector2(size * 1.3f, size * 1.3f), duration * 0.7f));
        seq.OnComplete(() => ReturnParticle(p));
    }

    public void SpawnTrailDot(Vector3 worldPos, Color color, float size = 10f)
    {
        Vector2 pos = WorldToVFX(worldPos);
        var p = GetParticle();
        p.sprite = glowSprite;
        p.rectTransform.anchoredPosition = pos;
        p.rectTransform.sizeDelta = new Vector2(size, size);
        p.rectTransform.localScale = Vector3.one;
        p.color = new Color(color.r, color.g, color.b, 0.6f);

        var seq = DOTween.Sequence();
        seq.Append(p.DOFade(0f, 0.35f).SetEase(Ease.InQuad));
        seq.Join(p.rectTransform.DOScale(0.2f, 0.35f).SetEase(Ease.InQuad));
        seq.OnComplete(() => ReturnParticle(p));
    }

    public void SpawnComboLineGlow(Vector3 worldPosA, Vector3 worldPosB, Color color)
    {
        Vector2 a = WorldToVFX(worldPosA);
        Vector2 b = WorldToVFX(worldPosB);

        int dots = 5;
        for (int i = 0; i < dots; i++)
        {
            float t = (float)i / (dots - 1);
            Vector2 pos = Vector2.Lerp(a, b, t);
            float delay = t * 0.15f;

            var p = GetParticle();
            p.sprite = glowSprite;
            p.rectTransform.anchoredPosition = pos;
            p.rectTransform.sizeDelta = new Vector2(16f, 16f);
            p.rectTransform.localScale = Vector3.zero;
            p.color = new Color(color.r, color.g, color.b, 0.8f);

            var seq = DOTween.Sequence();
            seq.SetDelay(delay);
            seq.Append(p.rectTransform.DOScale(1.3f, 0.15f).SetEase(Ease.OutBack));
            seq.Append(p.rectTransform.DOScale(0f, 0.3f).SetEase(Ease.InQuad));
            seq.Join(p.DOFade(0f, 0.3f));
            seq.OnComplete(() => ReturnParticle(p));
        }
    }

    public void SpawnWinCelebration(float intensity)
    {
        int count;
        float spread;

        if (intensity >= 10f)      { count = 50; spread = 600f; }
        else if (intensity >= 5f)  { count = 35; spread = 500f; }
        else if (intensity >= 2f)  { count = 20; spread = 400f; }
        else                       { count = 12; spread = 300f; }

        StartCoroutine(WinCelebrationSequence(count, spread, intensity));
    }

    private IEnumerator WinCelebrationSequence(int count, float spread, float intensity)
    {
        Color[] colors = {
            new Color(1f, 0.82f, 0.15f),
            new Color(1f, 0.4f, 0.3f),
            new Color(0.3f, 1f, 0.55f),
            new Color(0.4f, 0.6f, 1f),
            new Color(1f, 0.6f, 0.9f),
            Color.white
        };

        int waves = intensity >= 5f ? 3 : (intensity >= 2f ? 2 : 1);

        for (int w = 0; w < waves; w++)
        {
            int waveCount = count / waves;
            for (int i = 0; i < waveCount; i++)
            {
                var p = GetParticle();
                p.sprite = Random.value > 0.5f ? glowSprite : circleSprite;

                Color c = colors[Random.Range(0, colors.Length)];
                float size = Random.Range(8f, 24f);
                float startX = Random.Range(-spread, spread);
                float startY = Random.Range(400f, 700f);
                float endY = Random.Range(-800f, -400f);
                float duration = Random.Range(1.2f, 2.2f);
                float drift = Random.Range(-80f, 80f);

                p.rectTransform.anchoredPosition = new Vector2(startX, startY);
                p.rectTransform.sizeDelta = new Vector2(size, size);
                p.rectTransform.localScale = Vector3.one;
                p.color = new Color(c.r, c.g, c.b, 0.9f);

                var seq = DOTween.Sequence();
                seq.Append(p.rectTransform.DOAnchorPosY(endY, duration).SetEase(Ease.InQuad));
                seq.Join(p.rectTransform.DOAnchorPosX(startX + drift, duration).SetEase(Ease.InOutSine));
                seq.Join(p.rectTransform.DOLocalRotate(
                    new Vector3(0, 0, Random.Range(-360f, 360f)), duration, RotateMode.FastBeyond360));
                seq.Insert(duration * 0.6f, p.DOFade(0f, duration * 0.4f));
                seq.Insert(duration * 0.7f, p.rectTransform.DOScale(0.3f, duration * 0.3f));
                seq.OnComplete(() => ReturnParticle(p));
            }
            yield return new WaitForSeconds(0.25f);
        }

        if (intensity >= 5f)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = (360f / 6) * i * Mathf.Deg2Rad;
                SpawnFlash(
                    new Vector2(Mathf.Cos(angle) * 150f, Mathf.Sin(angle) * 150f),
                    colors[i % colors.Length], 80f, 0.4f);
            }
        }
    }

    public void SpawnCycleEndEffect()
    {
        Color color = new Color(1f, 0.82f, 0.15f);
        int count = 8;
        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            var p = GetParticle();
            p.sprite = glowSprite;

            float dist = Random.Range(100f, 200f);
            Vector2 start = Vector2.zero;
            Vector2 end = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            p.rectTransform.anchoredPosition = start;
            p.rectTransform.sizeDelta = new Vector2(18f, 18f);
            p.rectTransform.localScale = Vector3.one * 0.5f;
            p.color = new Color(color.r, color.g, color.b, 0.8f);

            var seq = DOTween.Sequence();
            seq.Append(p.rectTransform.DOAnchorPos(end, 0.35f).SetEase(Ease.OutCubic));
            seq.Join(p.rectTransform.DOScale(0f, 0.35f).SetEase(Ease.InQuad));
            seq.Join(p.DOFade(0f, 0.35f));
            seq.OnComplete(() => ReturnParticle(p));
        }
    }

    public void ShakeCanvas(RectTransform target, float intensity = 1f)
    {
        if (target == null) return;
        target.DOKill();
        float strength = 8f * intensity;
        target.DOShakeAnchorPos(0.35f, new Vector2(strength, strength * 0.7f), 12, 60f, false, true, ShakeRandomnessMode.Full)
            .OnComplete(() => target.anchoredPosition = Vector2.zero);
    }

    private Coroutine trailCoroutine;

    public void StartShrinkTrail(RectTransform target, Color color)
    {
        StopShrinkTrail();
        trailCoroutine = StartCoroutine(ShrinkTrailRoutine(target, color));
    }

    public void StopShrinkTrail()
    {
        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }
    }

    private IEnumerator ShrinkTrailRoutine(RectTransform target, Color color)
    {
        float interval = 0.03f;
        while (target != null && target.gameObject.activeInHierarchy)
        {
            float radius = target.sizeDelta.x * 0.5f;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius * 0.4f, Mathf.Sin(angle) * radius * 0.4f, 0f);

            SpawnTrailDot(target.position + offset, color, Random.Range(6f, 14f));
            yield return new WaitForSeconds(interval);
        }
    }

    public void SpawnSymbolRevealTick(Vector3 worldPos)
    {
        Vector2 pos = WorldToVFX(worldPos);
        SpawnFlash(pos, new Color(1f, 0.92f, 0.3f), 30f, 0.2f);
    }
}
