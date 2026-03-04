using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class ResultScreenUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup rootGroup;

    [Header("Result Label")]
    [SerializeField] private TextMeshProUGUI resultLabel;
    [SerializeField] private RectTransform resultLabelRect;

    [Header("Payout")]
    [SerializeField] private TextMeshProUGUI payoutText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private RectTransform payoutContainer;

    [Header("Symbols Row")]
    [SerializeField] private RectTransform symbolsRow;
    [SerializeField] private Image[] symbolSlots;
    [SerializeField] private Image[] symbolBgs;
    [SerializeField] private TextMeshProUGUI[] symbolLabels;

    [Header("Balance")]
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI betText;

    [Header("Buttons")]
    [SerializeField] private Button playAgainBtn;
    [SerializeField] private Button menuBtn;
    [SerializeField] private RectTransform buttonsRow;

    [Header("Background")]
    [SerializeField] private Image flashOverlay;
    [SerializeField] private Image bgPanel;

    [Header("Config")]
    [SerializeField] private SymbolConfig symbolConfig;
    private Coroutine revealCoroutine;

    private static readonly Color WinColor  = new Color(1f, 0.82f, 0.15f);
    private static readonly Color LoseColor = new Color(0.6f, 0.62f, 0.8f);
    private static readonly Color WinBg     = new Color(0.08f, 0.06f, 0.02f, 0.95f);
    private static readonly Color LoseBg    = new Color(0.04f, 0.04f, 0.1f, 0.95f);

    private void Start()
    {
        if (symbolConfig == null)
            symbolConfig = Resources.Load<SymbolConfig>("SymbolConfig");

        if (playAgainBtn != null)
        {
            playAgainBtn.onClick.RemoveAllListeners();
            playAgainBtn.onClick.AddListener(() => ResultScreenController.Instance?.OnPlayAgain());
        }
        if (menuBtn != null)
        {
            menuBtn.onClick.RemoveAllListeners();
            menuBtn.onClick.AddListener(() => ResultScreenController.Instance?.OnMainMenu());
        }

        if (flashOverlay != null) flashOverlay.color = new Color(1f, 1f, 1f, 0f);
    }

    public void Show(List<SymbolConfig.SymbolType?> captured, PayoutCalculator.PayoutResult payout, float betAmount, int stage)
    {
        gameObject.SetActive(true);
        rootGroup.alpha = 0f;

        SetupSymbolSlots(captured, payout);
        SetupPayoutDisplay(payout, betAmount);
        SetupButtons();

        if (bgPanel != null)
            bgPanel.DOColor(payout.isWin ? WinBg : LoseBg, 0f);

        if (revealCoroutine != null) StopCoroutine(revealCoroutine);
        revealCoroutine = StartCoroutine(RevealSequence(payout));
    }

    private void SetupSymbolSlots(List<SymbolConfig.SymbolType?> captured, PayoutCalculator.PayoutResult payout)
    {
        if (symbolSlots == null) return;

        for (int i = 0; i < symbolSlots.Length; i++)
        {
            bool hasSymbol = i < captured.Count && captured[i].HasValue;
            var slot = symbolSlots[i];
            var bg = symbolBgs != null && i < symbolBgs.Length ? symbolBgs[i] : null;
            var label = symbolLabels != null && i < symbolLabels.Length ? symbolLabels[i] : null;

            if (!hasSymbol)
            {
                slot.color = new Color(1f, 1f, 1f, 0.15f);
                if (bg != null) bg.color = new Color(0.2f, 0.2f, 0.3f, 0.5f);
                if (label != null) label.text = "—";
                continue;
            }

            var sym = captured[i].Value;
            bool isWinning = payout.winningSymbols.Contains(sym);

            if (symbolConfig != null)
            {
                var icon = symbolConfig.GetIcon(sym);
                if (icon != null) slot.sprite = icon;
                slot.color = Color.white;
                if (bg != null) bg.color = symbolConfig.GetColor(sym);
            }

            if (label != null) label.text = sym.ToString().ToUpper();

            slot.transform.localScale = Vector3.zero;
            if (bg != null) bg.transform.localScale = Vector3.zero;
        }
    }

    private void SetupPayoutDisplay(PayoutCalculator.PayoutResult payout, float betAmount)
    {
        if (resultLabel != null)
        {
            resultLabel.text = payout.resultLabel;
            resultLabel.color = payout.isWin ? WinColor : LoseColor;
            resultLabel.alpha = 0f;
        }

        if (payoutText != null)
        {
            payoutText.text = payout.isWin ? $"+${payout.totalPayout:N0}" : "$0";
            payoutText.color = payout.isWin ? WinColor : LoseColor;
            payoutText.alpha = 0f;
        }

        if (multiplierText != null)
        {
            multiplierText.text = payout.isWin ? $"x{payout.multiplier:F1}" : "";
            multiplierText.alpha = 0f;
        }

        if (balanceText != null)
            balanceText.text = $"BALANCE  ${BalanceManager.Instance?.Balance:N0}";

        if (betText != null)
            betText.text = $"BET  ${betAmount:N0}";

        if (buttonsRow != null)
            buttonsRow.localScale = Vector3.zero;
    }

    private void SetupButtons()
    {
        if (playAgainBtn != null)
            playAgainBtn.interactable = BalanceManager.Instance != null
                && BalanceManager.Instance.Balance >= BetManager.Instance?.CurrentBet;
    }

    private IEnumerator RevealSequence(PayoutCalculator.PayoutResult payout)
    {
        rootGroup.DOFade(1f, 0.3f);
        yield return new WaitForSeconds(0.35f);

        if (resultLabelRect != null)
        {
            resultLabelRect.localScale = Vector3.one * 0.5f;
            resultLabel.DOFade(1f, 0.2f);
            resultLabelRect.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
        }
        yield return new WaitForSeconds(0.4f);

        if (symbolSlots != null)
        {
            for (int i = 0; i < symbolSlots.Length; i++)
            {
                if (symbolSlots[i].transform.localScale == Vector3.zero)
                    continue;

                var slot = symbolSlots[i];
                var bg = symbolBgs != null && i < symbolBgs.Length ? symbolBgs[i] : null;

                if (bg != null)
                    bg.transform.DOScale(1f, 0.22f).SetEase(Ease.OutBack);
                slot.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

                SFXManager.Instance?.PlaySymbolTick();
                VFXManager.Instance?.SpawnSymbolRevealTick(slot.transform.position);
                yield return new WaitForSeconds(0.12f);
            }
        }

        yield return new WaitForSeconds(0.3f);

        if (payout.isWin)
        {
            yield return StartCoroutine(HighlightWinners(payout));

            SFXManager.Instance?.PlayWin(payout.multiplier);

            VFXManager.Instance?.SpawnWinCelebration(payout.multiplier);

            if (payout.multiplier >= 5f)
            {
                var shakeTarget = rootGroup != null ? rootGroup.transform as RectTransform : null;
                VFXManager.Instance?.ShakeCanvas(shakeTarget, payout.multiplier >= 10f ? 2f : 1f);
            }

            if (flashOverlay != null)
            {
                flashOverlay.color = new Color(WinColor.r, WinColor.g, WinColor.b, 0.25f);
                flashOverlay.DOFade(0f, 0.5f);
            }

            if (payoutText != null)
            {
                payoutContainer?.DOPunchScale(Vector3.one * 0.12f, 0.3f, 5, 0.4f);
                payoutText.DOFade(1f, 0.2f);

                float displayVal = 0f;
                DOTween.To(() => displayVal, x =>
                {
                    displayVal = x;
                    payoutText.text = $"+${x:N0}";
                }, payout.totalPayout, 0.6f).SetEase(Ease.OutCubic);
            }

            if (multiplierText != null)
                multiplierText.DOFade(1f, 0.25f).SetDelay(0.1f);
        }
        else
        {
            if (payoutText != null) payoutText.DOFade(1f, 0.3f);
            SFXManager.Instance?.PlayLose();
        }

        yield return new WaitForSeconds(0.5f);

        if (balanceText != null && BalanceManager.Instance != null)
        {
            balanceText.text = $"BALANCE  ${BalanceManager.Instance.Balance:N0}";
            balanceText.rectTransform.DOPunchScale(Vector3.one * 0.08f, 0.25f, 4, 0.4f);
        }

        yield return new WaitForSeconds(0.3f);

        if (buttonsRow != null)
            buttonsRow.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
    }

    private IEnumerator HighlightWinners(PayoutCalculator.PayoutResult payout)
    {
        if (symbolSlots == null) yield break;

        for (int i = 0; i < symbolSlots.Length; i++)
        {
            var bg = symbolBgs != null && i < symbolBgs.Length ? symbolBgs[i] : null;
            if (bg == null) continue;

            bool isWin = payout.winningSymbols.Count > 0 && i < payout.winningSymbols.Count;
            if (!isWin)
            {
                bg.DOFade(0.25f, 0.2f);
                symbolSlots[i].DOFade(0.3f, 0.2f);
            }
        }

        yield return new WaitForSeconds(0.15f);

        for (int pulse = 0; pulse < 2; pulse++)
        {
            for (int i = 0; i < symbolSlots.Length; i++)
            {
                if (symbolBgs == null || i >= symbolBgs.Length) continue;
                bool isWin = payout.winningSymbols.Count > 0 && i < payout.winningSymbols.Count;
                if (!isWin) continue;

                symbolSlots[i].transform.DOPunchScale(Vector3.one * 0.18f, 0.2f, 5, 0.4f);
                symbolBgs[i].DOColor(WinColor, 0.1f)
                    .OnComplete(() => symbolBgs[i]?.DOColor(symbolConfig != null
                        ? symbolConfig.GetColor(payout.winningSymbols[0])
                        : Color.white, 0.2f));
            }
            yield return new WaitForSeconds(0.28f);
        }
    }

    public void Hide(System.Action onComplete = null)
    {
        if (revealCoroutine != null) StopCoroutine(revealCoroutine);
        rootGroup.DOFade(0f, 0.25f).OnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }
}
