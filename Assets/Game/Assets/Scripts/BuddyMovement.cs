
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class BuddyMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [Min(0f)] public float moveSpeed = 5f;
    [Min(0f)] public float acceleration = 55f;
    [Min(0f)] public float deceleration = 70f;
    [Range(0f, 1f)] public float airControl = 0.8f;

    [Header("Jump")]
    [Min(0f)] public float jumpForce = 7f;
    [Min(0f)] public float coyoteTime = 0.12f;
    [Min(0f)] public float jumpBufferTime = 0.12f;
    [Range(0.05f, 1f)] public float jumpCutMultiplier = 0.45f;
    [Min(0f)] public float maxFallSpeed = 22f;

    [Header("Animation")]
    public Animator animator;

    private const string WalkingAnimation = "Buddy_Walking";
    private const string IdleAnimation = "Buddy_idle";

    private Rigidbody2D rb;
    private float moveInput;
    private float jumpBufferTimer;
    private float coyoteTimer;

    private bool jumpHeld;
    private bool jumpCutRequested;

    private string currentAnimation = "";

    private readonly ContactPoint2D[] contacts =
        new ContactPoint2D[16];

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        rb.interpolation =
            RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        PlayAnimation(IdleAnimation);
    }

    private void Update()
    {
        ReadInput();

        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        if (coyoteTimer > 0f)
            coyoteTimer -= Time.deltaTime;

        if (!jumpHeld && GetVelocity().y > 0.05f)
            jumpCutRequested = true;

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();

        if (grounded)
            coyoteTimer = coyoteTime;

        Vector2 velocity = GetVelocity();

        float targetSpeed = moveInput * moveSpeed;

        float control = grounded ? 1f : airControl;

        float rate = Mathf.Abs(targetSpeed) > 0.01f
            ? acceleration * control
            : deceleration * control;

        velocity.x = Mathf.MoveTowards(
            velocity.x,
            targetSpeed,
            rate * Time.fixedDeltaTime
        );

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            velocity.y = jumpForce;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            jumpCutRequested = false;
        }

        if (jumpCutRequested && velocity.y > 0f)
        {
            velocity.y *= jumpCutMultiplier;
            jumpCutRequested = false;
        }

        velocity.y = Mathf.Max(
            velocity.y,
            -maxFallSpeed
        );

        SetVelocity(velocity);
    }

    private void ReadInput()
    {
        moveInput = 0f;

        bool pressedJump = false;
        jumpHeld = false;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed ||
                keyboard.leftArrowKey.isPressed)
                moveInput -= 1f;

            if (keyboard.dKey.isPressed ||
                keyboard.rightArrowKey.isPressed)
                moveInput += 1f;

            pressedJump |=
                keyboard.spaceKey.wasPressedThisFrame ||
                keyboard.wKey.wasPressedThisFrame ||
                keyboard.upArrowKey.wasPressedThisFrame;

            jumpHeld |=
                keyboard.spaceKey.isPressed ||
                keyboard.wKey.isPressed ||
                keyboard.upArrowKey.isPressed;
        }

        Gamepad gamepad = Gamepad.current;

        if (gamepad != null)
        {
            float stick = gamepad.leftStick.ReadValue().x;

            if (Mathf.Abs(stick) > Mathf.Abs(moveInput))
                moveInput = stick;

            pressedJump |=
                gamepad.buttonSouth.wasPressedThisFrame;

            jumpHeld |=
                gamepad.buttonSouth.isPressed;
        }

#elif ENABLE_LEGACY_INPUT_MANAGER
        moveInput = Input.GetAxisRaw("Horizontal");

        pressedJump =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow);

        jumpHeld =
            Input.GetKey(KeyCode.Space) ||
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.UpArrow);
#endif

        moveInput = Mathf.Clamp(moveInput, -1f, 1f);

        if (pressedJump)
            jumpBufferTimer = jumpBufferTime;
    }

    private bool IsGrounded()
    {
        int count = rb.GetContacts(contacts);

        for (int i = 0; i < count; i++)
        {
            if (contacts[i].normal.y > 0.55f)
                return true;
        }

        return false;
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        bool isWalking = Mathf.Abs(moveInput) > 0.1f;

        if (isWalking)
        {
            PlayAnimation(WalkingAnimation);
        }
        else
        {
            PlayAnimation(IdleAnimation);
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (animator == null)
            return;

        // Don't restart an animation that is already active.
        if (currentAnimation == animationName)
            return;

        int stateHash = Animator.StringToHash(
            "Base Layer." + animationName
        );

        if (!animator.HasState(0, stateHash))
        {
            Debug.LogWarning(
                "Animator state not found: " + animationName,
                this
            );
            return;
        }

        currentAnimation = animationName;

        // Switch immediately and start at the first frame.
        animator.Play(stateHash, 0, 0f);
    }

    private Vector2 GetVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }

    private void SetVelocity(Vector2 velocity)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = velocity;
#else
        rb.velocity = velocity;
#endif
    }
}
