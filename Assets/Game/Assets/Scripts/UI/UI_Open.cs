
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIShow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button triggerButton;
    [SerializeField] private RectTransform target;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.25f;

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
            triggerButton.onClick.AddListener(Show);
    }

    public void Show()
    {
        if (target == null || animation != null)
            return;

        target.localScale = Vector3.zero;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        target.gameObject.SetActive(true);

        animation = StartCoroutine(AnimateShow());
    }

    private IEnumerator AnimateShow()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(duration, 0.001f)
            );

            float smooth = 1f - Mathf.Pow(1f - t, 3f);

            target.localScale = Vector3.LerpUnclamped(
                Vector3.zero,
                originalScale,
                smooth
            );

            canvasGroup.alpha = smooth;

            yield return null;
        }

        target.localScale = originalScale;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        animation = null;
    }

    private void OnDestroy()
    {
        if (triggerButton != null)
            triggerButton.onClick.RemoveListener(Show);
    }
}
