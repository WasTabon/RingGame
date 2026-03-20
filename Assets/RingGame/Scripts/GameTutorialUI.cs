using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameTutorialUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup dimOverlay;

    [Header("Suit Mapping")]
    [SerializeField] private Image[] suitIcons;
    [SerializeField] private TextMeshProUGUI[] directionLabels;

    [Header("Rules")]
    [SerializeField] private TextMeshProUGUI rulesText;

    [Header("Button")]
    [SerializeField] private Button gotItButton;
    [SerializeField] private RectTransform gotItButtonRect;

    [Header("Config")]
    [SerializeField] private SymbolConfig symbolConfig;

    private System.Action onCloseCallback;
    private bool isOpen;

    public void Show(System.Action onClose)
    {
        if (isOpen) return;
        isOpen = true;
        onCloseCallback = onClose;
        gameObject.SetActive(true);

        PopulateMapping();
        PopulateRules();

        if (gotItButton != null)
        {
            gotItButton.onClick.RemoveAllListeners();
            gotItButton.onClick.AddListener(OnGotItClicked);
        }
        else
        {
            Debug.LogError("GameTutorialUI: gotItButton is null!");
        }

        dimOverlay.alpha = 0f;
        dimOverlay.blocksRaycasts = true;
        dimOverlay.interactable = true;
        dimOverlay.DOFade(0.8f, 0.25f);

        panelGroup.alpha = 0f;
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;
        panelRect.localScale = Vector3.one * 0.85f;

        var seq = DOTween.Sequence();
        seq.Append(panelGroup.DOFade(1f, 0.25f).SetEase(Ease.OutQuad));
        seq.Join(panelRect.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
    }

    public void Hide(System.Action onComplete = null)
    {
        if (!isOpen) return;
        isOpen = false;

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
        if (SwipeDirectionMap.Instance == null)
        {
            Debug.LogError("GameTutorialUI: SwipeDirectionMap.Instance is null!");
            return;
        }
        if (symbolConfig == null)
        {
            Debug.LogError("GameTutorialUI: symbolConfig is null!");
            return;
        }

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
            if (i >= suitIcons.Length)
            {
                Debug.LogError("GameTutorialUI: suitIcons array too small for index " + i);
                break;
            }
            if (i >= directionLabels.Length)
            {
                Debug.LogError("GameTutorialUI: directionLabels array too small for index " + i);
                break;
            }

            var suit = suits[i];
            var icon = symbolConfig.GetIcon(suit);
            var color = symbolConfig.GetColor(suit);

            if (icon != null)
                suitIcons[i].sprite = icon;
            suitIcons[i].color = color;

            if (mapping.TryGetValue(suit, out var dir))
            {
                directionLabels[i].text = SwipeDirectionMap.GetArrowForDirection(dir) + " " + SwipeDirectionMap.GetNameForDirection(dir);
                directionLabels[i].color = color;
            }
            else
            {
                Debug.LogError("GameTutorialUI: No mapping for " + suit);
            }
        }
    }

    private void PopulateRules()
    {
        if (rulesText == null)
        {
            Debug.LogError("GameTutorialUI: rulesText is null!");
            return;
        }

        rulesText.text =
            "Swipe in the correct direction when the ring glows yellow.\n\n" +
            "Wrong direction = nothing happens.\n" +
            "Too slow = you lose an attempt.\n\n" +
            "WILD symbols accept any swipe direction.\n\n" +
            "Match symbols for bigger payouts!";
    }

    private void OnGotItClicked()
    {
        SFXManager.Instance?.PlayButtonClick();

        if (gotItButtonRect != null)
        {
            gotItButtonRect.DOKill();
            gotItButtonRect.DOScale(0.9f, 0.06f).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                    gotItButtonRect.DOScale(1f, 0.1f).SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            var cb = onCloseCallback;
                            Hide(() => cb?.Invoke());
                        }));
        }
        else
        {
            var cb = onCloseCallback;
            Hide(() => cb?.Invoke());
        }
    }
}
