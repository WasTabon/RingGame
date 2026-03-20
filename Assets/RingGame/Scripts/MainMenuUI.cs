using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MainMenuUI : MonoBehaviour
{
    [Header("Logo")]
    [SerializeField] private RectTransform logoContainer;
    [SerializeField] private CanvasGroup logoGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("Buttons")]
    [SerializeField] private CanvasGroup buttonsGroup;
    [SerializeField] private Button playButton;
    [SerializeField] private RectTransform playButtonRect;
    [SerializeField] private Button settingsButton;

    [Header("Panels")]
    [SerializeField] private SettingsUI settingsUI;
    [SerializeField] private TutorialUI tutorialUI;
    [SerializeField] private CoinShopUI coinShopUI;

    [Header("Shop")]
    [SerializeField] private Button shopButton;

    [Header("Background Rings")]
    [SerializeField] private RectTransform[] bgRingVisuals;
    [SerializeField] private CanvasGroup bgGroup;

    [Header("Beat Pulse Settings")]
    [SerializeField] private float beatPulseScale = 1.05f;
    [SerializeField] private float beatPulseDuration = 0.07f;

    private bool canInteract;

    private void Start()
    {
        canInteract = false;
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (shopButton != null)
        {
            shopButton.onClick.RemoveAllListeners();
            shopButton.onClick.AddListener(OnShopClicked);
        }
        else
        {
            Debug.LogWarning("MainMenuUI: shopButton is null!");
        }

        PlayIntroAnimation();
        StartBgAnimation();
    }

    private void OnEnable()
    {
        SubscribeToAudio();
    }

    private void OnDisable()
    {
        UnsubscribeFromAudio();
    }

    private void OnDestroy()
    {
        UnsubscribeFromAudio();
    }

    private void SubscribeToAudio()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnBeat -= HandleBeat;
            AudioManager.Instance.OnBeat += HandleBeat;
        }
    }

    private void UnsubscribeFromAudio()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.OnBeat -= HandleBeat;
    }

    private void PlayIntroAnimation()
    {
        if (bgGroup != null)
        {
            bgGroup.alpha = 0f;
            bgGroup.DOFade(0.5f, 1.4f).SetEase(Ease.OutQuad);
        }

        if (logoGroup != null)
        {
            logoGroup.alpha = 0f;
            logoContainer.localScale = Vector3.one * 0.8f;
            logoGroup.DOFade(1f, 0.6f).SetDelay(0.4f).SetEase(Ease.OutQuad);
            logoContainer.DOScale(1f, 0.7f).SetDelay(0.4f).SetEase(Ease.OutBack);
        }

        if (buttonsGroup != null)
        {
            buttonsGroup.alpha = 0f;
            buttonsGroup.DOFade(1f, 0.5f).SetDelay(1f).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    canInteract = true;
                    StartPlayButtonPulse();
                    CheckFirstLaunch();
                });
        }
    }

    private void CheckFirstLaunch()
    {
        if (SettingsManager.Instance != null && !SettingsManager.Instance.TutorialDone)
        {
            if (tutorialUI != null)
                tutorialUI.Show(null);
        }
    }

    private void StartPlayButtonPulse()
    {
        if (playButtonRect == null) return;
        playButtonRect.DOScale(1.04f, 0.9f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StartBgAnimation()
    {
        if (bgRingVisuals == null) return;

        for (int i = 0; i < bgRingVisuals.Length; i++)
        {
            if (bgRingVisuals[i] == null) continue;
            float dir = i % 2 == 0 ? 1f : -1f;
            float duration = 10f + i * 4f;
            bgRingVisuals[i]
                .DORotate(new Vector3(0, 0, 360f * dir), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }
    }

    private void HandleBeat()
    {
        if (logoContainer == null) return;
        logoContainer.DOKill(false);
        logoContainer.DOScale(beatPulseScale, beatPulseDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
                logoContainer.DOScale(1f, beatPulseDuration * 2f).SetEase(Ease.InQuad));
    }

    private void OnPlayClicked()
    {
        if (!canInteract) return;
        canInteract = false;

        SFXManager.Instance?.PlayButtonClick();

        playButtonRect.DOKill(false);
        playButtonRect.DOScale(0.9f, 0.08f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
                playButtonRect.DOScale(1.06f, 0.13f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => GameManager.Instance.GoToGame()));
    }

    private void OnSettingsClicked()
    {
        if (!canInteract) return;
        settingsUI?.Show();
    }

    private void OnShopClicked()
    {
        if (!canInteract) return;
        Debug.Assert(coinShopUI != null, "MainMenuUI: coinShopUI is null!");
        coinShopUI.Show();
    }
}
