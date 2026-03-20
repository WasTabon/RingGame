using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Iteration14_Setup : EditorWindow
{
    private Transform canvasParent;
    private Transform buttonsParent;

    [MenuItem("RingGame/Iteration 14/Setup Coin Shop")]
    public static void Open()
    {
        GetWindow<Iteration14_Setup>("Iteration 14 \u2014 Coin Shop");
    }

    private void OnGUI()
    {
        GUILayout.Label("Coin Shop + IAP (Iteration 14)", EditorStyles.boldLabel);
        GUILayout.Space(4);
        GUILayout.Label("Creates Shop button + CoinShopUI panel.", EditorStyles.miniLabel);
        GUILayout.Label("Run on MainMenu scene.", EditorStyles.miniLabel);
        GUILayout.Space(8);

        canvasParent = (Transform)EditorGUILayout.ObjectField(
            "Canvas Parent (MainMenuCanvas)", canvasParent, typeof(Transform), true);

        buttonsParent = (Transform)EditorGUILayout.ObjectField(
            "Buttons Parent (ButtonsContainer)", buttonsParent, typeof(Transform), true);

        if (canvasParent == null)
            EditorGUILayout.HelpBox("Drag MainMenuCanvas from Hierarchy", MessageType.Warning);
        if (buttonsParent == null)
            EditorGUILayout.HelpBox("Drag ButtonsContainer from Hierarchy", MessageType.Warning);

        GUI.enabled = canvasParent != null && buttonsParent != null;

        GUILayout.Space(8);
        if (GUILayout.Button("Setup Coin Shop", GUILayout.Height(40)))
            DoSetup();

        GUI.enabled = true;

        GUILayout.Space(16);
        GUILayout.Label("Utilities", EditorStyles.boldLabel);
        if (GUILayout.Button("Validate", GUILayout.Height(30)))
            Validate();
    }

    private void DoSetup()
    {
        CreateShopButton();
        CreateCoinShopPanel();
        WireMainMenuUI();

        EditorSceneManager.MarkSceneDirty(canvasParent.gameObject.scene);

        Debug.Log("[Iter14] Setup complete. Save scene (Ctrl+S).");
        EditorUtility.DisplayDialog("Done",
            "Coin Shop created.\n\n" +
            "\u2022 SHOP button in " + buttonsParent.name + "\n" +
            "\u2022 CoinShopPanel in " + canvasParent.name + "\n" +
            "\u2022 MainMenuUI wired\n\n" +
            "Next steps:\n" +
            "1. Add IAP Button component to BuyButton\n" +
            "2. Set Product ID on CoinShopUI\n" +
            "3. Wire OnBuyClicked, OnPurchaseComplete,\n" +
            "   OnPurchaseFailed, OnProductFetched\n\n" +
            "Save scene (Ctrl+S).", "OK");
    }

    private void CreateShopButton()
    {
        var existing = FindChildByName(buttonsParent, "ShopButton");
        if (existing != null)
        {
            Debug.Log("[Iter14] ShopButton already exists.");
            return;
        }

        var shopGO = new GameObject("ShopButton");
        shopGO.transform.SetParent(buttonsParent, false);
        var shopRT = shopGO.AddComponent<RectTransform>();
        shopRT.anchorMin = new Vector2(0.5f, 0.5f);
        shopRT.anchorMax = new Vector2(0.5f, 0.5f);
        shopRT.pivot = new Vector2(0.5f, 0.5f);
        shopRT.anchoredPosition = new Vector2(0f, -55f);
        shopRT.sizeDelta = new Vector2(300f, 56f);

        var shopImg = shopGO.AddComponent<Image>();
        shopImg.color = new Color(0.3f, 0.55f, 1f);

        var shopBtn = shopGO.AddComponent<Button>();
        var colors = shopBtn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 0.9f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        shopBtn.colors = colors;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(shopGO.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        var textTMP = textGO.AddComponent<TextMeshProUGUI>();
        textTMP.text = "SHOP";
        textTMP.fontSize = 24;
        textTMP.fontStyle = FontStyles.Bold;
        textTMP.alignment = TextAlignmentOptions.Center;
        textTMP.color = Color.white;

        Undo.RegisterCreatedObjectUndo(shopGO, "Create ShopButton");
        Debug.Log("[Iter14] ShopButton created in " + buttonsParent.name);
    }

    private void CreateCoinShopPanel()
    {
        var existing = Object.FindObjectOfType<CoinShopUI>(true);
        if (existing != null)
        {
            Debug.Log("[Iter14] CoinShopUI already exists on: " + existing.gameObject.name);
            return;
        }

        var rootGO = new GameObject("CoinShopPanel");
        rootGO.transform.SetParent(canvasParent, false);
        rootGO.transform.SetAsLastSibling();
        var rootRT = rootGO.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;

        var dimGO = new GameObject("DimOverlay");
        dimGO.transform.SetParent(rootGO.transform, false);
        var dimRT = dimGO.AddComponent<RectTransform>();
        dimRT.anchorMin = Vector2.zero;
        dimRT.anchorMax = Vector2.one;
        dimRT.offsetMin = Vector2.zero;
        dimRT.offsetMax = Vector2.zero;
        var dimImg = dimGO.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.6f);
        dimImg.raycastTarget = true;
        var dimCG = dimGO.AddComponent<CanvasGroup>();
        dimGO.AddComponent<Button>();

        var cardGO = CreateChild(rootGO, "Card", new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.75f));
        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.06f, 0.05f, 0.15f, 0.95f);
        cardImg.raycastTarget = true;
        var cardCG = cardGO.AddComponent<CanvasGroup>();

        var titleGO = CreateChild(cardGO, "Title", new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.96f));
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "COIN SHOP";
        titleTMP.fontSize = 28f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.82f, 0.15f);
        titleTMP.raycastTarget = false;

        var balanceHeaderGO = CreateChild(cardGO, "BalanceHeader", new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.82f));
        var balanceHeaderTMP = balanceHeaderGO.AddComponent<TextMeshProUGUI>();
        balanceHeaderTMP.text = "YOUR BALANCE";
        balanceHeaderTMP.fontSize = 14f;
        balanceHeaderTMP.alignment = TextAlignmentOptions.Center;
        balanceHeaderTMP.color = new Color(0.6f, 0.6f, 0.8f);
        balanceHeaderTMP.raycastTarget = false;

        var balanceGO = CreateChild(cardGO, "BalanceText", new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.74f));
        var balanceTMP = balanceGO.AddComponent<TextMeshProUGUI>();
        balanceTMP.text = "$1,000";
        balanceTMP.fontSize = 32f;
        balanceTMP.fontStyle = FontStyles.Bold;
        balanceTMP.alignment = TextAlignmentOptions.Center;
        balanceTMP.color = Color.white;
        balanceTMP.raycastTarget = false;

        var packBgGO = CreateChild(cardGO, "PackBg", new Vector2(0.08f, 0.32f), new Vector2(0.92f, 0.58f));
        var packBgImg = packBgGO.AddComponent<Image>();
        packBgImg.color = new Color(0.1f, 0.09f, 0.22f, 0.9f);
        packBgImg.raycastTarget = false;

        var packTitleGO = CreateChild(packBgGO, "PackTitle", new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.95f));
        var packTitleTMP = packTitleGO.AddComponent<TextMeshProUGUI>();
        packTitleTMP.text = "1000 COINS";
        packTitleTMP.fontSize = 26f;
        packTitleTMP.fontStyle = FontStyles.Bold;
        packTitleTMP.alignment = TextAlignmentOptions.Center;
        packTitleTMP.color = new Color(1f, 0.82f, 0.15f);
        packTitleTMP.raycastTarget = false;

        var packDescGO = CreateChild(packBgGO, "PackDesc", new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.5f));
        var packDescTMP = packDescGO.AddComponent<TextMeshProUGUI>();
        packDescTMP.text = "Add 1000 coins to your balance";
        packDescTMP.fontSize = 14f;
        packDescTMP.alignment = TextAlignmentOptions.Center;
        packDescTMP.color = new Color(0.7f, 0.7f, 0.85f);
        packDescTMP.raycastTarget = false;

        var buyBtnGO = CreateChild(cardGO, "BuyButton", new Vector2(0.15f, 0.16f), new Vector2(0.85f, 0.28f));
        var buyBtnImg = buyBtnGO.AddComponent<Image>();
        buyBtnImg.color = new Color(0.25f, 0.85f, 0.45f);
        var buyBtn = buyBtnGO.AddComponent<Button>();

        var buyTextGO = CreateChild(buyBtnGO, "Text", new Vector2(0f, 0f), new Vector2(1f, 1f));
        var buyTMP = buyTextGO.AddComponent<TextMeshProUGUI>();
        buyTMP.text = "$0.99";
        buyTMP.fontSize = 24f;
        buyTMP.fontStyle = FontStyles.Bold;
        buyTMP.alignment = TextAlignmentOptions.Center;
        buyTMP.color = new Color(0.02f, 0.08f, 0.04f);
        buyTMP.raycastTarget = false;

        var loadingGO = CreateChild(cardGO, "LoadingButton", new Vector2(0.15f, 0.16f), new Vector2(0.85f, 0.28f));
        var loadingImg = loadingGO.AddComponent<Image>();
        loadingImg.color = new Color(0.3f, 0.3f, 0.45f);
        loadingImg.raycastTarget = true;

        var loadingTextGO = CreateChild(loadingGO, "Text", new Vector2(0f, 0f), new Vector2(1f, 1f));
        var loadingTMP = loadingTextGO.AddComponent<TextMeshProUGUI>();
        loadingTMP.text = "LOADING...";
        loadingTMP.fontSize = 20f;
        loadingTMP.fontStyle = FontStyles.Bold;
        loadingTMP.alignment = TextAlignmentOptions.Center;
        loadingTMP.color = new Color(0.7f, 0.7f, 0.8f);
        loadingTMP.raycastTarget = false;

        loadingGO.SetActive(false);

        var statusGO = CreateChild(cardGO, "StatusText", new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.15f));
        var statusTMP = statusGO.AddComponent<TextMeshProUGUI>();
        statusTMP.text = "";
        statusTMP.fontSize = 16f;
        statusTMP.alignment = TextAlignmentOptions.Center;
        statusTMP.color = new Color(0.25f, 0.82f, 0.50f);
        statusTMP.raycastTarget = false;

        var closeBtnGO = CreateChild(cardGO, "CloseButton", new Vector2(0.85f, 0.88f), new Vector2(0.97f, 0.97f));
        var closeImg = closeBtnGO.AddComponent<Image>();
        closeImg.color = new Color(0.4f, 0.35f, 0.55f);
        var closeBtn = closeBtnGO.AddComponent<Button>();
        var closeTextGO = CreateChild(closeBtnGO, "X", new Vector2(0f, 0f), new Vector2(1f, 1f));
        var closeTMP = closeTextGO.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "X";
        closeTMP.fontSize = 22f;
        closeTMP.fontStyle = FontStyles.Bold;
        closeTMP.alignment = TextAlignmentOptions.Center;
        closeTMP.color = Color.white;
        closeTMP.raycastTarget = false;

        var shopUI = rootGO.AddComponent<CoinShopUI>();
        var so = new SerializedObject(shopUI);

        so.FindProperty("panelGroup").objectReferenceValue = cardCG;
        so.FindProperty("panelRect").objectReferenceValue = cardGO.GetComponent<RectTransform>();
        so.FindProperty("dimOverlay").objectReferenceValue = dimCG;
        so.FindProperty("priceText").objectReferenceValue = buyTMP;
        so.FindProperty("statusText").objectReferenceValue = statusTMP;
        so.FindProperty("balanceText").objectReferenceValue = balanceTMP;
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("loadingButton").objectReferenceValue = loadingGO;

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(rootGO, "Create CoinShopPanel");
        Debug.Log("[Iter14] CoinShopPanel created inside " + canvasParent.name);
        Debug.Log("[Iter14] BuyButton created \u2014 add IAP Button component and wire methods manually.");
    }

    private void WireMainMenuUI()
    {
        var menuUI = Object.FindObjectOfType<MainMenuUI>(true);
        if (menuUI != null)
        {
            var so = new SerializedObject(menuUI);

            var shopUI = Object.FindObjectOfType<CoinShopUI>(true);
            if (shopUI != null)
            {
                so.FindProperty("coinShopUI").objectReferenceValue = shopUI;
                Debug.Log("[Iter14] MainMenuUI.coinShopUI wired.");
            }
            else
            {
                Debug.LogError("[Iter14] CoinShopUI not found!");
            }

            var shopBtn = FindChildByName(buttonsParent, "ShopButton");
            if (shopBtn != null)
            {
                var btn = shopBtn.GetComponent<Button>();
                if (btn != null)
                {
                    so.FindProperty("shopButton").objectReferenceValue = btn;
                    Debug.Log("[Iter14] MainMenuUI.shopButton wired.");
                }
                else
                {
                    Debug.LogError("[Iter14] ShopButton has no Button component!");
                }
            }
            else
            {
                Debug.LogError("[Iter14] ShopButton not found in " + buttonsParent.name + "!");
            }

            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogError("[Iter14] MainMenuUI not found in scene!");
        }
    }

    [MenuItem("RingGame/Iteration 14/Validate")]
    public static void Validate()
    {
        Debug.Log("[Iter14 Validate] \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550");
        int issues = 0;

        var shopUI = Object.FindObjectOfType<CoinShopUI>(true);
        if (shopUI != null)
        {
            Debug.Log("[Iter14] \u2713 CoinShopUI found on: " + shopUI.gameObject.name);
            var so = new SerializedObject(shopUI);
            CheckProp(so, "panelGroup", ref issues);
            CheckProp(so, "panelRect", ref issues);
            CheckProp(so, "dimOverlay", ref issues);
            CheckProp(so, "priceText", ref issues);
            CheckProp(so, "statusText", ref issues);
            CheckProp(so, "balanceText", ref issues);
            CheckProp(so, "closeButton", ref issues);
            CheckProp(so, "loadingButton", ref issues);
        }
        else
        {
            Debug.LogError("[Iter14] CoinShopUI NOT found! Run Setup.");
            issues++;
        }

        var menuUI = Object.FindObjectOfType<MainMenuUI>(true);
        if (menuUI != null)
        {
            var so = new SerializedObject(menuUI);

            var coinProp = so.FindProperty("coinShopUI");
            if (coinProp != null && coinProp.objectReferenceValue != null)
                Debug.Log("[Iter14] \u2713 MainMenuUI.coinShopUI wired");
            else
            {
                Debug.LogError("[Iter14] MainMenuUI.coinShopUI is null!");
                issues++;
            }

            var shopProp = so.FindProperty("shopButton");
            if (shopProp != null && shopProp.objectReferenceValue != null)
                Debug.Log("[Iter14] \u2713 MainMenuUI.shopButton wired");
            else
            {
                Debug.LogError("[Iter14] MainMenuUI.shopButton is null!");
                issues++;
            }
        }
        else
        {
            Debug.LogError("[Iter14] MainMenuUI NOT found!");
            issues++;
        }

        var balance = Object.FindObjectOfType<BalanceManager>(true);
        if (balance != null)
            Debug.Log("[Iter14] \u2713 BalanceManager found");
        else
        {
            Debug.LogWarning("[Iter14] BalanceManager not found in this scene (lives on Game scene, OK if on MainMenu)");
        }

        if (issues == 0)
            Debug.Log("[Iter14 Validate] \u2705 All good!");
        else
            Debug.LogError("[Iter14 Validate] " + issues + " issue(s) found.");

        Debug.Log("[Iter14 Validate] \u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550");
    }

    private static void CheckProp(SerializedObject so, string name, ref int issues)
    {
        var prop = so.FindProperty(name);
        if (prop != null && prop.objectReferenceValue != null)
            Debug.Log("[Iter14]   \u2713 " + name);
        else
        {
            Debug.LogError("[Iter14]   " + name + " is null!");
            issues++;
        }
    }

    private static GameObject CreateChild(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    private static Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindChildByName(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
