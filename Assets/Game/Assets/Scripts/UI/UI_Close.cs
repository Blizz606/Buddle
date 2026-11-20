
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIShrink : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button triggerButton;
    [SerializeField] private RectTransform target;

    [Header("Animation")]
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private AnimationCurve shrinkCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private Coroutine animation;

    private void Awake()
    {
        if (target == null) return;

        originalScale = target.localScale;

        canvasGroup = target.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = target.gameObject.AddComponent<CanvasGroup>();

        if (triggerButton != null)
            triggerButton.onClick.AddListener(Hide);
    }

    public void Hide()
    {
        if (target == null || !target.gameObject.activeSelf)
            return;

        if (animation != null)
            StopCoroutine(animation);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        animation = StartCoroutine(Shrink());
    }

    private IEnumerator Shrink()
    {
        float elapsed = 0f;
        Vector3 startScale = target.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.001f));
            float eased = shrinkCurve.Evaluate(t);

            target.localScale = Vector3.LerpUnclamped(
                startScale,
                Vector3.zero,
                eased
            );

            yield return null;
        }

        target.gameObject.SetActive(false);
        target.localScale = originalScale;

        animation = null;
    }

    public void Show()
    {
        if (target == null) return;

        if (animation != null)
            StopCoroutine(animation);

        target.localScale = originalScale;
        target.gameObject.SetActive(true);

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        animation = null;
    }

    private void OnDestroy()
    {
        if (triggerButton != null)
            triggerButton.onClick.RemoveListener(Hide);
    }
}
