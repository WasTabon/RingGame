using UnityEngine;
using System.Collections.Generic;

public class ResultScreenController : MonoBehaviour
{
    public static ResultScreenController Instance { get; private set; }

    [SerializeField] private ResultScreenUI resultScreenUI;

    private PayoutCalculator.PayoutResult lastPayout;
    private int lastStage;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ShowResult(List<SymbolConfig.SymbolType?> capturedSymbols, float betAmount, int stage)
    {
        lastStage = stage;
        lastPayout = PayoutCalculator.Calculate(capturedSymbols, betAmount);

        if (lastPayout.isWin && lastPayout.totalPayout > 0f)
        {
            BalanceManager.Instance?.AddBalance(lastPayout.totalPayout);
            StageManager.Instance?.AdvanceStage();
        }

        resultScreenUI?.Show(capturedSymbols, lastPayout, betAmount, stage);
    }

    public void OnPlayAgain()
    {
        resultScreenUI?.Hide(() =>
        {
            GameManager.Instance.SetState(GameManager.GameState.BetScreen);

            var betUI = FindObjectOfType<BetScreenUI>(true);
            if (betUI != null)
            {
                betUI.gameObject.SetActive(true);
                betUI.ResetAndShow();
            }
        });
    }

    public void OnMainMenu()
    {
        resultScreenUI?.Hide(() => GameManager.Instance.GoToMainMenu());
    }
}
