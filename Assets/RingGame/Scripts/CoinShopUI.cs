using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using DG.Tweening;
using TMPro;

public class CoinShopUI : MonoBehaviour
{
    public string productId = "com.ringgame.coins1000";

    public GameObject loadingButton;

    [Header("Shop Panel")]
    public CanvasGroup panelGroup;
    public RectTransform panelRect;
    public CanvasGroup dimOverlay;

    [Header("UI")]
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI balanceText;
    public Button closeButton;

    private bool isOpen;

    private void Awake()
    {
        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        dimOverlay.alpha = 0f;
        dimOverlay.blocksRaycasts = false;
        dimOverlay.interactable = false;
    }

    private void OnEnable()
    {
        closeButton.onClick.RemoveListener(Hide);
        closeButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        if (isOpen) return;
        isOpen = true;

        statusText.text = "";
        loadingButton.SetActive(false);
        RefreshBalance();

        SFXManager.Instance?.PlayButtonClick();

        dimOverlay.alpha = 0f;
        dimOverlay.blocksRaycasts = true;
        dimOverlay.interactable = true;
        dimOverlay.DOFade(0.6f, 0.3f);

        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;
        panelRect.localScale = Vector3.one * 0.8f;
        panelGroup.DOFade(1f, 0.25f);
        panelRect.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }

    public void Hide()
    {
        if (!isOpen) return;
        isOpen = false;

        SFXManager.Instance?.PlayButtonClick();

        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        panelGroup.DOFade(0f, 0.2f);
        panelRect.DOScale(0.8f, 0.2f).SetEase(Ease.InBack);

        dimOverlay.DOFade(0f, 0.2f).OnComplete(() =>
        {
            dimOverlay.blocksRaycasts = false;
            dimOverlay.interactable = false;
        });
    }

    public void OnBuyClicked()
    {
        loadingButton.SetActive(true);
    }

    public void OnPurchaseComplete(Product product)
    {
        if (product.definition.id == productId)
        {
            Debug.Log("[IAP] Purchase complete: " + productId);

            BalanceManager.Instance.AddBalance(1000f);

            loadingButton.SetActive(false);
            RefreshBalance();

            statusText.color = new Color(0.25f, 0.82f, 0.50f, 1f);
            statusText.text = "+1000 COINS!";

            SFXManager.Instance?.PlayButtonClick();

            panelRect.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5);
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
    {
        if (product.definition.id == productId)
        {
            Debug.Log("[IAP] Failed: " + description.message);

            loadingButton.SetActive(false);

            statusText.color = new Color(0.90f, 0.22f, 0.35f, 1f);
            statusText.text = "Purchase failed";
        }
    }

    public void OnProductFetched(Product product)
    {
        Debug.Log("[IAP] Fetched: " + product.metadata.localizedPriceString);
        priceText.text = product.metadata.localizedPriceString;
    }

    private void RefreshBalance()
    {
        balanceText.text = "$" + BalanceManager.Instance.Balance.ToString("N0");
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(Hide);

        Button dimBtn = dimOverlay.GetComponent<Button>();
        if (dimBtn != null)
            dimBtn.onClick.RemoveListener(Hide);
    }
}
