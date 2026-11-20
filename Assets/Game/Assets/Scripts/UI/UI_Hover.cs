
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform target;
    [SerializeField] private Graphic targetGraphic;

    [Header("Hover Transform")]
    [SerializeField] private Vector2 hoverScale = new Vector2(1.08f, 1.08f);
    [SerializeField] private float hoverRotation = 4f;
    [SerializeField] private Vector2 hoverOffset = Vector2.zero;

    [Header("Animation Settings")]
    [SerializeField] private float hoverDuration = 0.18f;
    [SerializeField] private float returnDuration = 0.22f;
    [SerializeField] private AnimationCurve animationCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Color Settings")]
    [SerializeField] private bool changeColor = false;
    [SerializeField] private Color hoverColor = Color.white;

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Vector2 originalPosition;
    private Color originalColor;

    private Coroutine animation;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        if (target == null)
        {
            enabled = false;
            return;
        }

        originalScale = target.localScale;
        originalRotation = target.localRotation;
        originalPosition = target.anchoredPosition;

        if (targetGraphic != null)
            originalColor = targetGraphic.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Animate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Animate(false);
    }

    private void Animate(bool hovering)
    {
        if (target == null)
            return;

        if (animation != null)
            StopCoroutine(animation);

        animation = StartCoroutine(AnimateHover(hovering));
    }

    private IEnumerator AnimateHover(bool hovering)
    {
        Vector3 startScale = target.localScale;
        Quaternion startRotation = target.localRotation;
        Vector2 startPosition = target.anchoredPosition;

        Color startColor = targetGraphic != null
            ? targetGraphic.color
            : Color.white;

        Vector3 endScale = hovering
            ? Vector3.Scale(
                originalScale,
                new Vector3(hoverScale.x, hoverScale.y, 1f)
            )
            : originalScale;

        Quaternion endRotation = hovering
            ? originalRotation * Quaternion.Euler(0f, 0f, hoverRotation)
            : originalRotation;

        Vector2 endPosition = hovering
            ? originalPosition + hoverOffset
            : originalPosition;

        Color endColor = hovering && changeColor
            ? hoverColor
            : originalColor;

        float duration = hovering ? hoverDuration : returnDuration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(duration, 0.001f)
            );

            float smooth = animationCurve.Evaluate(t);

            target.localScale = Vector3.LerpUnclamped(
                startScale, endScale, smooth
            );

            target.localRotation = Quaternion.SlerpUnclamped(
                startRotation, endRotation, smooth
            );

            target.anchoredPosition = Vector2.LerpUnclamped(
                startPosition, endPosition, smooth
            );

            if (changeColor && targetGraphic != null)
            {
                targetGraphic.color = Color.LerpUnclamped(
                    startColor, endColor, smooth
                );
            }

            yield return null;
        }

        target.localScale = endScale;
        target.localRotation = endRotation;
        target.anchoredPosition = endPosition;

        if (changeColor && targetGraphic != null)
            targetGraphic.color = endColor;

        animation = null;
    }

    private void OnDisable()
    {
        if (animation != null)
        {
            StopCoroutine(animation);
            animation = null;
        }

        if (target == null)
            return;

        target.localScale = originalScale;
        target.localRotation = originalRotation;
        target.anchoredPosition = originalPosition;

        if (changeColor && targetGraphic != null)
            targetGraphic.color = originalColor;
    }
}
