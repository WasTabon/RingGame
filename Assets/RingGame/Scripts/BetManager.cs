using UnityEngine;

public class BetManager : MonoBehaviour
{
    public static BetManager Instance { get; private set; }

    [Header("Bet Config")]
    [SerializeField] private float minBet = 10f;
    [SerializeField] private float maxBet = 500f;
    [SerializeField] private float betStep = 10f;

    public float CurrentBet { get; private set; }
    public float MinBet => minBet;
    public float MaxBet => maxBet;
    public float BetStep => betStep;

    public System.Action<float> OnBetChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentBet = minBet;
    }

    public void IncreaseBet()
    {
        SetBet(CurrentBet + betStep);
    }

    public void DecreaseBet()
    {
        SetBet(CurrentBet - betStep);
    }

    public void SetBetMin() => SetBet(minBet);
    public void SetBetMax() => SetBet(maxBet);
    public void SetBetHalf() => SetBet(Mathf.Round(BalanceManager.Instance.Balance * 0.5f / betStep) * betStep);

    public void SetBet(float value)
    {
        CurrentBet = Mathf.Clamp(
            Mathf.Round(value / betStep) * betStep,
            minBet,
            Mathf.Min(maxBet, BalanceManager.Instance.Balance)
        );
        OnBetChanged?.Invoke(CurrentBet);
    }
}
