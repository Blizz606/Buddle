
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class EyeTracking : MonoBehaviour
{
    [Header("Eye References")]
    [SerializeField] private RectTransform leftEye;
    [SerializeField] private RectTransform rightEye;

    [Header("Movement Settings")]
    [SerializeField] private Vector2 maxOffset = new Vector2(12f, 8f);
    [SerializeField] private float trackingDistance = 150f;

    [Header("Smoothing")]
    [SerializeField, Range(0.02f, 0.5f)]
    private float smoothTime = 0.15f;

    private Vector2 leftStart;
    private Vector2 rightStart;
    private Vector2 leftCenter;
    private Vector2 rightCenter;

    private Vector2 leftVelocity;
    private Vector2 rightVelocity;

    private RectTransform leftParent;
    private RectTransform rightParent;

    private Camera uiCamera;
    private bool initialized;

    private void Start()
    {
        if (leftEye == null || rightEye == null)
        {
            Debug.LogError("EyeTracking: Assign both eyes in the Inspector.");
            return;
        }

        leftParent = leftEye.parent as RectTransform;
        rightParent = rightEye.parent as RectTransform;

        if (leftParent == null || rightParent == null)
            return;

        leftStart = leftEye.anchoredPosition;
        rightStart = rightEye.anchoredPosition;

        leftCenter = leftParent.InverseTransformPoint(leftEye.position);
        rightCenter = rightParent.InverseTransformPoint(rightEye.position);

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.rootCanvas.worldCamera != null
                ? canvas.rootCanvas.worldCamera
                : Camera.main;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null) return;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
#else
        Vector2 mousePosition = Input.mousePosition;
#endif

        float deltaTime = Time.unscaledDeltaTime;

        if (deltaTime <= 0f) return;

        MoveEye(
            leftEye,
            leftParent,
            leftStart,
            leftCenter,
            mousePosition,
            ref leftVelocity,
            deltaTime
        );

        MoveEye(
            rightEye,
            rightParent,
            rightStart,
            rightCenter,
            mousePosition,
            ref rightVelocity,
            deltaTime
        );
    }

    private void MoveEye(
        RectTransform eye,
        RectTransform parent,
        Vector2 start,
        Vector2 center,
        Vector2 mousePosition,
        ref Vector2 velocity,
        float deltaTime)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            mousePosition,
            uiCamera,
            out Vector2 localMouse))
            return;

        Vector2 direction = localMouse - center;

        Vector2 normalized = Vector2.ClampMagnitude(
            direction / Mathf.Max(1f, trackingDistance),
            1f
        );

        Vector2 target = start + Vector2.Scale(normalized, maxOffset);

        eye.anchoredPosition = Vector2.SmoothDamp(
            eye.anchoredPosition,
            target,
            ref velocity,
            smoothTime,
            Mathf.Infinity,
            deltaTime
        );
    }
}
