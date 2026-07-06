using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fill Animation")]
    public float fillAnimDuration = 0.25f;
    public AnimationCurve fillCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Fade Animation")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Hit Flash")]
    public Color flashColor = Color.yellow;
    public Color normalColor = Color.white;
    public float flashDuration = 0.2f;
    public AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;
    private Coroutine fadeRoutine;
    private Coroutine fillRoutine;
    private Coroutine flashRoutine;
    private float currentFill;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (fillImage == null) fillImage = GetComponentInChildren<Image>();
        if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>();

        if (fillImage != null)
        {
            currentFill = fillImage.fillAmount;
            fillImage.color = normalColor;
        }
    }

    public void SetFillImmediate(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        if (fillRoutine != null)
        {
            StopCoroutine(fillRoutine);
            fillRoutine = null;
        }

        currentFill = normalized;
        if (fillImage != null)
            fillImage.fillAmount = normalized;
    }

    public void AnimateFillTo(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        if (fillImage == null) return;

        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        if (fillAnimDuration <= 0f)
        {
            SetFillImmediate(normalized);
            return;
        }

        fillRoutine = StartCoroutine(FillRoutine(normalized));
    }

    private IEnumerator FillRoutine(float targetFill)
    {
        float startFill = currentFill;
        float elapsed = 0f;

        while (elapsed < fillAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = fillCurve.Evaluate(Mathf.Clamp01(elapsed / fillAnimDuration));
            currentFill = Mathf.Lerp(startFill, targetFill, t);
            fillImage.fillAmount = currentFill;
            yield return null;
        }

        currentFill = targetFill;
        fillImage.fillAmount = targetFill;
        fillRoutine = null;
    }

    public void Flash()
    {
        if (fillImage == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        fillImage.color = flashColor;

        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = flashCurve.Evaluate(Mathf.Clamp01(elapsed / flashDuration));
            fillImage.color = Color.Lerp(flashColor, normalColor, t);
            yield return null;
        }

        fillImage.color = normalColor;
        flashRoutine = null;
    }

    public void SetWorldPosition(Vector3 worldPos)
    {
        if (rectTransform != null)
            rectTransform.position = worldPos;
    }

    public void SetAlphaImmediate(float alpha)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = alpha;
    }

    public void FadeTo(float targetAlpha, float duration)
    {
        if (canvasGroup == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            return;
        }

        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = fadeCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        fadeRoutine = null;
    }
}