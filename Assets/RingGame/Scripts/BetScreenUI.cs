using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BetScreenUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup rootGroup;
    [SerializeField] private RectTransform rootRect;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI balanceLabel;
    [SerializeField] private TextMeshProUGUI balanceValue;
    [SerializeField] private RectTransform headerRect;

    [Header("Bet Display")]
    [SerializeField] private RectTransform betDisplayRect;
    [SerializeField] private TextMeshProUGUI betLabel;
    [SerializeField] private TextMeshProUGUI betValue;
    [SerializeField] private Image betDisplayBg;

    [Header("Bet Controls")]
    [SerializeField] private Button decreaseBtn;
    [SerializeField] private RectTransform decreaseBtnRect;
    [SerializeField] private Button increaseBtn;
    [SerializeField] private RectTransform increaseBtnRect;

    [Header("Preset Buttons")]
    [SerializeField] private Button minBtn;
    [SerializeField] private Button halfBtn;
    [SerializeField] private Button maxBtn;
    [SerializeField] private RectTransform presetRow;

    [Header("Start Button")]
    [SerializeField] private Button startBtn;
    [SerializeField] private RectTransform startBtnRect;
    [SerializeField] private CanvasGroup startBtnGroup;
    [SerializeField] private TextMeshProUGUI startBtnText;
    [SerializeField] private Image startBtnImage;

    [Header("Stage Display")]
    [SerializeField] private TMPro.TextMeshProUGUI stageLabel;
    [SerializeField] private TMPro.TextMeshProUGUI stageRingsLabel;

    [Header("Bet Slider Visual")]
    [SerializeField] private RectTransform sliderFill;
    [SerializeField] private RectTransform sliderTrack;

    private bool canInteract;
    private float displayedBalance;
    private float displayedBet;

    private void Start()
    {
        canInteract = false;
        displayedBalance = BalanceManager.Instance.Balance;
        displayedBet = BetManager.Instance.CurrentBet;

        SetupListeners();
        UpdateBalanceDisplay(BalanceManager.Instance.Balance, false);
        UpdateBetDisplay(BetManager.Instance.CurrentBet, false);
        UpdateSlider(false);
        UpdateStartButton(false);
        UpdateStageDisplay(false);
        PlayEnterAnimation();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (BalanceManager.Instance != null)
        {
            BalanceManager.Instance.OnBalanceChanged -= OnBalanceChanged;
            BalanceManager.Instance.OnBalanceChanged += OnBalanceChanged;
        }
        if (BetManager.Instance != null)
        {
            BetManager.Instance.OnBetChanged -= OnBetChanged;
            BetManager.Instance.OnBetChanged += OnBetChanged;
        }
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged -= OnStageChanged;
            StageManager.Instance.OnStageChanged += OnStageChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (BalanceManager.Instance != null)
            BalanceManager.Instance.OnBalanceChanged -= OnBalanceChanged;
        if (BetManager.Instance != null)
            BetManager.Instance.OnBetChanged -= OnBetChanged;
        if (StageManager.Instance != null)
            StageManager.Instance.OnStageChanged -= OnStageChanged;
    }

    private void SetupListeners()
    {
        decreaseBtn.onClick.RemoveAllListeners();
        increaseBtn.onClick.RemoveAllListeners();
        minBtn.onClick.RemoveAllListeners();
        halfBtn.onClick.RemoveAllListeners();
        maxBtn.onClick.RemoveAllListeners();
        startBtn.onClick.RemoveAllListeners();

        decreaseBtn.onClick.AddListener(() => OnBetButton(false));
        increaseBtn.onClick.AddListener(() => OnBetButton(true));
        minBtn.onClick.AddListener(() => OnPresetBtn(BetManager.Instance.SetBetMin, minBtn));
        halfBtn.onClick.AddListener(() => OnPresetBtn(BetManager.Instance.SetBetHalf, halfBtn));
        maxBtn.onClick.AddListener(() => OnPresetBtn(BetManager.Instance.SetBetMax, maxBtn));
        startBtn.onClick.AddListener(OnStartClicked);
    }

    public void ResetAndShow()
    {
        canInteract = false;
        rootGroup.alpha = 0f;
        UpdateBalanceDisplay(BalanceManager.Instance.Balance, false);
        UpdateBetDisplay(BetManager.Instance.CurrentBet, false);
        UpdateSlider(false);
        UpdateStartButton(false);
        UpdateStageDisplay(true);
        PlayEnterAnimation();
    }
    
    private void PlayEnterAnimation()
    {
        rootGroup.alpha = 0f;

        headerRect.anchoredPosition += new Vector2(0f, 30f);
        betDisplayRect.localScale = Vector3.one * 0.85f;
        presetRow.anchoredPosition += new Vector2(0f, -20f);
        startBtnRect.anchoredPosition += new Vector2(0f, -40f);

        var seq = DOTween.Sequence();
        seq.Append(rootGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
        seq.Join(headerRect.DOAnchorPosY(headerRect.anchoredPosition.y - 30f, 0.4f).SetEase(Ease.OutCubic));
        seq.Join(betDisplayRect.DOScale(1f, 0.45f).SetEase(Ease.OutBack).SetDelay(0.05f));
        seq.Join(presetRow.DOAnchorPosY(presetRow.anchoredPosition.y + 20f, 0.4f).SetEase(Ease.OutCubic).SetDelay(0.1f));
        seq.Join(startBtnRect.DOAnchorPosY(startBtnRect.anchoredPosition.y + 40f, 0.45f).SetEase(Ease.OutCubic).SetDelay(0.15f));
        seq.OnComplete(() =>
        {
            canInteract = true;
            PulseStartButton();
        });
    }

    private void PulseStartButton()
    {
        if (BalanceManager.Instance.Balance < BetManager.Instance.CurrentBet) return;
        startBtnRect.DOKill(false);
        startBtnRect.DOScale(1.03f, 0.85f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnBalanceChanged(float newBalance)
    {
        UpdateBalanceDisplay(newBalance, true);
        UpdateStartButton(true);
        UpdateSlider(true);
    }

    private void OnBetChanged(float newBet)
    {
        UpdateBetDisplay(newBet, true);
        UpdateStartButton(true);
        UpdateSlider(true);
    }

    private void UpdateBalanceDisplay(float value, bool animate)
    {
        if (animate)
        {
            DOTween.To(() => displayedBalance, x =>
            {
                displayedBalance = x;
                balanceValue.text = FormatCurrency(x);
            }, value, 0.4f).SetEase(Ease.OutCubic);

            balanceValue.rectTransform.DOKill(false);
            balanceValue.rectTransform.DOScale(1.12f, 0.1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => balanceValue.rectTransform.DOScale(1f, 0.15f).SetEase(Ease.InQuad));
        }
        else
        {
            displayedBalance = value;
            balanceValue.text = FormatCurrency(value);
        }
    }

    private void UpdateBetDisplay(float value, bool animate)
    {
        if (animate)
        {
            DOTween.To(() => displayedBet, x =>
            {
                displayedBet = x;
                betValue.text = FormatCurrency(x);
            }, value, 0.25f).SetEase(Ease.OutCubic);

            betDisplayRect.DOKill(false);
            betDisplayRect.DOPunchScale(Vector3.one * 0.06f, 0.25f, 5, 0.5f);
        }
        else
        {
            displayedBet = value;
            betValue.text = FormatCurrency(value);
        }
    }

    private void UpdateSlider(bool animate)
    {
        if (sliderFill == null || sliderTrack == null) return;

        float maxBet = Mathf.Min(BetManager.Instance.MaxBet, BalanceManager.Instance.Balance);
        float t = maxBet > BetManager.Instance.MinBet
            ? (BetManager.Instance.CurrentBet - BetManager.Instance.MinBet) / (maxBet - BetManager.Instance.MinBet)
            : 0f;

        float trackWidth = sliderTrack.rect.width;
        float targetWidth = Mathf.Max(8f, trackWidth * t);

        if (animate)
            sliderFill.DOSizeDelta(new Vector2(targetWidth, sliderFill.sizeDelta.y), 0.25f).SetEase(Ease.OutCubic);
        else
            sliderFill.sizeDelta = new Vector2(targetWidth, sliderFill.sizeDelta.y);
    }

    private void UpdateStartButton(bool animate)
    {
        bool canStart = BalanceManager.Instance.Balance >= BetManager.Instance.CurrentBet;

        startBtnRect.DOKill(false);

        if (animate)
        {
            startBtnGroup.DOFade(canStart ? 1f : 0.4f, 0.2f);
        }
        else
        {
            startBtnGroup.alpha = canStart ? 1f : 0.4f;
        }

        startBtn.interactable = canStart;

        if (canStart) PulseStartButton();
    }

    private void OnBetButton(bool increase)
    {
        if (!canInteract) return;

        if (increase)
            BetManager.Instance.IncreaseBet();
        else
            BetManager.Instance.DecreaseBet();

        var btnRect = increase ? increaseBtnRect : decreaseBtnRect;
        btnRect.DOKill(false);
        btnRect.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f);
    }

    private void OnPresetBtn(System.Action setter, Button btn)
    {
        if (!canInteract) return;
        setter();

        var rt = btn.GetComponent<RectTransform>();
        rt.DOKill(false);
        rt.DOPunchScale(Vector3.one * 0.12f, 0.2f, 5, 0.5f);
    }

    private void OnStartClicked()
    {
        if (!canInteract) return;
        if (BalanceManager.Instance.Balance < BetManager.Instance.CurrentBet) return;

        canInteract = false;
        BalanceManager.Instance.TrySpend(BetManager.Instance.CurrentBet);

        startBtnRect.DOKill(false);
        startBtnRect.DOScale(0.9f, 0.08f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
                startBtnRect.DOScale(1.1f, 0.15f).SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        rootGroup.DOFade(0f, 0.25f).OnComplete(() =>
                        {
                            gameObject.SetActive(false);
                            GameManager.Instance.SetState(GameManager.GameState.RhythmPhase);
                            RhythmPhaseController.Instance.ActivatePhaseFromBet();
                        });
                    }));
    }

    private void OnStageChanged(int stage)
    {
        UpdateStageDisplay(true);
    }

    private void UpdateStageDisplay(bool animate)
    {
        if (StageManager.Instance == null) return;
        int stage = StageManager.Instance.CurrentStage;
        int rings = StageManager.Instance.GetRingCountForStage(stage);

        if (stageLabel != null)
        {
            stageLabel.text = $"STAGE {stage}";
            if (animate)
            {
                stageLabel.rectTransform.DOKill();
                stageLabel.rectTransform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 5, 0.4f);
            }
        }
        if (stageRingsLabel != null)
            stageRingsLabel.text = $"{rings} RINGS";
    }

    private string FormatCurrency(float value)
    {
        return "$" + value.ToString("N0");
    }
}
