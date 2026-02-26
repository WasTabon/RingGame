using UnityEngine;

public class BalanceManager : MonoBehaviour
{
    public static BalanceManager Instance { get; private set; }

    private const string BalanceKey = "PlayerBalance";
    private const float DefaultBalance = 1000f;

    public float Balance { get; private set; }

    public System.Action<float> OnBalanceChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Balance = PlayerPrefs.GetFloat(BalanceKey, DefaultBalance);
    }

    public void AddBalance(float amount)
    {
        Balance += amount;
        Save();
        OnBalanceChanged?.Invoke(Balance);
    }

    public bool TrySpend(float amount)
    {
        if (Balance < amount) return false;
        Balance -= amount;
        Save();
        OnBalanceChanged?.Invoke(Balance);
        return true;
    }

    public void ResetBalance()
    {
        Balance = DefaultBalance;
        Save();
        OnBalanceChanged?.Invoke(Balance);
    }

    private void Save()
    {
        PlayerPrefs.SetFloat(BalanceKey, Balance);
        PlayerPrefs.Save();
    }
}
