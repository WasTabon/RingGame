using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SettingsUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform panelRect;

    [Header("Music")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicValueText;

    [Header("SFX")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxValueText;

    [Header("Vibration")]
    [SerializeField] private Button vibrationToggle;
    [SerializeField] private TextMeshProUGUI vibrationText;
    [SerializeField] private Image vibrationBg;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button resetTutorialButton;

    private static readonly Color ToggleOnColor = new Color(0.3f, 0.85f, 0.5f);
    private static readonly Color ToggleOffColor = new Color(0.5f, 0.5f, 0.6f);

    private void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }
        if (vibrationToggle != null)
        {
            vibrationToggle.onClick.RemoveAllListeners();
            vibrationToggle.onClick.AddListener(OnVibrationToggle);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }
        if (resetTutorialButton != null)
        {
            resetTutorialButton.onClick.RemoveAllListeners();
            resetTutorialButton.onClick.AddListener(OnResetTutorial);
        }

        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (SettingsManager.Instance != null)
        {
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(SettingsManager.Instance.MusicVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(SettingsManager.Instance.SFXVolume);
            UpdateMusicText(SettingsManager.Instance.MusicVolume);
            UpdateSFXText(SettingsManager.Instance.SFXVolume);
            UpdateVibrationVisual(SettingsManager.Instance.VibrationEnabled);
        }

        panelGroup.alpha = 0f;
        panelRect.localScale = Vector3.one * 0.85f;

        var seq = DOTween.Sequence();
        seq.Append(panelGroup.DOFade(1f, 0.25f).SetEase(Ease.OutQuad));
        seq.Join(panelRect.DOScale(1f, 0.3f).SetEase(Ease.OutBack));

        SFXManager.Instance?.PlayButtonClick();
    }

    public void Hide()
    {
        SFXManager.Instance?.PlayButtonClick();

        var seq = DOTween.Sequence();
        seq.Append(panelGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad));
        seq.Join(panelRect.DOScale(0.85f, 0.2f).SetEase(Ease.InQuad));
        seq.OnComplete(() => gameObject.SetActive(false));
    }

    private void OnMusicChanged(float val)
    {
        SettingsManager.Instance?.SetMusicVolume(val);
        UpdateMusicText(val);
    }

    private void OnSFXChanged(float val)
    {
        SettingsManager.Instance?.SetSFXVolume(val);
        UpdateSFXText(val);
    }

    private void OnVibrationToggle()
    {
        if (SettingsManager.Instance == null) return;
        bool newState = !SettingsManager.Instance.VibrationEnabled;
        SettingsManager.Instance.SetVibration(newState);
        UpdateVibrationVisual(newState);
        SFXManager.Instance?.PlayButtonClick();
        if (newState) SettingsManager.Instance.Vibrate();
    }

    private void OnResetTutorial()
    {
        SettingsManager.Instance?.ResetTutorial();
        SFXManager.Instance?.PlayButtonClick();
        if (resetTutorialButton != null)
        {
            var rt = resetTutorialButton.GetComponent<RectTransform>();
            rt.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.4f);
        }
    }

    private void UpdateMusicText(float val)
    {
        if (musicValueText != null) musicValueText.text = Mathf.RoundToInt(val * 100) + "%";
    }

    private void UpdateSFXText(float val)
    {
        if (sfxValueText != null) sfxValueText.text = Mathf.RoundToInt(val * 100) + "%";
    }

    private void UpdateVibrationVisual(bool on)
    {
        if (vibrationText != null) vibrationText.text = on ? "ON" : "OFF";
        if (vibrationBg != null) vibrationBg.DOColor(on ? ToggleOnColor : ToggleOffColor, 0.15f);
    }
}
