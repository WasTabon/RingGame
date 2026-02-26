using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class RingsManager : MonoBehaviour
{
    public static RingsManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform ringsContainer;
    [SerializeField] private SymbolConfig symbolConfig;
    [SerializeField] private Sprite ringSprite;
    [SerializeField] private Sprite symbolBgSprite;

    private readonly float[] ringDiameters = { 165f, 248f, 328f, 405f, 478f };
    private readonly float[] baseRotationSpeeds = { 30f, -42f, 55f, -65f, 76f };

    private readonly Color[] ringColors = {
        new Color(0.48f, 0.58f, 1f, 0.9f),
        new Color(1f, 0.42f, 0.58f, 0.9f),
        new Color(0.35f, 0.92f, 0.62f, 0.9f),
        new Color(1f, 0.78f, 0.28f, 0.9f),
        new Color(0.72f, 0.42f, 1f, 0.9f)
    };

    private List<RingController> activeRings = new List<RingController>();

    public List<RingController> ActiveRings => activeRings;
    public int RingCount => activeRings.Count;

    public System.Action OnRingsSpawned;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetupForStage(int stage)
    {
        stage = Mathf.Clamp(stage, 1, 4);
        ClearRings();
        int ringCount = stage + 1;
        SpawnRings(ringCount, stage);
        OnRingsSpawned?.Invoke();
    }

    private void ClearRings()
    {
        foreach (var ring in activeRings)
        {
            if (ring != null)
                Destroy(ring.gameObject);
        }
        activeRings.Clear();
    }

    private void SpawnRings(int count, int stage)
    {
        var symbols = GetShuffledSymbols(count);
        float stageSpeedMult = 1f + (stage - 1) * 0.15f;

        for (int i = 0; i < count; i++)
        {
            var controller = BuildRing(i, symbols[i], stageSpeedMult);
            activeRings.Add(controller);
        }
    }

    private RingController BuildRing(int index, SymbolConfig.SymbolType symbol, float speedMult)
    {
        var go = new GameObject($"Ring_{index}");
        go.transform.SetParent(ringsContainer, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(ringDiameters[index], ringDiameters[index]);

        var glowGO = new GameObject("Glow");
        glowGO.transform.SetParent(go.transform, false);
        var glowRT = glowGO.AddComponent<RectTransform>();
        glowRT.anchorMin = new Vector2(0.5f, 0.5f);
        glowRT.anchorMax = new Vector2(0.5f, 0.5f);
        glowRT.pivot = new Vector2(0.5f, 0.5f);
        glowRT.anchoredPosition = Vector2.zero;
        glowRT.sizeDelta = new Vector2(ringDiameters[index] + 32f, ringDiameters[index] + 32f);
        var glowImg = glowGO.AddComponent<Image>();
        glowImg.sprite = ringSprite;
        glowImg.color = new Color(1f, 1f, 1f, 0f);
        glowImg.raycastTarget = false;

        var bodyGO = new GameObject("Body");
        bodyGO.transform.SetParent(go.transform, false);
        var bodyRT = bodyGO.AddComponent<RectTransform>();
        bodyRT.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRT.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRT.pivot = new Vector2(0.5f, 0.5f);
        bodyRT.anchoredPosition = Vector2.zero;
        bodyRT.sizeDelta = new Vector2(ringDiameters[index], ringDiameters[index]);
        var bodyImg = bodyGO.AddComponent<Image>();
        bodyImg.sprite = ringSprite;
        bodyImg.raycastTarget = false;

        var markerRootGO = new GameObject("MarkerRoot");
        markerRootGO.transform.SetParent(go.transform, false);
        var markerRT = markerRootGO.AddComponent<RectTransform>();
        markerRT.anchorMin = new Vector2(0.5f, 0.5f);
        markerRT.anchorMax = new Vector2(0.5f, 0.5f);
        markerRT.pivot = new Vector2(0.5f, 0.5f);
        markerRT.anchoredPosition = Vector2.zero;
        markerRT.sizeDelta = Vector2.zero;

        var bgGO = new GameObject("SymbolBg");
        bgGO.transform.SetParent(markerRootGO.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0.5f, 0.5f);
        bgRT.anchorMax = new Vector2(0.5f, 0.5f);
        bgRT.pivot = new Vector2(0.5f, 0.5f);
        bgRT.anchoredPosition = Vector2.zero;
        bgRT.sizeDelta = new Vector2(46f, 46f);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.sprite = symbolBgSprite;
        bgImg.raycastTarget = false;

        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(bgGO.transform, false);
        var iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.12f, 0.12f);
        iconRT.anchorMax = new Vector2(0.88f, 0.88f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;

        var controller = go.AddComponent<RingController>();
        controller.Setup(bodyRT, bodyImg, glowImg, markerRT, bgRT, bgImg, iconImg);

        float speed = baseRotationSpeeds[index % baseRotationSpeeds.Length] * speedMult;
        controller.Init(speed, ringDiameters[index], symbol, symbolConfig, ringColors[index % ringColors.Length]);

        return controller;
    }

    private List<SymbolConfig.SymbolType> GetShuffledSymbols(int count)
    {
        var all = new List<SymbolConfig.SymbolType>
        {
            SymbolConfig.SymbolType.Spades,
            SymbolConfig.SymbolType.Hearts,
            SymbolConfig.SymbolType.Diamonds,
            SymbolConfig.SymbolType.Clubs
        };

        var result = new List<SymbolConfig.SymbolType>();
        var pool = new List<SymbolConfig.SymbolType>(all);

        for (int i = 0; i < count; i++)
        {
            if (pool.Count == 0)
                pool = new List<SymbolConfig.SymbolType>(all);
            int idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return result;
    }

    public void HighlightRing(int index, bool highlighted)
    {
        if (index < 0 || index >= activeRings.Count) return;
        activeRings[index].SetHighlighted(highlighted);
    }

    public void CaptureRing(int index, System.Action onComplete = null)
    {
        if (index < 0 || index >= activeRings.Count) return;
        activeRings[index].Capture(onComplete);
    }

    public void MissOnRing(int index)
    {
        if (index < 0 || index >= activeRings.Count) return;
        activeRings[index].RegisterMiss();
    }

    public void SpeedUpAllRings(float multiplier)
    {
        foreach (var ring in activeRings)
            ring.SpeedUp(multiplier);
    }

    public void ShrinkAllRings(System.Action onComplete = null)
    {
        if (ringsContainer == null) { onComplete?.Invoke(); return; }

        ringsContainer.DOKill();
        DOTween.Sequence()
            .Append(ringsContainer.DOScale(0.87f, 0.22f).SetEase(Ease.OutQuad))
            .Append(ringsContainer.DOScale(1f, 0.32f).SetEase(Ease.OutBack))
            .OnComplete(() =>
            {
                SpeedUpAllRings(1.15f);
                onComplete?.Invoke();
            });
    }

    public void PlayEntrance()
    {
        if (ringsContainer == null) return;
        ringsContainer.localScale = Vector3.one * 0.7f;
        var cg = ringsContainer.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        ringsContainer.DOScale(1f, 0.6f).SetEase(Ease.OutBack);
        if (cg != null) cg.DOFade(1f, 0.4f);
    }
}
