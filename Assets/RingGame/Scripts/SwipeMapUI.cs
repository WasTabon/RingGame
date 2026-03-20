using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SwipeMapUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup dimOverlay;

    [Header("Suit Rows")]
    [SerializeField] private Image[] suitIcons;
    [SerializeField] private TextMeshProUGUI[] directionLabels;

    [Header("Button")]
    [SerializeField] private Button goButton;
    [SerializeField] private RectTransform goButtonRect;

    [Header("Config")]
    [SerializeField] private SymbolConfig symbolConfig;

    private System.Action onGoCallback;

    private void OnEnable()
    {
        if (goButton != null)
        {
            goButton.onClick.RemoveAllListeners();
            goButton.onClick.AddListener(OnGoClicked);
        }
        else
        {
            Debug.LogError("SwipeMapUI: goButton is null!");
        }
    }

    public void Show(System.Action onGo)
    {
        onGoCallback = onGo;
        gameObject.SetActive(true);

        PopulateMapping();

        dimOverlay.alpha = 0f;
        dimOverlay.blocksRaycasts = true;
        dimOverlay.interactable = true;
        dimOverlay.DOFade(0.75f, 0.25f);

        panelGroup.alpha = 0f;
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;
        panelRect.localScale = Vector3.one * 0.85f;

        var seq = DOTween.Sequence();
        seq.Append(panelGroup.DOFade(1f, 0.25f).SetEase(Ease.OutQuad));
        seq.Join(panelRect.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        seq.OnComplete(() =>
        {
            if (goButtonRect != null)
            {
                goButtonRect.DOKill();
                goButtonRect.DOScale(1.05f, 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            }
        });
    }

    public void Hide(System.Action onComplete = null)
    {
        if (goButtonRect != null)
            goButtonRect.DOKill();

        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        var seq = DOTween.Sequence();
        seq.Append(panelGroup.DOFade(0f, 0.2f));
        seq.Join(panelRect.DOScale(0.85f, 0.2f).SetEase(Ease.InBack));
        seq.Join(dimOverlay.DOFade(0f, 0.2f));
        seq.OnComplete(() =>
        {
            dimOverlay.blocksRaycasts = false;
            dimOverlay.interactable = false;
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    private void PopulateMapping()
    {
        Debug.Assert(SwipeDirectionMap.Instance != null, "SwipeMapUI: SwipeDirectionMap.Instance is null!");
        Debug.Assert(symbolConfig != null, "SwipeMapUI: symbolConfig is null!");

        var mapping = SwipeDirectionMap.Instance.GetMapping();

        var suits = new[]
        {
            SymbolConfig.SymbolType.Spades,
            SymbolConfig.SymbolType.Hearts,
            SymbolConfig.SymbolType.Diamonds,
            SymbolConfig.SymbolType.Clubs
        };

        for (int i = 0; i < suits.Length; i++)
        {
            Debug.Assert(i < suitIcons.Length, "SwipeMapUI: suitIcons array too small for index " + i);
            Debug.Assert(i < directionLabels.Length, "SwipeMapUI: directionLabels array too small for index " + i);

            var suit = suits[i];
            var icon = symbolConfig.GetIcon(suit);
            var color = symbolConfig.GetColor(suit);

            if (icon != null)
                suitIcons[i].sprite = icon;
            suitIcons[i].color = color;

            if (mapping.TryGetValue(suit, out var dir))
            {
                directionLabels[i].text = SwipeDirectionMap.GetArrowForDirection(dir);
                directionLabels[i].color = color;
            }
            else
            {
                Debug.LogError("SwipeMapUI: No mapping found for " + suit);
            }

            suitIcons[i].transform.localScale = Vector3.zero;
            directionLabels[i].transform.localScale = Vector3.zero;

            float delay = 0.1f + i * 0.08f;
            suitIcons[i].transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(delay);
            directionLabels[i].transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(delay + 0.05f);
        }
    }

    private void OnGoClicked()
    {
        SFXManager.Instance?.PlayButtonClick();

        if (goButtonRect != null)
        {
            goButtonRect.DOKill();
            goButtonRect.DOScale(0.9f, 0.06f).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                    goButtonRect.DOScale(1.05f, 0.1f).SetEase(Ease.OutBack)
                        .OnComplete(() => Hide(() => onGoCallback?.Invoke())));
        }
        else
        {
            Hide(() => onGoCallback?.Invoke());
        }
    }
}
