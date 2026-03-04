using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private float fadeDuration = 0.35f;

    private RectTransform wipePanel;
    private Image wipeImage;
    private Canvas transitionCanvas;
    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        EnsureWipePanel();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void EnsureWipePanel()
    {
        if (wipePanel != null) return;

        transitionCanvas = fadeOverlay != null
            ? fadeOverlay.GetComponentInParent<Canvas>()
            : null;

        if (transitionCanvas == null)
        {
            var cGO = new GameObject("TransitionCanvas");
            cGO.transform.SetParent(transform, false);
            transitionCanvas = cGO.AddComponent<Canvas>();
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            transitionCanvas.sortingOrder = 999;
            cGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        }

        var wipeGO = new GameObject("WipePanel");
        wipeGO.transform.SetParent(transitionCanvas.transform, false);
        wipePanel = wipeGO.AddComponent<RectTransform>();
        wipePanel.anchorMin = Vector2.zero;
        wipePanel.anchorMax = Vector2.one;
        wipePanel.offsetMin = Vector2.zero;
        wipePanel.offsetMax = Vector2.zero;

        wipeImage = wipeGO.AddComponent<Image>();
        wipeImage.color = new Color(0.04f, 0.03f, 0.12f, 1f);
        wipeImage.raycastTarget = true;

        wipePanel.gameObject.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        WipeIn();
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        WipeOut(() =>
        {
            SceneManager.LoadScene(sceneName);
            isTransitioning = false;
        });
    }

    private void WipeOut(System.Action onComplete)
    {
        EnsureWipePanel();
        wipePanel.gameObject.SetActive(true);

        wipePanel.pivot = new Vector2(0.5f, 0f);
        wipePanel.anchorMin = Vector2.zero;
        wipePanel.anchorMax = new Vector2(1f, 0f);
        wipePanel.offsetMin = Vector2.zero;
        wipePanel.offsetMax = Vector2.zero;
        wipeImage.color = new Color(0.04f, 0.03f, 0.12f, 1f);

        wipePanel.DOKill();
        wipePanel.DOAnchorMax(Vector2.one, fadeDuration)
            .SetEase(Ease.InOutCubic)
            .SetUpdate(true)
            .OnComplete(() => onComplete?.Invoke());
    }

    private void WipeIn()
    {
        EnsureWipePanel();
        wipePanel.gameObject.SetActive(true);

        wipePanel.pivot = new Vector2(0.5f, 1f);
        wipePanel.anchorMin = Vector2.zero;
        wipePanel.anchorMax = Vector2.one;
        wipePanel.offsetMin = Vector2.zero;
        wipePanel.offsetMax = Vector2.zero;
        wipeImage.color = new Color(0.04f, 0.03f, 0.12f, 1f);

        wipePanel.DOKill();
        wipePanel.DOAnchorMin(new Vector2(0f, 1f), fadeDuration)
            .SetEase(Ease.InOutCubic)
            .SetUpdate(true)
            .OnComplete(() => wipePanel.gameObject.SetActive(false));
    }
}
