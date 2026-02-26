using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        BetScreen,
        RhythmPhase,
        ResultScreen
    }

    public GameState CurrentState { get; private set; }

    public System.Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentState = GameState.MainMenu;
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void GoToGame()
    {
        SetState(GameState.BetScreen);
        SceneTransitionManager.Instance.LoadScene("Game");
    }

    public void GoToMainMenu()
    {
        SetState(GameState.MainMenu);
        SceneTransitionManager.Instance.LoadScene("MainMenu");
    }
}
