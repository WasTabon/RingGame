using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private float fadeDuration = 0.35f;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FadeIn();
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        FadeOut(() =>
        {
            SceneManager.LoadScene(sceneName);
            isTransitioning = false;
        });
    }

    private void FadeIn()
    {
        if (fadeOverlay == null) return;
        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.alpha = 1f;
        fadeOverlay.DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => fadeOverlay.gameObject.SetActive(false));
    }

    private void FadeOut(System.Action onComplete)
    {
        if (fadeOverlay == null)
        {
            onComplete?.Invoke();
            return;
        }

        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.alpha = 0f;
        fadeOverlay.DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .SetEase(Ease.InQuad)
            .OnComplete(() => onComplete?.Invoke());
    }
}
