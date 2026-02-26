using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    [Header("Grid Placeholder")]
    [SerializeField] private RectTransform gridArea;
    [SerializeField] private CanvasGroup gridGroup;

    [Header("Dev Back Button")]
    [SerializeField] private Button backButton;

    private int currentCycle = 1;
    private int totalCycles = 4;
    private int remainingAttempts = 4;

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => RhythmPhaseController.Instance?.OnBackToBet());
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        rootGroup.alpha = 0f;
        topBar.anchoredPosition += new Vector2(0f, 20f);
        gridArea.anchoredPosition += new Vector2(0f, -20f);

        UpdateCycleDisplay(1, 4, false);
        UpdateAttempts(4, false);
        UpdateBetDisplay(false);

        var seq = DOTween.Sequence();
        seq.Append(rootGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
        seq.Join(topBar.DOAnchorPosY(topBar.anchoredPosition.y - 20f, 0.35f).SetEase(Ease.OutCubic));
        seq.Join(gridArea.DOAnchorPosY(gridArea.anchoredPosition.y + 20f, 0.35f)
            .SetEase(Ease.OutCubic).SetDelay(0.05f));
    }

    public void Hide()
    {
        if (!gameObject.activeSelf) return;

        rootGroup.DOFade(0f, 0.22f).OnComplete(() => gameObject.SetActive(false));
    }

    public void UpdateCycleDisplay(int cycle, int total, bool animate)
    {
        currentCycle = cycle;
        totalCycles = total;

        if (cycleText == null) return;
        cycleText.text = $"CYCLE {cycle}/{total}";

        if (animate)
        {
            cycleText.rectTransform.DOKill();
            cycleText.rectTransform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 5, 0.5f);
        }
    }

    public void UpdateAttempts(int remaining, bool animate)
    {
        remainingAttempts = remaining;
        if (attemptDots == null) return;

        for (int i = 0; i < attemptDots.Length; i++)
        {
            bool active = i < remaining;
            var dot = attemptDots[i];

            if (animate && !active && dot.color.a > 0.5f)
            {
                int idx = i;
                dot.DOFade(0.2f, 0.18f).SetEase(Ease.OutQuad);
                dot.rectTransform.DOPunchScale(Vector3.one * -0.2f, 0.2f, 4, 0.5f);
            }
            else
            {
                dot.color = new Color(dot.color.r, dot.color.g, dot.color.b, active ? 1f : 0.2f);
            }
        }
    }

    private void UpdateBetDisplay(bool animate)
    {
        if (betAmountText == null) return;
        if (BetManager.Instance != null)
            betAmountText.text = "$" + BetManager.Instance.CurrentBet.ToString("N0");
    }

    public void PulseBeatFeedback()
    {
        if (topBar == null) return;
        topBar.DOKill();
        topBar.DOPunchScale(Vector3.one * 0.02f, 0.1f, 3, 0.5f);
    }
}
