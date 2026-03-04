using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TutorialUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform panelRect;

    [Header("Content")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI pageIndicator;
    [SerializeField] private TextMeshProUGUI tapHint;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    private int currentPage;
    private System.Action onComplete;

    private static readonly string[] Titles =
    {
        "Welcome to RingGame",
        "How to Play",
        "Capturing Symbols",
        "Building Combos",
        "Stages and Rewards",
    };

    private static readonly string[] Pages =
    {
        "RingGame is a rhythm casino game where your timing skills determine your wins.\n\n" +
        "Tap to the beat, capture symbols, and match them for big payouts.",

        "When a round starts, rings spin around the screen. Each ring carries a symbol.\n\n" +
        "A yellow shrinking circle will appear on one ring at a time. " +
        "This is your timing window. Tap anywhere on the screen before it closes to capture that ring.",

        "Each successful tap captures the symbol on that ring and adds it to the grid at the bottom of the screen.\n\n" +
        "If you miss the timing window, you lose an attempt. " +
        "You get 4 attempts per cycle and there are 4 cycles per round.",

        "Matching symbols in the grid means bigger payouts. " +
        "Two of the same symbol is a small win. Three or four is much better.\n\n" +
        "Wild symbols can substitute for any other symbol to complete a match. " +
        "They appear in cycles 3 and 4.\n\n" +
        "At the end of the round, your combos are revealed with lines connecting matching symbols.",

        "Winning a round advances you to the next stage. " +
        "Higher stages add more rings and tighten the timing windows, " +
        "but the potential rewards are bigger.\n\n" +
        "Set your bet amount before each round. " +
        "Your payout is your bet multiplied by the combo multiplier.\n\n" +
        "Good luck and trust your rhythm.",
    };

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextPage);
        }
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(Skip);
        }

        gameObject.SetActive(false);
    }

    public void Show(System.Action onDone)
    {
        onComplete = onDone;
        currentPage = 0;
        gameObject.SetActive(true);

        panelGroup.alpha = 0f;
        panelRect.localScale = Vector3.one * 0.9f;

        var seq = DOTween.Sequence();
        seq.Append(panelGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
        seq.Join(panelRect.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        seq.OnComplete(() => ShowPage(0));
    }

    private void ShowPage(int index)
    {
        currentPage = index;

        if (titleText != null)
        {
            titleText.text = Titles[index];
            titleText.alpha = 0f;
            titleText.DOFade(1f, 0.2f);
        }

        if (bodyText != null)
        {
            bodyText.text = Pages[index];
            bodyText.alpha = 0f;
            bodyText.DOFade(1f, 0.25f).SetDelay(0.05f);
        }

        if (pageIndicator != null)
            pageIndicator.text = (index + 1) + " / " + Pages.Length;

        bool isLast = index >= Pages.Length - 1;
        if (tapHint != null)
            tapHint.text = isLast ? "TAP TO START" : "TAP TO CONTINUE";

        if (nextButton != null)
        {
            var btnText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = isLast ? "START" : "NEXT";
        }
    }

    private void NextPage()
    {
        SFXManager.Instance?.PlayButtonClick();

        if (currentPage >= Pages.Length - 1)
        {
            Complete();
            return;
        }

        bodyText?.DOKill();
        titleText?.DOKill();

        var seq = DOTween.Sequence();
        if (bodyText != null)
            seq.Append(bodyText.DOFade(0f, 0.12f));
        if (titleText != null)
            seq.Join(titleText.DOFade(0f, 0.12f));
        seq.OnComplete(() => ShowPage(currentPage + 1));
    }

    private void Skip()
    {
        SFXManager.Instance?.PlayButtonClick();
        Complete();
    }

    private void Complete()
    {
        SettingsManager.Instance?.MarkTutorialDone();

        var seq = DOTween.Sequence();
        seq.Append(panelGroup.DOFade(0f, 0.25f).SetEase(Ease.InQuad));
        seq.Join(panelRect.DOScale(0.9f, 0.25f).SetEase(Ease.InQuad));
        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }
}
