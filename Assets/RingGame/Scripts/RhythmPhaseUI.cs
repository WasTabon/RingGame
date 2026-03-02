using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class RhythmPhaseUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup rootGroup;

    [Header("Top Bar")]
    [SerializeField] private RectTransform topBar;
    [SerializeField] private TextMeshProUGUI cycleText;
    [SerializeField] private TextMeshProUGUI betAmountText;
    [SerializeField] private RectTransform attemptsRow;
    [SerializeField] private Image[] attemptDots;

    [Header("Rings Area")]
    [SerializeField] private RectTransform ringsArea;

    [Header("Shrinking Ring")]
    [SerializeField] private RectTransform shrinkingRing;
    [SerializeField] private Image shrinkingRingImage;

    [Header("Hit/Miss Feedback")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private RectTransform feedbackRect;

    [Header("Cycle Flash")]
    [SerializeField] private Image flashOverlay;

    [Header("Grid Placeholder")]
    [SerializeField] private RectTransform gridArea;
    [SerializeField] private CanvasGroup gridGroup;

    [Header("Dev Back Button")]
    [SerializeField] private Button backButton;

    private Coroutine shrinkCoroutine;
    private Tween feedbackTween;

    private static readonly Color HitColor = new Color(0.3f, 1f, 0.55f);
    private static readonly Color MissColor = new Color(1f, 0.28f, 0.28f);
    private static readonly Color CycleColor = new Color(1f, 0.82f, 0.15f);

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => RhythmPhaseController.Instance?.OnBackToBet());
        }

        if (shrinkingRing != null) shrinkingRing.gameObject.SetActive(false);
        if (feedbackText != null) feedbackText.alpha = 0f;
        if (flashOverlay != null) flashOverlay.color = new Color(1f, 1f, 1f, 0f);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        rootGroup.alpha = 0f;

        var seq = DOTween.Sequence();
        seq.Append(rootGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
        seq.Join(topBar.DOAnchorPosY(topBar.anchoredPosition.y - 20f, 0.35f).SetEase(Ease.OutCubic));
    }

    public void Hide()
    {
        if (!gameObject.activeSelf) return;
        StopShrinkCoroutine();
        rootGroup.DOFade(0f, 0.22f).OnComplete(() => gameObject.SetActive(false));
    }

    public void UpdateCycleDisplay(int cycle, int total, bool animate)
    {
        if (cycleText == null) return;
        cycleText.text = $"CYCLE {cycle}/{total}";

        if (animate)
        {
            cycleText.rectTransform.DOKill();
            cycleText.rectTransform.DOPunchScale(Vector3.one * 0.18f, 0.28f, 5, 0.5f);
            cycleText.DOFade(0f, 0.1f).OnComplete(() => cycleText.DOFade(1f, 0.15f));
        }

        if (betAmountText != null && BetManager.Instance != null)
            betAmountText.text = "$" + BetManager.Instance.CurrentBet.ToString("N0");
    }

    public void UpdateAttempts(int remaining, bool animate)
    {
        if (attemptDots == null) return;
        for (int i = 0; i < attemptDots.Length; i++)
        {
            bool active = i < remaining;
            var dot = attemptDots[i];

            if (animate && !active && dot.color.a > 0.5f)
            {
                dot.DOFade(0.18f, 0.2f);
                dot.rectTransform.DOPunchScale(Vector3.one * -0.25f, 0.22f, 4, 0.5f);
            }
            else
            {
                dot.color = new Color(dot.color.r, dot.color.g, dot.color.b, active ? 1f : 0.18f);
                dot.rectTransform.localScale = Vector3.one;
            }
        }
    }

    public void ShowShrinkingRing(int ringIndex, float duration)
    {
        if (shrinkingRing == null || RingsManager.Instance == null) return;

        StopShrinkCoroutine();

        var rings = RingsManager.Instance.ActiveRings;
        if (ringIndex >= rings.Count) return;

        var targetRing = rings[ringIndex];
        var targetRT = targetRing.GetComponent<RectTransform>();

        shrinkingRing.gameObject.SetActive(true);
        shrinkingRingImage.color = new Color(1f, 0.92f, 0.25f, 0.9f);

        float startSize = targetRT.sizeDelta.x * 1.7f;
        float endSize = targetRT.sizeDelta.x * 1.05f;

        shrinkingRing.position = targetRT.position;
        shrinkingRing.sizeDelta = new Vector2(startSize, startSize);

        shrinkingRing.DOKill();
        shrinkingRing.DOSizeDelta(new Vector2(endSize, endSize), duration)
            .SetEase(Ease.InCubic);

        shrinkingRingImage.DOFade(0.9f, duration * 0.2f)
            .OnComplete(() => shrinkingRingImage.DOFade(0.5f, duration * 0.8f));
    }

    public void HideShrinkingRing()
    {
        StopShrinkCoroutine();
        if (shrinkingRing == null) return;
        shrinkingRing.DOKill();
        shrinkingRingImage.DOFade(0f, 0.1f)
            .OnComplete(() => shrinkingRing.gameObject.SetActive(false));
    }

    public void ShowHitFeedback()
    {
        ShowFeedback("HIT!", HitColor);
        FlashScreen(new Color(HitColor.r, HitColor.g, HitColor.b, 0.12f));
    }

    public void ShowMissFeedback()
    {
        ShowFeedback("MISS", MissColor);
        FlashScreen(new Color(MissColor.r, MissColor.g, MissColor.b, 0.15f));
    }

    private void ShowFeedback(string text, Color color)
    {
        if (feedbackText == null) return;

        feedbackTween?.Kill();
        feedbackText.text = text;
        feedbackText.color = new Color(color.r, color.g, color.b, 0f);
        feedbackRect.localScale = Vector3.one * 0.7f;
        feedbackRect.anchoredPosition = new Vector2(0f, 0f);

        var seq = DOTween.Sequence();
        seq.Append(feedbackText.DOFade(1f, 0.08f));
        seq.Join(feedbackRect.DOScale(1.1f, 0.12f).SetEase(Ease.OutBack));
        seq.Append(feedbackRect.DOAnchorPosY(40f, 0.35f).SetEase(Ease.OutCubic));
        seq.Join(feedbackText.DOFade(0f, 0.25f).SetDelay(0.1f));
        feedbackTween = seq;
    }

    public void ShowCapturedSymbol(int ringIndex, SymbolConfig.SymbolType symbol)
    {
        if (RingsManager.Instance == null) return;
        var rings = RingsManager.Instance.ActiveRings;
        if (ringIndex >= rings.Count) return;
    }

    public void ShowCycleCompleteEffect(int cycleNumber)
    {
        if (cycleText != null)
        {
            cycleText.DOKill();
            cycleText.rectTransform.DOPunchScale(Vector3.one * 0.25f, 0.35f, 6, 0.4f);
        }

        FlashScreen(new Color(CycleColor.r, CycleColor.g, CycleColor.b, 0.18f));
    }

    public void ShowPhaseComplete(System.Action onComplete)
    {
        if (flashOverlay == null) { onComplete?.Invoke(); return; }

        var seq = DOTween.Sequence();
        seq.Append(flashOverlay.DOColor(new Color(1f, 0.82f, 0.15f, 0f), 0f));
        seq.Append(flashOverlay.DOFade(0.35f, 0.25f).SetEase(Ease.OutQuad));
        seq.Append(rootGroup.DOFade(0f, 0.4f).SetDelay(0.3f));
        seq.OnComplete(() => onComplete?.Invoke());
    }

    private void FlashScreen(Color color)
    {
        if (flashOverlay == null) return;
        flashOverlay.DOKill();
        flashOverlay.color = color;
        flashOverlay.DOFade(0f, 0.3f).SetEase(Ease.OutQuad);
    }

    public void PulseBeatFeedback()
    {
        if (topBar == null) return;
        topBar.DOKill(false);
        topBar.DOPunchScale(Vector3.one * 0.018f, 0.1f, 3, 0.5f);
    }

    private void StopShrinkCoroutine()
    {
        if (shrinkCoroutine != null)
        {
            StopCoroutine(shrinkCoroutine);
            shrinkCoroutine = null;
        }
    }
}
