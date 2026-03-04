using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class FloatingBG : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int shapeCount = 18;
    [SerializeField] private float minSize = 20f;
    [SerializeField] private float maxSize = 80f;
    [SerializeField] private float minDrift = 15f;
    [SerializeField] private float maxDrift = 60f;
    [SerializeField] private float minAlpha = 0.03f;
    [SerializeField] private float maxAlpha = 0.12f;
    [SerializeField] private float minCycleDuration = 8f;
    [SerializeField] private float maxCycleDuration = 20f;

    private RectTransform container;
    private List<Image> shapes = new List<Image>();
    private Sprite glowSprite;

    private static readonly Color[] Palette =
    {
        new Color(0.4f, 0.5f, 1f),
        new Color(0.8f, 0.35f, 0.9f),
        new Color(0.3f, 0.85f, 0.6f),
        new Color(1f, 0.65f, 0.3f),
        new Color(0.9f, 0.3f, 0.5f),
        new Color(0.3f, 0.7f, 1f),
    };

    private void Start()
    {
        VFXFactory.EnsureGenerated();
        glowSprite = VFXFactory.CreateGlowSprite();

        container = GetComponent<RectTransform>();
        if (container == null)
        {
            container = gameObject.AddComponent<RectTransform>();
        }

        SpawnShapes();
    }

    private void SpawnShapes()
    {
        float w = container.rect.width > 0 ? container.rect.width : 1080f;
        float h = container.rect.height > 0 ? container.rect.height : 1920f;

        for (int i = 0; i < shapeCount; i++)
        {
            var go = new GameObject($"Shape_{i}");
            go.transform.SetParent(container, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            float size = Random.Range(minSize, maxSize);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(
                Random.Range(-w * 0.5f, w * 0.5f),
                Random.Range(-h * 0.5f, h * 0.5f));

            var img = go.AddComponent<Image>();
            img.sprite = glowSprite;
            img.raycastTarget = false;

            Color c = Palette[Random.Range(0, Palette.Length)];
            float alpha = Random.Range(minAlpha, maxAlpha);
            img.color = new Color(c.r, c.g, c.b, alpha);

            rt.localScale = Vector3.one * Random.Range(0.6f, 1.2f);

            shapes.Add(img);
            AnimateShape(rt, img, alpha);
        }
    }

    private void AnimateShape(RectTransform rt, Image img, float baseAlpha)
    {
        float duration = Random.Range(minCycleDuration, maxCycleDuration);
        float driftX = Random.Range(-maxDrift, maxDrift);
        float driftY = Random.Range(-maxDrift, maxDrift);
        float rot = Random.Range(-90f, 90f);
        float scaleTarget = Random.Range(0.7f, 1.4f);

        Vector2 startPos = rt.anchoredPosition;

        var seq = DOTween.Sequence();
        seq.Append(rt.DOAnchorPos(startPos + new Vector2(driftX, driftY), duration * 0.5f).SetEase(Ease.InOutSine));
        seq.Join(img.DOFade(baseAlpha * 1.5f, duration * 0.5f).SetEase(Ease.InOutSine));
        seq.Join(rt.DOScale(scaleTarget, duration * 0.5f).SetEase(Ease.InOutSine));
        seq.Join(rt.DOLocalRotate(new Vector3(0, 0, rot), duration * 0.5f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine));

        seq.Append(rt.DOAnchorPos(startPos, duration * 0.5f).SetEase(Ease.InOutSine));
        seq.Join(img.DOFade(baseAlpha, duration * 0.5f).SetEase(Ease.InOutSine));
        seq.Join(rt.DOScale(rt.localScale, duration * 0.5f).SetEase(Ease.InOutSine));
        seq.Join(rt.DOLocalRotate(new Vector3(0, 0, -rot * 0.5f), duration * 0.5f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine));

        seq.SetLoops(-1, LoopType.Restart);
    }

    private void OnDestroy()
    {
        foreach (var s in shapes)
        {
            if (s != null)
            {
                s.DOKill();
                s.rectTransform.DOKill();
            }
        }
    }
}
