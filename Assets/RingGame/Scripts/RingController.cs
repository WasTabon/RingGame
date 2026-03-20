using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RingController : MonoBehaviour
{
    public enum RingState { Spinning, Highlighted, Captured, Stopped }

    private RectTransform ringBodyRect;
    private Image ringBodyImage;
    private Image ringGlowImage;
    private Image outerGlowImage;
    private Image innerGlowImage;
    private RectTransform symbolMarkerRoot;
    private RectTransform symbolIconRect;
    private Image symbolBgImage;
    private Image symbolIconImage;

    public RingState CurrentState { get; private set; }
    public SymbolConfig.SymbolType SymbolType { get; private set; }

    private float rotationSpeed;
    private float currentAngle;
    private Color baseRingColor;
    private bool initialized;
    private float musicPulseSmooth;

    private bool spawnComplete;
    private bool isDimmed;
    
    private static readonly Color HighlightColor = new Color(1f, 0.92f, 0.25f);
    private static readonly Color CapturedColor = new Color(0.25f, 1f, 0.55f);
    private static readonly Color MissedColor = new Color(1f, 0.25f, 0.25f);
    private static readonly Color StoppedColor = new Color(0.5f, 0.52f, 0.7f);

    public void Setup(RectTransform bodyRect, Image bodyImage, Image glowImage,
        RectTransform markerRoot, RectTransform iconRect, Image bgImage, Image iconImage)
    {
        ringBodyRect = bodyRect;
        ringBodyImage = bodyImage;
        ringGlowImage = glowImage;
        symbolMarkerRoot = markerRoot;
        symbolIconRect = iconRect;
        symbolBgImage = bgImage;
        symbolIconImage = iconImage;
    }

    public void SetGlowLayers(Image outer, Image inner)
    {
        outerGlowImage = outer;
        innerGlowImage = inner;
    }

    public void Init(float speed, float diameter, SymbolConfig.SymbolType symbolType,
        SymbolConfig config, Color ringColor)
    {
        rotationSpeed = speed;
        currentAngle = Random.Range(0f, 360f);
        SymbolType = symbolType;
        baseRingColor = ringColor;
        CurrentState = RingState.Spinning;

        ringBodyRect.sizeDelta = new Vector2(diameter, diameter);
        ringGlowImage.rectTransform.sizeDelta = new Vector2(diameter + 30f, diameter + 30f);

        float markerRadius = diameter * 0.5f - 10f;
        symbolMarkerRoot.anchoredPosition = new Vector2(0f, markerRadius);

        ringBodyImage.color = ringColor;
        ringGlowImage.color = new Color(ringColor.r, ringColor.g, ringColor.b, 0f);

        if (config != null)
        {
            var icon = config.GetIcon(symbolType);
            var symColor = config.GetColor(symbolType);
            symbolBgImage.color = symColor;
            if (icon != null)
                symbolIconImage.sprite = icon;
        }

        transform.localEulerAngles = new Vector3(0f, 0f, currentAngle);
        initialized = true;

        PlaySpawnAnimation();
    }

    private void PlaySpawnAnimation()
    {
        spawnComplete = false;
        var myRT = GetComponent<RectTransform>();
        myRT.localScale = Vector3.zero;
        symbolIconRect.localScale = Vector3.zero;

        myRT.DOScale(1f, 0.5f)
            .SetEase(Ease.OutBack)
            .SetDelay(Random.Range(0f, 0.12f))
            .OnComplete(() => spawnComplete = true);

        symbolIconRect.DOScale(1f, 0.4f)
            .SetEase(Ease.OutBack)
            .SetDelay(Random.Range(0.1f, 0.28f));
    }

    private void Update()
    {
        if (!initialized) return;

        if (CurrentState == RingState.Spinning || CurrentState == RingState.Highlighted)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            transform.localEulerAngles = new Vector3(0f, 0f, currentAngle);
        }

        if (symbolIconRect != null)
            symbolIconRect.rotation = Quaternion.identity;

        // пульсация управляется через BeatPulse() — вызывается от AudioManager.OnBeat
    }

    public void SetDimmed(bool dimmed)
    {
        if (isDimmed == dimmed) return;
        isDimmed = dimmed;

        if (CurrentState == RingState.Captured) return;

        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();
        cg.DOFade(dimmed ? 0.15f : 1f, 0.15f).SetEase(Ease.OutQuad);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (CurrentState == RingState.Captured || CurrentState == RingState.Stopped) return;

        CurrentState = highlighted ? RingState.Highlighted : RingState.Spinning;

        var myRT = GetComponent<RectTransform>();
        ringBodyImage.DOKill();
        ringGlowImage.DOKill();
        myRT.DOKill();
        symbolIconRect.DOKill();

        if (highlighted)
        {
            ringBodyImage.DOColor(HighlightColor, 0.1f).SetEase(Ease.OutQuad);
            ringGlowImage.DOColor(new Color(HighlightColor.r, HighlightColor.g, HighlightColor.b, 0.6f), 0.1f);
            myRT.DOScale(1.07f, 0.14f).SetEase(Ease.OutBack);
            symbolIconRect.DOPunchScale(Vector3.one * 0.22f, 0.22f, 6, 0.5f);
        }
        else
        {
            ringBodyImage.DOColor(baseRingColor, 0.22f).SetEase(Ease.OutQuad);
            ringGlowImage.DOFade(0f, 0.22f);
            myRT.DOScale(1f, 0.18f).SetEase(Ease.OutQuad);
        }
    }

    public void Capture(System.Action onComplete = null)
    {
        if (CurrentState == RingState.Captured) return;
        CurrentState = RingState.Captured;

        var myRT = GetComponent<RectTransform>();
        DOTween.Kill(myRT);
        ringBodyImage.DOKill();
        ringGlowImage.DOKill();
        symbolIconRect.DOKill();

        var seq = DOTween.Sequence();
        seq.Append(myRT.DOShakeRotation(0.2f, new Vector3(0, 0, 16f), 10, 45f, false));
        seq.Join(ringBodyImage.DOColor(Color.white, 0.06f));
        seq.Append(ringBodyImage.DOColor(CapturedColor, 0.18f).SetEase(Ease.OutQuad));
        seq.Join(ringGlowImage.DOColor(new Color(CapturedColor.r, CapturedColor.g, CapturedColor.b, 0.75f), 0.15f));
        seq.Join(myRT.DOScale(1.12f, 0.14f).SetEase(Ease.OutBack));
        seq.Append(myRT.DOScale(1f, 0.22f).SetEase(Ease.InOutSine));
        seq.Join(ringGlowImage.DOFade(0.28f, 0.3f));
        seq.Join(symbolIconRect.DOPunchScale(Vector3.one * 0.38f, 0.32f, 8, 0.4f));
        seq.OnComplete(() => onComplete?.Invoke());
    }

    public void RegisterMiss()
    {
        if (CurrentState == RingState.Captured || CurrentState == RingState.Stopped) return;

        var myRT = GetComponent<RectTransform>();
        ringBodyImage.DOKill();
        DOTween.Kill(myRT);

        var seq = DOTween.Sequence();
        seq.Append(ringBodyImage.DOColor(MissedColor, 0.07f));
        seq.Join(myRT.DOShakePosition(0.18f, new Vector2(7f, 7f), 14, 90f, false));
        seq.Append(ringBodyImage.DOColor(baseRingColor, 0.28f));
    }

    public void StopSpinning()
    {
        CurrentState = RingState.Stopped;

        var myRT = GetComponent<RectTransform>();
        ringBodyImage.DOKill();
        ringGlowImage.DOKill();
        DOTween.Kill(myRT);

        ringBodyImage.DOColor(StoppedColor, 0.3f);
        ringGlowImage.DOFade(0f, 0.25f);
        myRT.DOScale(0.95f, 0.22f).SetEase(Ease.OutQuad);
    }

    public void SpeedUp(float multiplier)
    {
        rotationSpeed *= multiplier;
    }

    public void BeatPulse()
    {
        if (CurrentState != RingState.Spinning) return;
        if (!spawnComplete) return;
        var myRT = GetComponent<RectTransform>();
        myRT.DOKill(false);
        myRT.DOPunchScale(Vector3.one * 0.15f, 0.25f, 4, 0.5f);

        ringGlowImage.DOKill(false);
        ringGlowImage.DOColor(new Color(baseRingColor.r, baseRingColor.g, baseRingColor.b, 0.45f), 0.05f)
            .OnComplete(() => ringGlowImage.DOFade(0f, 0.2f));

        if (outerGlowImage != null)
        {
            outerGlowImage.DOKill(false);
            outerGlowImage.DOFade(0.2f, 0.06f)
                .OnComplete(() => outerGlowImage.DOFade(0.08f, 0.25f));
        }
        if (innerGlowImage != null)
        {
            innerGlowImage.DOKill(false);
            innerGlowImage.DOFade(0.12f, 0.06f)
                .OnComplete(() => innerGlowImage.DOFade(0.04f, 0.25f));
        }
    }

    public void ResetForNewCycle()
    {
        CurrentState = RingState.Spinning;
        isDimmed = false;
        ringBodyImage.DOColor(baseRingColor, 0.3f);
        ringGlowImage.DOFade(0f, 0.2f);
        var myRT = GetComponent<RectTransform>();
        myRT.DOScale(1f, 0.2f);

        var cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOKill();
            cg.DOFade(1f, 0.2f);
        }
    }

    public Vector3 GetSymbolWorldPosition()
    {
        return symbolIconRect != null ? symbolIconRect.position : transform.position;
    }
}
