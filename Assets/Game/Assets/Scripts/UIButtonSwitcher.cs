
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public class ButtonAction
    {
        public string name;
        public Button button;
        public GameObject[] hideElements;
        public GameObject[] showElements;
    }

    [Header("Button Actions")]
    [SerializeField] private ButtonAction[] actions;

    [Header("Initially Hidden")]
    [SerializeField] private GameObject[] initiallyHidden;

    [Header("Animation Settings")]
    [SerializeField, Min(0.01f)]
    private float duration = 0.25f;

    [SerializeField, Range(0.5f, 1f)]
    private float hiddenScale = 0.9f;

    [SerializeField]
    private AnimationCurve easing =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private readonly Dictionary<GameObject, Vector3> originalScales = new();
    private readonly Dictionary<GameObject, Coroutine> animations = new();
    private readonly Dictionary<Button, UnityAction> listeners = new();

    private void Awake()
    {
        foreach (ButtonAction action in actions)
        {
            foreach (GameObject obj in action.hideElements)
                Register(obj);

            foreach (GameObject obj in action.showElements)
                Register(obj);
        }

        foreach (GameObject obj in initiallyHidden)
        {
            Register(obj);

            if (obj == null) continue;

            CanvasGroup group = GetGroup(obj);

            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            obj.transform.localScale =
                originalScales[obj] * hiddenScale;

            obj.SetActive(false);
        }

        foreach (ButtonAction action in actions)
        {
            if (action.button == null) continue;

            ButtonAction current = action;
            UnityAction callback = () => Execute(current);

            current.button.onClick.AddListener(callback);

            if (!listeners.ContainsKey(current.button))
                listeners.Add(current.button, callback);
        }
    }

    private void Register(GameObject obj)
    {
        if (obj == null || originalScales.ContainsKey(obj))
            return;

        originalScales.Add(obj, obj.transform.localScale);
        GetGroup(obj);
    }

    private CanvasGroup GetGroup(GameObject obj)
    {
        CanvasGroup group = obj.GetComponent<CanvasGroup>();

        if (group == null)
            group = obj.AddComponent<CanvasGroup>();

        return group;
    }

    private void Execute(ButtonAction action)
    {
        foreach (GameObject obj in action.hideElements)
            Hide(obj);

        foreach (GameObject obj in action.showElements)
            Show(obj);
    }

    public void Show(GameObject obj)
    {
        Animate(obj, true);
    }

    public void Hide(GameObject obj)
    {
        Animate(obj, false);
    }

    private void Animate(GameObject obj, bool show)
    {
        if (obj == null) return;

        Register(obj);

        if (animations.TryGetValue(obj, out Coroutine running))
        {
            StopCoroutine(running);
            animations.Remove(obj);
        }

        CanvasGroup group = GetGroup(obj);

        if (show && !obj.activeSelf)
        {
            obj.SetActive(true);
            group.alpha = 0f;

            obj.transform.localScale =
                originalScales[obj] * hiddenScale;
        }

        if (!obj.activeInHierarchy)
            return;

        group.interactable = false;
        group.blocksRaycasts = false;

        animations[obj] = StartCoroutine(
            AnimatePanel(obj, group, show)
        );
    }

    private IEnumerator AnimatePanel(
        GameObject obj,
        CanvasGroup group,
        bool show)
    {
        float startAlpha = group.alpha;
        float endAlpha = show ? 1f : 0f;

        Vector3 startScale = obj.transform.localScale;
        Vector3 endScale = originalScales[obj] *
            (show ? 1f : hiddenScale);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = easing.Evaluate(t);

            group.alpha = Mathf.Lerp(
                startAlpha, endAlpha, smooth
            );

            obj.transform.localScale = Vector3.Lerp(
                startScale, endScale, smooth
            );

            yield return null;
        }

        group.alpha = endAlpha;
        obj.transform.localScale = endScale;

        if (show)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        else
        {
            obj.SetActive(false);
        }

        animations.Remove(obj);
    }

    private void OnDestroy()
    {
        foreach (ButtonAction action in actions)
        {
            if (action.button == null) continue;

            if (listeners.TryGetValue(action.button, out UnityAction callback))
                action.button.onClick.RemoveListener(callback);
        }
    }
}
