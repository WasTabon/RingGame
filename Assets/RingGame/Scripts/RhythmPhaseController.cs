using UnityEngine;

public class RhythmPhaseController : MonoBehaviour
{
    public static RhythmPhaseController Instance { get; private set; }

    [SerializeField] private RhythmPhaseUI rhythmPhaseUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ActivatePhaseFromBet()
    {
        rhythmPhaseUI?.Show();
        RingsManager.Instance?.SetupForStage(1);
        RingsManager.Instance?.PlayEntrance();
    }

    public void OnBackToBet()
    {
        GameManager.Instance.SetState(GameManager.GameState.BetScreen);
        rhythmPhaseUI?.Hide();

        var betUI = Object.FindObjectOfType<BetScreenUI>(true);
        if (betUI != null)
        {
            betUI.gameObject.SetActive(true);
            betUI.ResetAndShow();
        }
    }
}